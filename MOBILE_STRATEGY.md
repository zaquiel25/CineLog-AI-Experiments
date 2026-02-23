# CineLog Mobile Strategy - PWA Implementation Guide

## Goal

Allow users to "install" CineLog on their phone like a native app, without App Store or Play Store distribution. PWA (Progressive Web App) is the chosen approach.

## Why PWA?

| Approach | Timeline | Cost | Code Reuse | Recommendation |
|----------|----------|------|------------|----------------|
| **PWA** | ~1 week | $0 | 100% | **Start here** |
| Capacitor | 2-3 weeks | Low | 95% | If PWA succeeds and stores needed |
| .NET MAUI | 6+ months | High | 50-60% | Only if massive scale required |

**Key benefits**: Zero distribution friction, zero cost, instant updates via Azure deploy, leverages 100% of existing ASP.NET Core codebase. Can wrap with Capacitor later for app store presence if needed.

---

## Current State (What Already Exists)

The project already has several PWA foundations in place:

| Element | Status | Location |
|---------|--------|----------|
| Web manifest | Exists | `wwwroot/site.webmanifest` |
| Manifest link in layout | Exists | `Views/Shared/_Layout.cshtml` line 11 |
| Icon 192x192 | Exists | `wwwroot/android-chrome-192x192.png` |
| Icon 512x512 | Exists | `wwwroot/android-chrome-512x512.png` |
| Apple touch icon | Exists | `wwwroot/apple-touch-icon.png` |
| Favicons | Exists | `wwwroot/favicon.ico`, `favicon-16x16.png`, `favicon-32x32.png` |
| Theme color (#F4D03F) | Exists | In `site.webmanifest` |
| Display standalone | Exists | In `site.webmanifest` |
| Service worker | **Missing** | Needs `wwwroot/service-worker.js` |
| SW registration | **Missing** | Needs script in `_Layout.cshtml` |
| iOS meta tags | **Missing** | Needs tags in `<head>` of `_Layout.cshtml` |
| Offline fallback page | **Missing** | Needs view + controller action |

---

## Implementation Steps

### Step 1: Enrich the Web Manifest (5 min)

The current `wwwroot/site.webmanifest` is minimal. Add `description`, `start_url`, `orientation`, and `purpose` to icons:

```json
{
  "name": "CineLog - Your Movie Journal",
  "short_name": "CineLog",
  "description": "Track, rate, and discover your favorite movies",
  "start_url": "/",
  "display": "standalone",
  "orientation": "portrait",
  "theme_color": "#F4D03F",
  "background_color": "#212529",
  "icons": [
    {
      "src": "/android-chrome-192x192.png",
      "sizes": "192x192",
      "type": "image/png",
      "purpose": "any maskable"
    },
    {
      "src": "/android-chrome-512x512.png",
      "sizes": "512x512",
      "type": "image/png",
      "purpose": "any maskable"
    }
  ]
}
```

### Step 2: Add iOS Meta Tags to Layout (5 min)

In `Views/Shared/_Layout.cshtml`, add after the existing manifest link (line 11):

```html
<meta name="theme-color" content="#F4D03F">
<meta name="apple-mobile-web-app-capable" content="yes">
<meta name="apple-mobile-web-app-status-bar-style" content="black-translucent">
<meta name="apple-mobile-web-app-title" content="CineLog">
```

Note: `apple-touch-icon` link already exists at line 8.

### Step 3: Create Service Worker (core of PWA)

Create `wwwroot/service-worker.js` with three caching strategies:

```javascript
/**
 * CineLog Service Worker
 * Client-side caching that complements the existing server-side IMemoryCache.
 * Server handles: TMDB API data (24h), user blacklist/wishlist (15min)
 * Client handles: TMDB posters, static assets, offline navigation
 */

const STATIC_CACHE = 'cinelog-static-v1';
const POSTER_CACHE = 'cinelog-posters-v1';

const STATIC_ASSETS = [
  '/',
  '/css/site.css',
  '/css/spacing-enhancements.css',
  '/css/vertical-centering.css',
  '/css/bootstrap.min.css',
  '/js/site.js',
  '/lib/bootstrap/dist/js/bootstrap.bundle.min.js',
  '/lib/jquery/dist/jquery.min.js',
  '/android-chrome-192x192.png',
  '/favicon.ico'
];

// Install: cache static assets
self.addEventListener('install', event => {
  event.waitUntil(
    caches.open(STATIC_CACHE)
      .then(cache => cache.addAll(STATIC_ASSETS))
      .then(() => self.skipWaiting())
  );
});

// Activate: clean old caches
self.addEventListener('activate', event => {
  event.waitUntil(
    caches.keys().then(names =>
      Promise.all(
        names
          .filter(n => n.startsWith('cinelog-') && n !== STATIC_CACHE && n !== POSTER_CACHE)
          .map(n => caches.delete(n))
      )
    ).then(() => self.clients.claim())
  );
});

// Fetch: route requests to appropriate cache strategy
self.addEventListener('fetch', event => {
  const url = new URL(event.request.url);

  // TMDB Posters: Cache-First (images don't change)
  if (url.hostname.includes('image.tmdb.org')) {
    event.respondWith(
      caches.open(POSTER_CACHE).then(cache =>
        cache.match(event.request).then(cached => {
          if (cached) return cached;
          return fetch(event.request).then(response => {
            if (response.ok) cache.put(event.request, response.clone());
            return response;
          });
        })
      )
    );
    return;
  }

  // Static assets: Cache-First
  if (event.request.destination === 'style' ||
      event.request.destination === 'script' ||
      event.request.destination === 'font') {
    event.respondWith(
      caches.match(event.request).then(cached =>
        cached || fetch(event.request)
      )
    );
    return;
  }

  // Navigation: Network-First with offline fallback
  if (event.request.mode === 'navigate') {
    event.respondWith(
      fetch(event.request).catch(() => caches.match('/Home/Offline'))
    );
    return;
  }

  // Everything else: Network-First
  event.respondWith(
    fetch(event.request).catch(() => caches.match(event.request))
  );
});

// Handle cache clear messages from app
self.addEventListener('message', event => {
  if (event.data.action === 'clearCache') {
    event.waitUntil(
      caches.keys().then(names =>
        Promise.all(names.map(n => caches.delete(n)))
      )
    );
  }
});
```

### Step 4: Register Service Worker in Layout

In `Views/Shared/_Layout.cshtml`, add before the closing `</body>` tag (after the existing scripts):

```html
<!-- PWA Service Worker -->
<script>
if ('serviceWorker' in navigator) {
  window.addEventListener('load', () => {
    navigator.serviceWorker.register('/service-worker.js')
      .then(reg => console.log('SW registered:', reg.scope))
      .catch(err => console.log('SW registration failed:', err));
  });
}
</script>
```

### Step 5: Create Offline Fallback Page

**View** - `Views/Home/Offline.cshtml`:

```cshtml
@{
    ViewData["Title"] = "Offline";
}

<div class="text-center" style="padding: 80px 20px;">
    <h1 class="display-1">Offline</h1>
    <p class="lead">CineLog needs an internet connection to load new content.</p>
    <p>Previously viewed movies may still be available.</p>
    <a asp-controller="Movies" asp-action="Index" class="btn btn-warning mt-3">
        Try My Movies
    </a>
</div>
```

**Controller action** - Add to `Controllers/HomeController.cs`:

```csharp
/// <summary>
/// FEATURE: Offline fallback page for PWA service worker.
/// </summary>
public IActionResult Offline()
{
    return View();
}
```

### Step 6: Verify with Lighthouse

```
1. Run the app: dotnet run
2. Open Chrome at https://localhost:7186
3. DevTools (F12) > Lighthouse tab
4. Check "Progressive Web App"
5. Generate report
6. Target: Score >= 90/100
```

---

## Cache Architecture (Dual-Layer)

CineLog uses server-side caching already. The PWA service worker adds a complementary client-side layer:

```
User Device (Browser/PWA)
  PWA Service Worker Cache (NEW)
  - TMDB Posters: Cache-First (instant load after first view)
  - Static assets (CSS/JS): Cache-First
  - Navigation: Network-First with offline fallback
          |
          v  HTTP Request
Azure Server
  IMemoryCache (EXISTING)
  - TMDB API responses: 24h TTL
  - User blacklist/wishlist: 15min TTL
  - Director details: 24h TTL
          |
          v  API Call (only when both caches miss)
TMDB API (External)
```

**No conflicts**: Server uses C# cache keys (`movie_details_550`), client uses URLs (`https://cinelog-app.../Movies/Details/550`). Completely separate storage systems.

**Performance impact**: Poster images go from ~150-300ms to ~5-10ms after first load. Offline navigation becomes possible.

---

## iOS Considerations

iOS Safari does NOT show an automatic "Add to Home Screen" prompt like Android Chrome. Users must manually: **Share button > Add to Home Screen**.

The meta tags from Step 2 ensure the app looks native once installed (standalone mode, no Safari UI, proper status bar).

iOS cache limit is ~50MB. With poster images averaging ~100KB, this supports ~200-300 cached posters before eviction is needed. For most users this is more than enough.

---

## Rollback Plan

If the service worker causes issues in production:

1. **Quick fix**: Replace `wwwroot/service-worker.js` with a kill switch version:

```javascript
// Emergency: clear all caches and pass through all requests
self.addEventListener('install', () => self.skipWaiting());
self.addEventListener('activate', event => {
  event.waitUntil(
    caches.keys().then(names =>
      Promise.all(names.map(n => caches.delete(n)))
    ).then(() => self.clients.claim())
  );
});
self.addEventListener('fetch', event => {
  event.respondWith(fetch(event.request));
});
```

2. **Full removal**: Remove SW registration script from layout, remove manifest link meta tags, deploy. The kill switch SW will clean up client caches on next visit.

---

## Testing Checklist

- [ ] `dotnet build` succeeds with 0 errors
- [ ] Lighthouse PWA score >= 90/100
- [ ] App installable on Android Chrome (install prompt appears)
- [ ] App installable on iOS Safari (manual: Share > Add to Home Screen)
- [ ] App opens in standalone mode (no browser UI)
- [ ] TMDB posters load from cache on second visit (check Network tab)
- [ ] Offline mode shows fallback page (DevTools > Application > Service Workers > Offline)
- [ ] Icon displays correctly on home screen

## Files Modified/Created

| File | Action |
|------|--------|
| `wwwroot/site.webmanifest` | Modified (enriched) |
| `Views/Shared/_Layout.cshtml` | Modified (iOS meta tags + SW registration) |
| `wwwroot/service-worker.js` | **Created** |
| `Views/Home/Offline.cshtml` | **Created** |
| `Controllers/HomeController.cs` | Modified (Offline action) |

---

## Future Options (if needed)

- **Capacitor**: Wraps the PWA for App Store/Play Store distribution. Minimal code changes, adds store presence. Consider if install rate > 10% and users request it.
- **.NET MAUI**: Full native rewrite. Only if 100K+ users and PWA/Capacitor can't meet performance needs.

---

*Document updated: February 2026*
*Original: October 2025 (v1.0 - comprehensive plan)*
*Current: February 2026 (v2.0 - lean implementation guide)*
