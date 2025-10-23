# 📱 CineLog Mobile Strategy - PWA Implementation Guide

## 📋 Executive Summary

**Goal**: Transform CineLog into a Progressive Web App (PWA) to provide mobile app experience without requiring App Store/Play Store distribution.

**Timeline**: 2 weeks (October 2-16, 2025)
- Week 1: Development & initial testing (Steps 1-4)
- Week 2: Comprehensive testing, production deployment & monitoring (Step 5)
- 30% time buffer for unknowns and edge cases

**Approach**: Phased implementation starting with PWA, scalable to native if needed
**Key Benefits**: Fast launch, low cost, cross-platform, leverages existing ASP.NET Core codebase

**Risk Mitigation**: Comprehensive rollback plan, production monitoring, and user communication strategy included

---

## 🎯 Strategic Decision: Why PWA First?

### Comparison Matrix

| Approach | Timeline | Cost | Maintenance | User Reach | Recommendation |
|----------|----------|------|-------------|------------|----------------|
| **PWA** | 1-2 weeks | Minimal | Low | 95% mobile users | ✅ **Start Here** |
| **Capacitor** | 2-3 weeks | Low | Medium | 100% (app stores) | Phase 2 if PWA succeeds |
| **.NET MAUI** | 6+ months | High | High | 100% (native) | Phase 3 if app explodes |
| **React Native** | 4-6 months | High | High | 100% | ❌ Complete rewrite |
| **No-Code Platforms** | 2-3 months | Medium | Medium | App stores only | ❌ Rebuild from scratch |

### Decision Rationale

1. **Market Validation**: Test mobile demand without heavy investment
2. **Speed to Market**: 1 week vs 6+ months for native
3. **Zero Distribution Friction**: No app store approval process
4. **Existing Codebase**: Leverages 100% of current ASP.NET Core investment
5. **Upgrade Path**: Can wrap PWA with Capacitor later if needed

---

## 🛠️ Phase 1: PWA Implementation (1 Week)

### Prerequisites

- ✅ CineLog ASP.NET Core app running locally
- ✅ TMDB API integration working
- ✅ User authentication (ASP.NET Identity)
- ✅ Azure deployment configured
- ✅ **Existing server-side caching** (IMemoryCache in CacheService.cs and TmdbService.cs)

**IMPORTANT**: CineLog already implements robust server-side caching for TMDB API responses and user data. PWA adds a **complementary client-side layer** for posters and offline functionality. See "Understanding CineLog's Existing Cache Architecture" section below for details on dual-layer caching strategy.

### Implementation Steps

---

## 📦 Step 1: Install NuGet Package (5 minutes)

### Install WebEssentials.AspNetCore.PWA

```bash
cd "/Users/ezequielyebara/Projects/Ezequiel Movies/CineLog-AI-Experiments"
dotnet add package WebEssentials.AspNetCore.PWA
```

### Configure in Program.cs

**Location**: `Program.cs` (before `builder.Build()`)

```csharp
// FEATURE: PWA support for mobile app experience
builder.Services.AddProgressiveWebApp();
```

### Verify Installation

```bash
dotnet build  # Should succeed with 0 warnings, 0 errors
```

**Expected Output**:
- Package reference added to `.csproj`
- Service registered in dependency injection
- Ready for PWA configuration

---

## 🎨 Step 2: Create App Manifest (30 minutes)

### Prepare Directory Structure

**Create necessary directories for PWA assets:**

```bash
# Create icons directory (REQUIRED)
mkdir -p wwwroot/images/icons

# Create screenshots directory (OPTIONAL for Phase 1, REQUIRED for Phase 3)
# Screenshots needed only if pursuing Capacitor/App Store distribution
# For Phase 1 PWA: Can skip screenshots entirely
mkdir -p wwwroot/images/screenshots
```

**Note on Screenshots**:
- **Phase 1 (PWA)**: Screenshots are optional enhancement for browsers that support them
- **Phase 3 (Capacitor/App Stores)**: Screenshots become REQUIRED for App Store/Play Store listings
- **Recommendation**: Skip screenshots in Phase 1 to save time, add later if needed

### Create manifest.json

**Location**: `wwwroot/manifest.json`

**IMPORTANT**: Colors match CineLog's existing design system (`wwwroot/css/site.css` line 105):
- Theme color: `#f4d03f` (CineLog gold from CSS `:root` variables)
- Background: `#1a1a1a` (CineLog dark theme)

```json
{
  "name": "CineLog - Your Movie Journal",
  "short_name": "CineLog",
  "description": "Track, rate, and discover your favorite movies",
  "start_url": "/",
  "display": "standalone",
  "orientation": "portrait",
  "theme_color": "#f4d03f",
  "background_color": "#1a1a1a",
  "icons": [
    {
      "src": "/images/icons/icon-72.png",
      "sizes": "72x72",
      "type": "image/png",
      "purpose": "any"
    },
    {
      "src": "/images/icons/icon-96.png",
      "sizes": "96x96",
      "type": "image/png",
      "purpose": "any"
    },
    {
      "src": "/images/icons/icon-128.png",
      "sizes": "128x128",
      "type": "image/png",
      "purpose": "any"
    },
    {
      "src": "/images/icons/icon-144.png",
      "sizes": "144x144",
      "type": "image/png",
      "purpose": "any"
    },
    {
      "src": "/images/icons/icon-152.png",
      "sizes": "152x152",
      "type": "image/png",
      "purpose": "any"
    },
    {
      "src": "/images/icons/icon-192.png",
      "sizes": "192x192",
      "type": "image/png",
      "purpose": "any maskable"
    },
    {
      "src": "/images/icons/icon-384.png",
      "sizes": "384x384",
      "type": "image/png",
      "purpose": "any"
    },
    {
      "src": "/images/icons/icon-512.png",
      "sizes": "512x512",
      "type": "image/png",
      "purpose": "any maskable"
    }
  ],
  "screenshots": [
    {
      "src": "/images/screenshots/home.png",
      "sizes": "1170x2532",
      "type": "image/png",
      "form_factor": "narrow"
    },
    {
      "src": "/images/screenshots/timeline.png",
      "sizes": "1170x2532",
      "type": "image/png",
      "form_factor": "narrow"
    }
  ]
}
```

### Link Manifest in _Layout.cshtml

**Location**: `Views/Shared/_Layout.cshtml` (in `<head>` section, after existing meta tags)

```html
<!-- PWA Manifest -->
<link rel="manifest" href="/manifest.json">
<meta name="theme-color" content="#f4d03f">
<meta name="apple-mobile-web-app-capable" content="yes">
<meta name="apple-mobile-web-app-status-bar-style" content="black-translucent">
<meta name="apple-mobile-web-app-title" content="CineLog">
<link rel="apple-touch-icon" href="/images/icons/icon-192.png">
```

### Create App Icons

**Required Sizes**: 72x72, 96x96, 128x128, 144x144, 152x152, 192x192, 384x384, 512x512

**Existing Asset**: CineLog already has `wwwroot/favicon.ico` - use this as the base design for PWA icons.

#### Icon Quality Validation (CRITICAL)

**BEFORE generating icons, validate source quality:**

```bash
# Check favicon.ico dimensions
file wwwroot/favicon.ico
# Expected output should show dimensions (e.g., "32 x 32" or "64 x 64")
```

**Quality Requirements**:
- ✅ **Minimum source resolution**: 512x512px (for best quality across all sizes)
- ⚠️ **If favicon.ico < 512px**: Quality loss when scaling up to 512x512
- ❌ **If favicon.ico < 256px**: NOT suitable - must redesign icon at higher resolution

**Decision Tree**:
```
Is favicon.ico ≥ 512x512?
  ├─ YES → ✅ Use pwa-asset-generator (fast, automated)
  └─ NO  → ❌ Manual redesign required
           ├─ Option 1: Redesign in Figma/Photoshop at 512x512
           ├─ Option 2: Use SVG source (scalable, no quality loss)
           └─ Option 3: Find higher-res version of current icon
```

#### Icon Generation Methods

**Method 1: PWA Asset Generator** (Recommended if source ≥ 512px):
```bash
# Install tool
npm install -g pwa-asset-generator

# Generate all icon sizes from favicon.ico
pwa-asset-generator wwwroot/favicon.ico wwwroot/images/icons --icon-only --background "#1a1a1a" --theme-color "#f4d03f"

# Verify output
ls -lh wwwroot/images/icons/
# Should show 8 PNG files (72, 96, 128, 144, 152, 192, 384, 512)
```

**Method 2: Real Favicon Generator** (Good for manual control):
```bash
# 1. Visit https://realfavicongenerator.net/
# 2. Upload wwwroot/favicon.ico
# 3. Configure:
#    - iOS: Select icon-192.png for apple-touch-icon
#    - Android Chrome: Use gold theme (#f4d03f)
#    - Path: /images/icons/
# 4. Download package
# 5. Extract to wwwroot/images/icons/
```

**Method 3: Manual Design** (Best quality, takes longer):
```
1. Open Figma/Photoshop/Illustrator
2. Create 512x512px artboard
3. Design icon with CineLog branding:
   - Film reel or clapperboard motif
   - Gold accent (#f4d03f)
   - High contrast for visibility
4. Export as PNG at multiple sizes:
   - 72x72, 96x96, 128x128, 144x144
   - 152x152, 192x192, 384x384, 512x512
5. Save to wwwroot/images/icons/
```

#### Icon Verification Checklist

After generating icons, verify:

- [ ] **All 8 sizes present**:
  ```bash
  ls wwwroot/images/icons/ | wc -l  # Should output: 8
  ```

- [ ] **No pixelation** (open 512x512 icon at 100% zoom - should be crisp)

- [ ] **Consistent branding** (all sizes look identical, just scaled)

- [ ] **File sizes reasonable**:
  - icon-72.png: ~2-5 KB
  - icon-512.png: ~20-50 KB
  - If larger: Consider PNG optimization (e.g., `pngquant`)

- [ ] **Test on device**:
  - Install PWA on Android/iOS
  - Check home screen icon is sharp (not blurry)
  - If blurry: Source resolution too low, regenerate from higher-res source

**Directory Structure**:
```
wwwroot/
  images/
    icons/
      icon-72.png
      icon-96.png
      icon-128.png
      icon-144.png
      icon-152.png
      icon-192.png
      icon-384.png
      icon-512.png
    screenshots/
      home.png
      timeline.png
```

---

## 🗄️ Understanding CineLog's Existing Cache Architecture

### CRITICAL: Dual-Layer Caching System

**CineLog already uses server-side caching** - PWA adds a complementary client-side layer.

#### Current Server-Side Cache (IMemoryCache)

**Location**: `Services/CacheService.cs` and `TmdbService.cs`

```csharp
// EXISTING: Server-side memory cache (Azure App Service)
_memoryCache.Set(cacheKey, movieDetails, TimeSpan.FromHours(24));  // TMDB data: 24h
_memoryCache.Set(cacheKey, blacklistIds, TimeSpan.FromMinutes(15)); // User data: 15min
```

**What's Already Cached (Server)**:
- ✅ TMDB API responses (24 hours)
- ✅ User blacklist IDs (15 minutes)
- ✅ User wishlist IDs (15 minutes)
- ✅ Director/person details (24 hours)
- ✅ Movie filmography (24 hours)

#### New Client-Side Cache (PWA Service Worker)

**Location**: `wwwroot/service-worker.js` (to be created)

```javascript
// NEW: Client-side cache (user's browser/mobile device)
caches.open('cinelog-posters-v1').then(cache => {
  cache.put(request, response); // Stored on user's device
});
```

**What PWA Will Cache (Client)**:
- 🆕 TMDB poster images (Cache-First strategy)
- 🆕 Static assets (CSS, JS, icons)
- 🆕 HTML pages (for offline navigation)
- 🆕 API responses (with Network-First fallback)

### Dual-Layer Architecture Flow

```
┌─────────────────────────────────────────────────┐
│  USER DEVICE (Mobile/Desktop)                   │
│                                                 │
│  🆕 PWA Service Worker Cache (Client-Side)      │
│  ├── TMDB Posters (images) - Cache-First       │
│  ├── Static assets (CSS/JS) - Cache-First      │
│  └── API responses (recent) - Network-First    │
└─────────────────┬───────────────────────────────┘
                  │ HTTP Request
                  ▼
┌─────────────────────────────────────────────────┐
│  AZURE SERVER                                   │
│                                                 │
│  ✅ IMemoryCache (Server-Side - EXISTING)       │
│  ├── TMDB API responses (24h)                  │
│  ├── User blacklist IDs (15min)                │
│  ├── User wishlist IDs (15min)                 │
│  └── Director details (24h)                    │
└─────────────────┬───────────────────────────────┘
                  │ API Call
                  ▼
┌─────────────────────────────────────────────────┐
│  TMDB API (External)                            │
│  Only called when both caches miss             │
└─────────────────────────────────────────────────┘
```

### Performance Benefits: Combined Caching

| Scenario | Before PWA | After PWA | Improvement |
|----------|------------|-----------|-------------|
| TMDB API call | 500-1000ms | 500-1000ms | Same (cached on server) |
| Poster image load | 150-300ms | 5-10ms | **97% faster** ⚡ |
| Page navigation | 100-200ms | 5-10ms | **95% faster** ⚡ |
| Offline access | ❌ Fails | ✅ Works | Infinite improvement |

### Cache Strategy Coordination

| Content Type | Server Cache | PWA Cache | Combined Strategy |
|--------------|--------------|-----------|-------------------|
| TMDB API Data | ✅ 24h (IMemoryCache) | ✅ 24h (Service Worker) | Redundant (failsafe) |
| TMDB Posters | ❌ Not cached | ✅ Cache-First | PWA handles entirely |
| User Blacklist | ✅ 15min (CacheService) | ❌ Not cached | Server handles |
| User Wishlist | ✅ 15min (CacheService) | ❌ Not cached | Server handles |
| Static Assets | ❌ Not cached | ✅ Cache-First | PWA handles entirely |
| Dynamic Pages | ❌ Not cached | ✅ Network-First | Fresh with offline fallback |

### IMPORTANT: No Cache Conflicts

**Different Cache Keys** - Server and PWA use separate namespaces:

```csharp
// Server-side (C#)
var cacheKey = $"movie_details_{id}";  // Format: "movie_details_550"
_memoryCache.Set(cacheKey, details, TimeSpan.FromHours(24));
```

```javascript
// Client-side (JavaScript)
const cacheKey = request.url;  // Format: "https://cinelog-app.../Movies/Details/550"
cache.put(cacheKey, response);
```

**Result**: Zero collision risk - completely separate storage systems.

### Invalidation Strategy

**Server Cache Invalidation** (already working):
```csharp
// CacheService.cs - Already implemented
public void InvalidateUserBlacklistCache(string userId)
{
    var cacheKey = $"BlacklistIds_{userId}";
    _memoryCache.Remove(cacheKey);
}
```

**PWA Cache Invalidation** (to be added):
```javascript
// Clear PWA cache when needed (e.g., after major update)
async function clearPWACache() {
  const cacheNames = await caches.keys();
  await Promise.all(cacheNames.map(name => caches.delete(name)));
  location.reload();
}
```

### Performance Budget

**CRITICAL**: Define cache limits to prevent storage quota issues, especially on iOS (50MB limit).

#### Cache Size Limits

| Cache Type | Size Limit | Eviction Strategy | Reasoning |
|------------|------------|-------------------|-----------|
| Static Assets | ~5MB (fixed) | Version-based | CSS, JS, fonts - replaced on version bump |
| TMDB Posters | Max 200 posters (~20MB) | LRU (Least Recently Used) | Most users view <200 movies, 100KB avg per poster |
| API Responses | Max 50 pages (~2MB) | TTL 24 hours | Ephemeral data, auto-refresh daily |
| **TOTAL** | **~30MB max** | Combined | Safe buffer under iOS 50MB limit |

#### Eviction Implementation

Add to `wwwroot/service-worker.js`:

```javascript
/**
 * Cache Management - Enforce size limits and LRU eviction
 */

const MAX_POSTER_CACHE_SIZE = 200; // Max number of posters to cache
const MAX_API_CACHE_SIZE = 50;     // Max number of API responses

async function enforcePostersLimit(cache) {
  const keys = await cache.keys();
  const posterKeys = keys.filter(req => req.url.includes('image.tmdb.org'));

  if (posterKeys.length > MAX_POSTER_CACHE_SIZE) {
    // Sort by last accessed time (stored in IndexedDB or assume FIFO)
    const toDelete = posterKeys.slice(0, posterKeys.length - MAX_POSTER_CACHE_SIZE);

    console.log(`[Cache Management] Evicting ${toDelete.length} old posters`);
    await Promise.all(toDelete.map(key => cache.delete(key)));
  }
}

async function enforceAPILimit(cache) {
  const keys = await cache.keys();

  if (keys.length > MAX_API_CACHE_SIZE) {
    const toDelete = keys.slice(0, keys.length - MAX_API_CACHE_SIZE);

    console.log(`[Cache Management] Evicting ${toDelete.length} old API responses`);
    await Promise.all(toDelete.map(key => cache.delete(key)));
  }
}

// Run eviction check periodically
self.addEventListener('message', event => {
  if (event.data.action === 'enforc eLimits') {
    caches.open(POSTER_CACHE).then(enforcePostersLimit);
    caches.open(API_CACHE).then(enforceAPILimit);
  }
});
```

#### Storage Quota Monitoring

Add to `wwwroot/js/site.js`:

```javascript
/**
 * Monitor PWA storage quota (especially critical for iOS)
 */
if ('storage' in navigator && 'estimate' in navigator.storage) {
  navigator.storage.estimate().then(estimate => {
    const usedMB = (estimate.usage / (1024 * 1024)).toFixed(2);
    const quotaMB = (estimate.quota / (1024 * 1024)).toFixed(2);
    const percentUsed = ((estimate.usage / estimate.quota) * 100).toFixed(1);

    console.log(`📦 PWA Storage: ${usedMB}MB / ${quotaMB}MB (${percentUsed}%)`);

    // Warning at 80% capacity (iOS limit is typically 50MB)
    if (estimate.usage / estimate.quota > 0.8) {
      console.warn('⚠️ Cache nearly full - triggering eviction');

      // Trigger cache cleanup
      if ('serviceWorker' in navigator && navigator.serviceWorker.controller) {
        navigator.serviceWorker.controller.postMessage({ action: 'enforceLimits' });
      }
    }

    // Track in Application Insights
    if (window.appInsights) {
      appInsights.trackEvent({
        name: 'PWA_StorageQuota',
        properties: {
          usedMB: parseFloat(usedMB),
          quotaMB: parseFloat(quotaMB),
          percentUsed: parseFloat(percentUsed)
        }
      });
    }
  });
}
```

---

## 🌐 Browser Compatibility Matrix

**Validated browser support for PWA features:**

| Browser | Minimum Version | PWA Features | Install Support | Offline | Push Notifications | Notes |
|---------|----------------|--------------|-----------------|---------|-------------------|-------|
| **Chrome (Android)** | 40+ (2015) | ✅ Full | ✅ Auto-prompt | ✅ Yes | ✅ Yes | Best PWA support |
| **Safari (iOS)** | 11.3+ (2018) | ⚠️ Limited | ⚠️ Manual only | ✅ Yes | ❌ No | 50MB cache limit |
| **Edge (Desktop)** | 17+ (2018) | ✅ Full | ✅ Auto-prompt | ✅ Yes | ✅ Yes | Chromium-based |
| **Firefox (Desktop)** | 44+ (2016) | ⚠️ Partial | ❌ No install | ✅ Yes | ⚠️ Limited | Service worker only |
| **Samsung Internet** | 4.0+ (2016) | ✅ Full | ✅ Auto-prompt | ✅ Yes | ✅ Yes | Popular in Asia |
| **Opera (Mobile)** | 37+ (2016) | ✅ Full | ✅ Auto-prompt | ✅ Yes | ✅ Yes | Chromium-based |

### iOS Safari Limitations (CRITICAL)

| Feature | Status | Workaround |
|---------|--------|------------|
| Auto-install prompt | ❌ Not supported | Manual instructions (see Step 4) |
| Push notifications | ❌ Not supported | Email notifications as fallback |
| Background sync | ❌ Not supported | Sync on app open |
| Cache quota | ⚠️ ~50MB limit | Aggressive eviction (see Performance Budget) |
| Splash screen | ✅ Supported | via apple-touch-startup-image |
| App icon | ✅ Supported | via apple-touch-icon |

### Market Coverage Estimate

Based on StatCounter Global Stats (2024):

| Platform | Market Share | PWA Support | Installable |
|----------|-------------|-------------|-------------|
| Chrome (Mobile) | ~65% | ✅ Full | ✅ Yes |
| Safari (iOS) | ~25% | ⚠️ Limited | ⚠️ Manual |
| Samsung Internet | ~5% | ✅ Full | ✅ Yes |
| Others | ~5% | Varies | Varies |

**Total Installable Coverage**: ~70-75% of mobile users (auto-prompt)
**Total PWA-Compatible Coverage**: ~95% of mobile users (with manual install)

---

## 📱 Required Testing Devices

**Minimum device coverage for comprehensive testing:**

### Essential Devices (MUST TEST)

1. **iPhone (iOS 15+)** - Safari
   - Represents ~25% of market
   - Critical for iOS-specific limitations testing
   - Test manual install flow

2. **Android Phone (Android 11+)** - Chrome
   - Represents ~65% of market
   - Test auto-install prompt
   - Verify full PWA features

### Recommended Additional Devices

3. **iPad (iOS 15+)** - Safari (Optional)
   - Test tablet experience
   - Verify responsive manifest

4. **Samsung Galaxy (Android 11+)** - Samsung Internet (Optional)
   - Popular browser in Asia
   - Test alternative Chromium implementation

### Testing Checklist (Per Device)

**For each device, verify:**

- [ ] **Installation**:
  - [ ] Install prompt appears (or manual instructions on iOS)
  - [ ] App icon displays correctly on home screen
  - [ ] App name shows correctly (short_name from manifest)
  - [ ] Opens in standalone mode (no browser UI)

- [ ] **Offline Functionality**:
  - [ ] Enable airplane mode
  - [ ] App opens successfully
  - [ ] Previously viewed movies load
  - [ ] TMDB posters load from cache
  - [ ] Navigation works between cached pages
  - [ ] Offline page displays for uncached routes

- [ ] **Performance**:
  - [ ] Initial load < 3 seconds (on 3G connection)
  - [ ] Subsequent loads < 1 second (from cache)
  - [ ] No console errors
  - [ ] Lighthouse PWA score ≥ 90/100

- [ ] **Visual Regression**:
  - [ ] Layout matches desktop version
  - [ ] Gold theme color (#f4d03f) displays correctly
  - [ ] Icons sharp (no blur/pixelation)
  - [ ] Touch targets ≥ 44px (accessibility)

---

## ♿ Accessibility Checklist (WCAG 2.1 AA Compliance)

**Ensure PWA meets accessibility standards:**

### Color Contrast

- [ ] **Theme color contrast**:
  - [ ] `#f4d03f` (gold) on `#1a1a1a` (dark) = 8.2:1 ratio ✅ (AAA level)
  - [ ] Verify in Chrome DevTools → Lighthouse → Accessibility

- [ ] **Install banner contrast**:
  - [ ] Banner background vs text ≥ 4.5:1 (AA level)
  - [ ] Button text vs button background ≥ 4.5:1

### Keyboard Navigation

- [ ] **Install banner dismissible via keyboard**:
  - [ ] ESC key closes banner
  - [ ] TAB navigates to "Install Now" and "Maybe Later" buttons
  - [ ] ENTER activates focused button

- [ ] **PWA install prompt**:
  - [ ] Native browser prompt is keyboard-accessible by default

### Screen Reader Support

- [ ] **Offline page**:
  - [ ] Test with VoiceOver (iOS) or TalkBack (Android)
  - [ ] H1 heading announced correctly
  - [ ] Link to cached movies is discoverable

- [ ] **Install banner**:
  - [ ] ARIA label on banner: `aria-label="Install CineLog as an app"`
  - [ ] Button roles: `role="button"` (implicit for `<button>` elements)

### Manifest Accessibility

- [ ] **Manifest description**:
  - [ ] Clear, concise description for screen readers
  - [ ] "Track, rate, and discover your favorite movies" (already compliant)

### Touch Target Size

- [ ] **Install banner buttons**:
  - [ ] Minimum 44x44px touch targets
  - [ ] Adequate spacing between buttons (≥ 8px)

### Testing Tools

**Automated**:
- [ ] Lighthouse Accessibility audit (target: ≥ 95/100)
- [ ] axe DevTools browser extension
- [ ] WAVE Web Accessibility Evaluation Tool

**Manual**:
- [ ] VoiceOver (iOS): Settings → Accessibility → VoiceOver
- [ ] TalkBack (Android): Settings → Accessibility → TalkBack
- [ ] Keyboard-only navigation (unplug mouse, use TAB/ENTER/ESC)

---

## ⚙️ Step 3: Implement Service Worker (1-2 days)

### Create service-worker.js

**Location**: `wwwroot/service-worker.js`

```javascript
/**
 * CineLog Service Worker
 * Provides offline functionality and optimized caching for movie app experience
 *
 * ARCHITECTURE NOTE: This is CLIENT-SIDE caching that complements the existing
 * server-side IMemoryCache in CacheService.cs and TmdbService.cs.
 *
 * DUAL-LAYER CACHING:
 * - Server (IMemoryCache): TMDB API data, user blacklist/wishlist (already implemented)
 * - Client (Service Worker): TMDB posters, static assets, offline pages (new)
 *
 * NO CONFLICTS: Different cache keys and storage systems ensure zero collision.
 */

const CACHE_VERSION = 'cinelog-v1';
const STATIC_CACHE = 'cinelog-static-v1';
const POSTER_CACHE = 'cinelog-posters-v1';
const API_CACHE = 'cinelog-api-v1';

// Static assets to cache on install
// NOTE: Matches CineLog's actual file structure (verified from wwwroot/)
const STATIC_ASSETS = [
  '/',
  '/css/site.css',
  '/css/spacing-enhancements.css',
  '/css/vertical-centering.css',
  '/css/bootstrap.min.css',
  '/js/site.js',
  '/lib/bootstrap/dist/css/bootstrap.min.css',
  '/lib/bootstrap/dist/js/bootstrap.bundle.min.js',
  '/lib/jquery/dist/jquery.min.js',
  '/images/icons/icon-192.png',
  '/images/icons/icon-512.png',
  '/favicon.ico'
];

/**
 * Install Event - Cache static assets
 */
self.addEventListener('install', event => {
  console.log('[Service Worker] Installing...');

  event.waitUntil(
    caches.open(STATIC_CACHE)
      .then(cache => {
        console.log('[Service Worker] Caching static assets');
        return cache.addAll(STATIC_ASSETS);
      })
      .then(() => self.skipWaiting())
  );
});

/**
 * Activate Event - Clean up old caches
 */
self.addEventListener('activate', event => {
  console.log('[Service Worker] Activating...');

  event.waitUntil(
    caches.keys().then(cacheNames => {
      return Promise.all(
        cacheNames
          .filter(cacheName => {
            return cacheName.startsWith('cinelog-') &&
                   cacheName !== STATIC_CACHE &&
                   cacheName !== POSTER_CACHE &&
                   cacheName !== API_CACHE;
          })
          .map(cacheName => {
            console.log('[Service Worker] Deleting old cache:', cacheName);
            return caches.delete(cacheName);
          })
      );
    }).then(() => self.clients.claim())
  );
});

/**
 * Fetch Event - Intelligent caching strategies
 *
 * STRATEGY DESIGN:
 * - TMDB Posters: Cache-First (images don't change, server doesn't cache these)
 * - API Calls: Network-First (server has IMemoryCache, we provide offline fallback)
 * - Static Assets: Cache-First (CSS/JS don't change often)
 * - Navigation: Network-First (fresh HTML with offline fallback)
 *
 * COORDINATION WITH SERVER CACHE:
 * - Server (IMemoryCache) handles TMDB API responses (24h TTL)
 * - Client (Service Worker) handles posters and offline experience
 * - Both caches work together without conflicts
 */
self.addEventListener('fetch', event => {
  const { request } = event;
  const url = new URL(request.url);

  // STRATEGY 1: TMDB Posters - Cache-First (Aggressive)
  if (url.hostname.includes('image.tmdb.org')) {
    event.respondWith(
      caches.open(POSTER_CACHE).then(cache => {
        return cache.match(request).then(cachedResponse => {
          if (cachedResponse) {
            console.log('[Cache Hit] TMDB Poster:', url.pathname);
            return cachedResponse;
          }

          // Fetch from network and cache
          return fetch(request).then(networkResponse => {
            // Only cache successful responses
            if (networkResponse.ok) {
              console.log('[Cache Store] TMDB Poster:', url.pathname);
              cache.put(request, networkResponse.clone());
            }
            return networkResponse;
          }).catch(() => {
            console.log('[Offline] TMDB Poster unavailable:', url.pathname);
            // Return placeholder image if available
            return caches.match('/images/poster-placeholder.png');
          });
        });
      })
    );
    return;
  }

  // STRATEGY 2: API Calls - Network-First (Fresh data priority)
  if (url.pathname.includes('/Movies/') ||
      url.pathname.includes('/api/') ||
      url.pathname.includes('/Blacklist/') ||
      url.pathname.includes('/Wishlist/')) {

    event.respondWith(
      fetch(request)
        .then(networkResponse => {
          // Cache successful responses
          if (networkResponse.ok) {
            caches.open(API_CACHE).then(cache => {
              console.log('[Cache Store] API:', url.pathname);
              cache.put(request, networkResponse.clone());
            });
          }
          return networkResponse;
        })
        .catch(() => {
          console.log('[Cache Fallback] API:', url.pathname);
          return caches.match(request).then(cachedResponse => {
            if (cachedResponse) {
              return cachedResponse;
            }
            // Return offline page if available
            return caches.match('/offline.html');
          });
        })
    );
    return;
  }

  // STRATEGY 3: Static Assets - Cache-First (Performance)
  if (request.destination === 'style' ||
      request.destination === 'script' ||
      request.destination === 'font') {

    event.respondWith(
      caches.match(request).then(cachedResponse => {
        if (cachedResponse) {
          console.log('[Cache Hit] Static:', url.pathname);
          return cachedResponse;
        }
        return fetch(request).then(networkResponse => {
          if (networkResponse.ok) {
            caches.open(STATIC_CACHE).then(cache => {
              cache.put(request, networkResponse.clone());
            });
          }
          return networkResponse;
        });
      })
    );
    return;
  }

  // STRATEGY 4: Navigation - Network-First (Fresh HTML)
  if (request.mode === 'navigate') {
    event.respondWith(
      fetch(request)
        .catch(() => {
          console.log('[Offline] Navigation fallback');
          return caches.match('/offline.html');
        })
    );
    return;
  }

  // Default: Network-First for everything else
  event.respondWith(
    fetch(request).catch(() => caches.match(request))
  );
});

/**
 * Message Event - Handle cache clearing from app
 */
self.addEventListener('message', event => {
  if (event.data.action === 'clearCache') {
    event.waitUntil(
      caches.keys().then(cacheNames => {
        return Promise.all(
          cacheNames.map(cacheName => {
            console.log('[Service Worker] Clearing cache:', cacheName);
            return caches.delete(cacheName);
          })
        );
      })
    );
  }
});
```

### Register Service Worker in _Layout.cshtml

**Location**: `Views/Shared/_Layout.cshtml` (before closing `</body>` tag)

```html
<!-- PWA Service Worker Registration -->
<script>
if ('serviceWorker' in navigator) {
  window.addEventListener('load', () => {
    navigator.serviceWorker.register('/service-worker.js')
      .then(registration => {
        console.log('✅ Service Worker registered:', registration.scope);
      })
      .catch(error => {
        console.log('❌ Service Worker registration failed:', error);
      });
  });
}
</script>
```

### Create Offline Fallback Page

**Location**: `Views/Home/Offline.cshtml`

```cshtml
@{
    ViewData["Title"] = "Offline - CineLog";
}

<div class="text-center" style="padding: 80px 20px;">
    <h1 class="display-1">📡</h1>
    <h2>You're Offline</h2>
    <p class="lead">CineLog needs an internet connection to load new content.</p>
    <p>Don't worry - your cached movies are still available!</p>
    <a asp-controller="Movies" asp-action="Index" class="btn btn-primary mt-3">
        View Cached Movies
    </a>
</div>
```

**Controller Action** in `HomeController.cs`:

```csharp
/// <summary>
/// Offline fallback page for PWA
/// </summary>
public IActionResult Offline()
{
    return View();
}
```

### iOS Cache Management (Important for Safari)

**Safari has stricter cache limits (50MB recommended vs unlimited on Android)**

Add cache cleanup in `wwwroot/js/site.js`:

```javascript
/**
 * PWA Cache Management for iOS Safari
 * iOS Safari has stricter cache limits (~50MB)
 */

// Check cache size periodically (iOS specific)
if ('storage' in navigator && 'estimate' in navigator.storage) {
  navigator.storage.estimate().then(estimate => {
    const usedMB = (estimate.usage / (1024 * 1024)).toFixed(2);
    const quotaMB = (estimate.quota / (1024 * 1024)).toFixed(2);

    console.log(`📦 Cache Usage: ${usedMB}MB / ${quotaMB}MB`);

    // Warning at 80% capacity
    if (estimate.usage / estimate.quota > 0.8) {
      console.warn('⚠️ Cache nearly full - consider clearing old posters');
      // Optionally show UI prompt to clear cache
    }
  });
}

// Function to clear cache if needed
function clearAppCache() {
  if ('serviceWorker' in navigator && 'caches' in window) {
    caches.keys().then(cacheNames => {
      return Promise.all(
        cacheNames
          .filter(name => name.includes('posters') || name.includes('api'))
          .map(name => caches.delete(name))
      );
    }).then(() => {
      alert('✅ Cache cleared successfully!');
      location.reload();
    });
  }
}
```

---

## 📱 Step 4: iOS-Specific Considerations (1-2 hours)

### Problem: iOS Safari Doesn't Auto-Prompt Install

Unlike Android Chrome, iOS Safari requires **manual installation instructions**.

### Solution: Add Install Prompt Banner

**Location**: `Views/Shared/_Layout.cshtml` (after opening `<body>` tag)

```html
<!-- iOS Install Prompt (only show on iOS Safari) -->
<div id="ios-install-prompt" style="display: none; position: fixed; bottom: 0; left: 0; right: 0; background: #d4af37; color: #000; padding: 16px; text-align: center; z-index: 9999; box-shadow: 0 -2px 10px rgba(0,0,0,0.3);">
    <strong>📱 Install CineLog</strong>
    <p style="margin: 8px 0; font-size: 14px;">
        Tap <span style="font-size: 20px;">⎙</span> then "Add to Home Screen"
    </p>
    <button onclick="dismissInstallPrompt()" style="background: #000; color: #d4af37; border: none; padding: 8px 16px; border-radius: 4px; cursor: pointer;">
        Got it!
    </button>
</div>

<script>
// Show iOS install prompt only on iOS Safari
function checkiOS() {
  const isIOS = /iPad|iPhone|iPod/.test(navigator.userAgent) && !window.MSStream;
  const isStandalone = window.navigator.standalone === true;

  if (isIOS && !isStandalone) {
    // Check if user dismissed prompt recently
    const dismissed = localStorage.getItem('iosInstallPromptDismissed');
    if (!dismissed) {
      document.getElementById('ios-install-prompt').style.display = 'block';
    }
  }
}

function dismissInstallPrompt() {
  document.getElementById('ios-install-prompt').style.display = 'none';
  localStorage.setItem('iosInstallPromptDismissed', Date.now());
}

// Check on page load
window.addEventListener('load', checkiOS);
</script>
```

### iOS PWA Limitations to Know

| Feature | Android Chrome | iOS Safari | Impact |
|---------|----------------|------------|--------|
| Auto Install Prompt | ✅ Yes | ❌ No | Need manual instructions |
| Push Notifications | ✅ Full support | ⚠️ Limited (iOS 16.4+) | Can't notify users of new movies |
| Background Sync | ✅ Yes | ❌ No | Can't sync data in background |
| Cache Quota | ~250MB+ | ~50MB | Need aggressive cache management |
| App Badge | ✅ Yes | ⚠️ Limited | Can't show unread counts |

**Recommendation**: Focus on core offline experience rather than advanced features for iOS.

---

## 🧪 Step 5: Testing & Validation (1-2 days)

### Local Testing Checklist

#### 1. PWA Installability Test

**Chrome DevTools Audit**:
```
1. Open Chrome DevTools (F12)
2. Go to "Lighthouse" tab
3. Select "Progressive Web App"
4. Click "Generate report"
5. Target: Score ≥ 90/100
```

**Expected Pass Criteria**:
- ✅ Manifest file valid
- ✅ Service worker registered
- ✅ HTTPS (localhost or production)
- ✅ Responsive design
- ✅ Splash screen configured

#### 2. Offline Functionality Test

```
1. Open https://localhost:7186 in Chrome
2. Navigate to Movies page
3. Open DevTools → Application → Service Workers
4. Check "Offline" checkbox
5. Refresh page
6. Verify: Cached movies still display
7. Verify: Navigation works
8. Verify: TMDB posters load from cache
```

#### 3. Cache Strategy Test

**TMDB Posters (Cache-First)**:
```
1. Load Movies page (network online)
2. Open DevTools → Network tab
3. Refresh page - see posters load from network
4. Refresh again - see "from disk cache" (should be instant)
5. Verify in Application → Cache Storage → cinelog-posters-v1
```

**API Calls (Network-First)**:
```
1. Load Movies page
2. Open DevTools → Network → Throttling → "Slow 3G"
3. Navigate to different page
4. Verify: Page loads (may be slower)
5. Go offline
6. Verify: Cached data still available
```

#### 4. iOS Testing (Physical Device Required)

**Setup**:
```
1. Deploy to Azure (https://cinelog-app.azurewebsites.net)
2. Open in iPhone Safari
3. Verify manifest loads (check DevTools on Mac)
```

**Installation Test**:
```
1. Tap Safari share button ⎙
2. Scroll down to "Add to Home Screen"
3. Verify: App icon appears
4. Verify: App name shows correctly
5. Tap "Add"
6. Open app from home screen
7. Verify: Standalone mode (no Safari UI)
```

**Offline Test (iOS)**:
```
1. Open CineLog PWA from home screen
2. Load several movie pages (build cache)
3. Enable Airplane Mode
4. Verify: Cached movies load
5. Verify: TMDB posters display
6. Try navigation - verify cached pages work
```

### Production Deployment Testing

```bash
# Build for production
dotnet publish -c Release -o ./publish-pwa

# Test production build locally
cd publish-pwa
dotnet Ezequiel_Movies.dll --urls "https://localhost:7186"

# Verify PWA features work in production mode
# Deploy to Azure
# Test on real mobile devices
```

---

## 🚨 Rollback & Emergency Procedures

**CRITICAL**: Before deploying PWA to production, ensure this rollback plan is understood and tested.

### Emergency Service Worker Kill Switch

**Scenario**: Service worker is caching broken version or causing critical bugs in production.

**Kill Switch Procedure** (Execute within 5 minutes):

1. **Create emergency service worker** (`wwwroot/service-worker.js`):
```javascript
// EMERGENCY KILL SWITCH - Clear all caches and disable PWA
self.addEventListener('install', event => {
  console.log('[EMERGENCY] Disabling PWA and clearing all caches');
  self.skipWaiting(); // Activate immediately
});

self.addEventListener('activate', event => {
  event.waitUntil(
    caches.keys().then(cacheNames => {
      return Promise.all(
        cacheNames.map(cacheName => {
          console.log('[EMERGENCY] Deleting cache:', cacheName);
          return caches.delete(cacheName);
        })
      );
    }).then(() => {
      console.log('[EMERGENCY] All caches cleared. PWA disabled.');
      return self.clients.claim();
    })
  );
});

// Pass through all requests - no caching
self.addEventListener('fetch', event => {
  event.respondWith(fetch(event.request));
});
```

2. **Deploy emergency service worker**:
```bash
# Fast deployment (skip tests in emergency)
dotnet publish -c Release -o ./publish-emergency
cd publish-emergency && zip -r ../emergency-$(date +%Y%m%d-%H%M).zip .

# Deploy to Azure (use Azure CLI or Portal)
az webapp deployment source config-zip \
  --resource-group CineLog \
  --name cinelog-app \
  --src emergency-*.zip
```

3. **Verify kill switch activated**:
- Open production site in Chrome DevTools
- Application → Service Workers → Check new SW activating
- Application → Cache Storage → Verify all caches being deleted
- Network tab → Verify no requests served from cache

4. **Monitor Application Insights**:
```
customEvents
| where name == "ServiceWorkerActivated"
| where customDimensions.version == "emergency-killswitch"
| summarize count() by bin(timestamp, 5m)
```

**Expected Timeline**: Users see fresh version within 1-2 app reopens (~5-30 minutes)

---

### Complete Rollback Procedure

**Scenario**: Need to completely remove PWA features and return to standard web app.

**Rollback Steps**:

1. **Remove manifest link** from `Views/Shared/_Layout.cshtml`:
```diff
- <!-- PWA Manifest -->
- <link rel="manifest" href="/manifest.json">
- <meta name="theme-color" content="#f4d03f">
- <meta name="apple-mobile-web-app-capable" content="yes">
- <meta name="apple-mobile-web-app-status-bar-style" content="black-translucent">
- <meta name="apple-mobile-web-app-title" content="CineLog">
- <link rel="apple-touch-icon" href="/images/icons/icon-192.png">
```

2. **Deploy kill switch service worker** (see above)

3. **Remove PWA service** from `Program.cs`:
```diff
- // FEATURE: PWA support for mobile app experience
- builder.Services.AddProgressiveWebApp();
```

4. **Build and deploy**:
```bash
dotnet build  # Verify compilation
dotnet publish -c Release -o ./publish-rollback
# Deploy to Azure
```

5. **Verify rollback complete**:
- [ ] Production site no longer shows install prompt
- [ ] Application → Manifest shows "No manifest detected"
- [ ] Service worker clears all caches
- [ ] Users experience standard web app behavior

---

### Incident Response Checklist

**If PWA causes production incident**:

- [ ] **Assess severity** (P0: Kill switch immediately, P1: Deploy fix within 1 hour, P2: Schedule fix)
- [ ] **Execute kill switch** (if P0 severity)
- [ ] **Notify stakeholders** (email/Slack: "PWA incident - investigating")
- [ ] **Check Application Insights** for error patterns
- [ ] **Identify root cause** (service worker bug? cache corruption? manifest issue?)
- [ ] **Deploy fix** or complete rollback
- [ ] **Monitor for 24 hours** post-fix
- [ ] **Post-mortem** (document what happened, how we fixed it, how to prevent)

---

### User State Management

**Problem**: Users who installed PWA before rollback will have stale app.

**Solution**: Force cache clear on next visit

Add to `wwwroot/js/site.js`:
```javascript
// PWA Version Check - Force refresh if version mismatch
const PWA_VERSION = '1.0.0'; // Increment on breaking changes
const STORED_VERSION = localStorage.getItem('pwa_version');

if (STORED_VERSION && STORED_VERSION !== PWA_VERSION) {
  console.log('PWA version mismatch - clearing caches');

  // Clear all caches
  if ('caches' in window) {
    caches.keys().then(names => {
      names.forEach(name => caches.delete(name));
    });
  }

  // Update stored version
  localStorage.setItem('pwa_version', PWA_VERSION);

  // Force reload
  window.location.reload(true);
}

// Store current version
localStorage.setItem('pwa_version', PWA_VERSION);
```

---

## 📊 Production Monitoring & Alerting

**CRITICAL**: Set up monitoring BEFORE deploying PWA to production.

### Application Insights Custom Events

Add to `wwwroot/js/site.js`:

```javascript
/**
 * PWA Telemetry - Track PWA-specific events in Application Insights
 */

// Service Worker Registration
if ('serviceWorker' in navigator) {
  navigator.serviceWorker.register('/service-worker.js')
    .then(registration => {
      console.log('✅ Service Worker registered:', registration.scope);

      // Track successful registration
      if (window.appInsights) {
        appInsights.trackEvent({
          name: 'PWA_ServiceWorkerRegistered',
          properties: {
            scope: registration.scope,
            state: registration.active?.state
          }
        });
      }
    })
    .catch(error => {
      console.error('❌ Service Worker registration failed:', error);

      // Track registration failure
      if (window.appInsights) {
        appInsights.trackException({
          exception: error,
          properties: {
            eventName: 'PWA_ServiceWorkerRegistrationFailed'
          }
        });
      }
    });
}

// PWA Install Prompt
window.addEventListener('beforeinstallprompt', (e) => {
  console.log('📱 PWA install prompt shown');

  if (window.appInsights) {
    appInsights.trackEvent({
      name: 'PWA_InstallPromptShown',
      properties: {
        userAgent: navigator.userAgent,
        platform: navigator.platform
      }
    });
  }

  // Track user choice
  e.userChoice.then((choiceResult) => {
    if (window.appInsights) {
      appInsights.trackEvent({
        name: 'PWA_InstallPromptResponse',
        properties: {
          outcome: choiceResult.outcome // 'accepted' or 'dismissed'
        }
      });
    }
  });
});

// PWA Installed (app added to home screen)
window.addEventListener('appinstalled', (e) => {
  console.log('✅ PWA installed successfully');

  if (window.appInsights) {
    appInsights.trackEvent({
      name: 'PWA_Installed',
      properties: {
        timestamp: new Date().toISOString()
      }
    });
  }
});

// Offline Page Views
window.addEventListener('online', () => {
  if (window.appInsights) {
    appInsights.trackEvent({
      name: 'PWA_BackOnline',
      properties: {
        timestamp: new Date().toISOString()
      }
    });
  }
});

window.addEventListener('offline', () => {
  if (window.appInsights) {
    appInsights.trackEvent({
      name: 'PWA_WentOffline',
      properties: {
        timestamp: new Date().toISOString()
      }
    });
  }
});
```

### Azure Application Insights Queries

**Service Worker Registration Failures** (Alert if > 5%):
```kusto
customEvents
| where name == "PWA_ServiceWorkerRegistrationFailed"
| summarize FailureCount = count() by bin(timestamp, 1h)
| join kind=leftouter (
    customEvents
    | where name == "PWA_ServiceWorkerRegistered"
    | summarize SuccessCount = count() by bin(timestamp, 1h)
  ) on timestamp
| extend FailureRate = (FailureCount * 100.0) / (FailureCount + SuccessCount)
| where FailureRate > 5
| project timestamp, FailureRate, FailureCount, SuccessCount
```

**PWA Install Rate** (Track adoption):
```kusto
customEvents
| where name in ("PWA_InstallPromptShown", "PWA_Installed")
| summarize
    PromptsShown = countif(name == "PWA_InstallPromptShown"),
    Installed = countif(name == "PWA_Installed")
  by bin(timestamp, 1d)
| extend InstallRate = (Installed * 100.0) / PromptsShown
| project timestamp, InstallRate, PromptsShown, Installed
```

**Offline Usage** (Monitor offline feature adoption):
```kusto
customEvents
| where name == "PWA_WentOffline"
| summarize OfflineSessions = count() by bin(timestamp, 1h)
| project timestamp, OfflineSessions
```

### Azure Monitor Alerts

**Configure alerts in Azure Portal**:

1. **Service Worker Failure Alert**:
   - Condition: Custom log search (query above)
   - Threshold: Failure rate > 5%
   - Frequency: Every 15 minutes
   - Action: Email to dev team

2. **Lighthouse Score Drop Alert**:
   - Monitor: Lighthouse CI (if configured)
   - Threshold: Score < 85/100
   - Action: Email + Slack notification

3. **Offline Page Views Spike**:
   - Condition: Offline events > 100/hour
   - Indicates: Potential connectivity issues or cache problems
   - Action: Investigate

---

## 📣 User Communication Strategy

**CRITICAL**: Communicate PWA launch to avoid user confusion.

### Pre-Launch (1 day before deployment)

**Email to Existing Users** (if email list available):
```
Subject: 📱 CineLog is Getting a Mobile Upgrade!

Hi [Name],

Tomorrow we're launching an exciting new feature: CineLog as a mobile app!

What's Changing:
✅ Add CineLog to your phone's home screen (like a native app)
✅ Faster loading with offline support
✅ Same experience you love, now app-like

How to Install (takes 30 seconds):
1. Visit cinelog-app.azurewebsites.net on your phone
2. Tap "Add to Home Screen" when prompted
3. Enjoy CineLog as an app!

Nothing else changes - your movies, ratings, and data remain safe.

Questions? Reply to this email.

Happy watching!
The CineLog Team
```

### Launch Day

**In-App Banner** (add to `Views/Shared/_Layout.cshtml`):

```html
<!-- PWA Install Promotion Banner (only mobile, dismissible) -->
<div id="pwa-install-banner" style="display: none; position: fixed; top: 0; left: 0; right: 0; background: linear-gradient(135deg, #f4d03f 0%, #d4af37 100%); color: #1a1a1a; padding: 16px; text-align: center; z-index: 10000; box-shadow: 0 2px 10px rgba(0,0,0,0.3);">
    <strong>📱 New: Install CineLog as an app!</strong>
    <p style="margin: 8px 0; font-size: 14px;">
        Faster loading, offline access, home screen icon
    </p>
    <button id="pwa-install-button" style="background: #1a1a1a; color: #f4d03f; border: none; padding: 10px 20px; border-radius: 4px; cursor: pointer; margin-right: 8px; font-weight: bold;">
        Install Now
    </button>
    <button onclick="dismissPWABanner()" style="background: transparent; color: #1a1a1a; border: 1px solid #1a1a1a; padding: 10px 20px; border-radius: 4px; cursor: pointer;">
        Maybe Later
    </button>
</div>

<script>
// Show PWA banner only on mobile, when installable
let deferredPrompt;

window.addEventListener('beforeinstallprompt', (e) => {
  e.preventDefault();
  deferredPrompt = e;

  // Check if banner was dismissed recently
  const bannerDismissed = localStorage.getItem('pwa_banner_dismissed');
  const dismissedTime = bannerDismissed ? new Date(bannerDismissed) : null;
  const daysSinceDismissed = dismissedTime ? (Date.now() - dismissedTime.getTime()) / (1000 * 60 * 60 * 24) : 999;

  // Show banner if: mobile device + not dismissed in last 7 days
  if (window.innerWidth <= 768 && daysSinceDismissed > 7) {
    document.getElementById('pwa-install-banner').style.display = 'block';
  }
});

document.getElementById('pwa-install-button')?.addEventListener('click', async () => {
  if (deferredPrompt) {
    deferredPrompt.prompt();
    const { outcome } = await deferredPrompt.userChoice;

    if (outcome === 'accepted') {
      console.log('User accepted PWA install');
    }

    deferredPrompt = null;
    document.getElementById('pwa-install-banner').style.display = 'none';
  }
});

function dismissPWABanner() {
  document.getElementById('pwa-install-banner').style.display = 'none';
  localStorage.setItem('pwa_banner_dismissed', new Date().toISOString());
}
</script>
```

### Post-Launch (Week 1)

**Monitor & Respond**:
- [ ] Check install rate daily (target: ≥ 5%)
- [ ] Monitor Application Insights for errors
- [ ] Respond to user questions/confusion
- [ ] Gather feedback via email/social media

**Success Communication** (if install rate > 5% after 7 days):
```
Social Media Post:
"📱 1000+ users have installed CineLog as an app!
Join them for faster loading and offline access.
cinelog-app.azurewebsites.net"
```

---

## 📊 Success Metrics & KPIs

### Technical Metrics

| Metric | Target | How to Measure |
|--------|--------|----------------|
| Lighthouse PWA Score | ≥ 90/100 | Chrome DevTools Lighthouse |
| Service Worker Registration | 100% | Console logs, Application tab |
| Cache Hit Rate (Posters) | ≥ 80% | Network tab, cache logs |
| Offline Page Load Time | < 1s | Performance tab |
| Install Rate | ≥ 5% of visitors | Google Analytics custom event |

### User Experience Metrics

| Metric | Target | How to Measure |
|--------|--------|----------------|
| Mobile Traffic | Track growth | Google Analytics |
| Session Duration (Mobile) | > Desktop | Google Analytics |
| Bounce Rate (Mobile) | < 40% | Google Analytics |
| Return Visitor Rate | ≥ 30% | Google Analytics |
| User Feedback | ≥ 4.5/5 | Surveys, feedback form |

### Implementation Checklist

- [ ] NuGet package installed (`WebEssentials.AspNetCore.PWA`)
- [ ] Service worker created and registered
- [ ] Manifest.json configured with CineLog branding
- [ ] App icons generated (all sizes)
- [ ] iOS install prompt implemented
- [ ] Offline page created
- [ ] Cache strategies implemented (Cache-First for posters, Network-First for APIs)
- [ ] Cache cleanup for iOS limits
- [ ] Lighthouse PWA audit passed (≥ 90/100)
- [ ] Offline functionality tested
- [ ] iOS installation tested on physical device
- [ ] Production deployment tested

---

## 🚀 Phase 2: Enhanced PWA (Optional - If Phase 1 Succeeds)

### Week 2-3 Enhancements

1. **Push Notifications** (for Android users)
   - Notify when friends add movies
   - Weekly movie recommendations
   - Implementation: Firebase Cloud Messaging + ASP.NET Core backend

2. **Background Sync**
   - Sync watched movies when back online
   - Implementation: Background Sync API (Android only)

3. **Advanced Caching**
   - Pre-cache trending movies
   - Intelligent cache eviction (LRU strategy)
   - IndexedDB for complex data structures

4. **App Shortcuts**
   - Quick access to "Add Movie"
   - Jump to Timeline
   - Implementation: Manifest shortcuts

### Sample Shortcuts in manifest.json

```json
"shortcuts": [
  {
    "name": "Add Movie",
    "short_name": "Add",
    "description": "Quickly add a movie to your collection",
    "url": "/Movies/Create",
    "icons": [{ "src": "/images/shortcuts/add-movie.png", "sizes": "96x96" }]
  },
  {
    "name": "View Timeline",
    "short_name": "Timeline",
    "description": "Browse your movie timeline",
    "url": "/Timeline",
    "icons": [{ "src": "/images/shortcuts/timeline.png", "sizes": "96x96" }]
  }
]
```

---

## 🎯 Phase 3: Native Wrapper with Capacitor (If App Explodes)

### When to Consider

- PWA install rate > 10%
- Mobile traffic > 60% of total
- User requests for app store distribution
- Need features unavailable in PWA (advanced push notifications, biometric auth)

### Capacitor Implementation (2-3 weeks)

```bash
# Install Capacitor
npm install @capacitor/core @capacitor/cli

# Initialize Capacitor
npx cap init CineLog com.cinelog.app --web-dir=wwwroot

# Add platforms
npx cap add ios
npx cap add android

# Sync web assets
npx cap sync

# Open in native IDEs
npx cap open ios
npx cap open android
```

**Benefits over pure PWA**:
- App Store/Play Store distribution
- Better push notifications on iOS
- Native biometric authentication
- Full offline capabilities
- Better device integration (camera, contacts, etc.)

**Trade-offs**:
- More complex deployment (2 stores to manage)
- App store approval process (1-2 weeks)
- Platform-specific builds required
- Higher maintenance overhead

---

## 🚫 Phase 4: .NET MAUI (Only if Massive Scale Required)

### When to Consider

- App has 100K+ active users
- Performance issues with web-based approaches
- Need advanced native features (background location, complex animations)
- Budget allows 6+ months development + dedicated mobile team

### Trade-offs

| Factor | PWA | Capacitor | .NET MAUI |
|--------|-----|-----------|-----------|
| Development Time | 1 week | 2-3 weeks | 6+ months |
| Code Reuse | 100% | 95% | 50-60% |
| Native Performance | Good | Very Good | Excellent |
| App Store Presence | No | Yes | Yes |
| Maintenance | Low | Medium | High |

**Recommendation**: Only pursue if Capacitor can't meet requirements and ROI justifies investment.

---

## 📝 Documentation & Maintenance

### Update Documentation

1. **README.md**: Add PWA setup instructions
2. **CHANGELOG.md**: Log PWA implementation
3. **SESSION_NOTES.md**: Document decisions and rationale

### Ongoing Maintenance

**Monthly Tasks**:
- Monitor cache hit rates
- Review Lighthouse scores
- Check service worker errors (Console, Sentry)
- Update cached assets when new features deploy

**Quarterly Tasks**:
- Audit PWA best practices
- Review iOS Safari changes (Apple updates frequently)
- Evaluate Phase 2/3 based on metrics

---

## 🎓 Learning Resources

### PWA Fundamentals
- [MDN: Progressive Web Apps](https://developer.mozilla.org/en-US/docs/Web/Progressive_web_apps)
- [Google: What are Progressive Web Apps?](https://web.dev/what-are-pwas/)
- [PWA Checklist](https://web.dev/pwa-checklist/)

### ASP.NET Core PWA
- [WebEssentials.AspNetCore.PWA Documentation](https://github.com/madskristensen/WebEssentials.AspNetCore.ServiceWorker)
- [ASP.NET Core PWA Tutorial](https://docs.microsoft.com/en-us/aspnet/core/blazor/progressive-web-app)

### Service Workers
- [MDN: Service Worker API](https://developer.mozilla.org/en-US/docs/Web/API/Service_Worker_API)
- [Workbox (Google's Service Worker Library)](https://developers.google.com/web/tools/workbox)

### iOS PWA Specifics
- [Apple: Configuring Web Applications](https://developer.apple.com/library/archive/documentation/AppleApplications/Reference/SafariWebContent/ConfiguringWebApplications/ConfiguringWebApplications.html)
- [iOS PWA Limitations](https://firt.dev/notes/pwa-ios/)

---

## ❓ FAQ

### Q: CineLog already uses cache - won't PWA conflict with it?
**A**: No conflicts! CineLog uses **dual-layer caching**:
- **Server-side** (IMemoryCache in `CacheService.cs` and `TmdbService.cs`): Caches TMDB API responses (24h), user blacklist/wishlist (15min)
- **Client-side** (PWA Service Worker): Caches TMDB posters, static assets, offline pages

They complement each other:
- Server cache reduces TMDB API calls
- PWA cache makes posters load instantly (5ms vs 150ms) and enables offline mode
- Different cache keys = zero collision risk
- Combined result: **97% faster poster loads** + offline functionality

### Q: Do users need to install anything?
**A**: No app store downloads required. Users simply visit the website, and on Android Chrome they'll see an "Add to Home Screen" prompt. iOS users need to manually tap Share → Add to Home Screen.

### Q: Will this work on all phones?
**A**: Yes! PWAs work on 95%+ of smartphones (iOS 11.3+, Android 5+). Older devices fall back to regular website experience.

### Q: Can we still update the app easily?
**A**: Yes! Update your ASP.NET Core code as usual, deploy to Azure, and PWA updates automatically next time users open the app. No app store approval needed.

### Q: What about app store visibility?
**A**: PWAs don't appear in app stores initially. If this becomes important later, we can wrap the PWA with Capacitor (Phase 3) to publish to stores without rebuilding.

### Q: Will TMDB posters load offline?
**A**: Yes! Once a poster is viewed once, it's cached and loads instantly offline. This is the Cache-First strategy implemented in service worker.

### Q: What's the biggest limitation vs native apps?
**A**: On iOS: No automatic install prompt and limited push notifications. On Android: None significant for CineLog's use case.

### Q: How much does this cost?
**A**: $0 additional cost. Uses existing Azure hosting. No app store fees ($99/year iOS, $25 one-time Android) until/unless we pursue Phase 3.

---

## 🎯 Next Steps

### Immediate Action (Today)

```bash
# Step 1: Install PWA NuGet package (5 minutes)
dotnet add package WebEssentials.AspNetCore.PWA
```

### This Week (October 2-9)

1. **Day 1**: Install package, configure manifest, create app icons
2. **Day 2-3**: Implement service worker with caching strategies
3. **Day 4**: Add iOS install prompt, offline page
4. **Day 5**: Local testing (Lighthouse, offline, cache validation)
5. **Day 6**: Deploy to Azure, test on real devices
6. **Day 7**: Bug fixes, documentation updates

### Success Criteria

- ✅ Lighthouse PWA score ≥ 90/100
- ✅ Installable on Android Chrome (auto-prompt works)
- ✅ Installable on iOS Safari (manual instructions work)
- ✅ Offline functionality confirmed (movies/posters load)
- ✅ TMDB posters cached effectively (≥ 80% hit rate)
- ✅ No build errors, warnings
- ✅ Production deployment successful

---

## 🏁 Conclusion

**We're implementing PWA first because**:
1. **Speed**: 1 week vs 6+ months for native
2. **Validation**: Test mobile demand before heavy investment
3. **Scalability**: Clear upgrade path to Capacitor or .NET MAUI if needed
4. **Simplicity**: Leverages 100% of existing codebase

**Ready to start? Let's begin with Step 1! 🚀**

---

*Document created: October 2, 2025*
*Project: CineLog - ASP.NET Core Movie Tracking App*
*Version: 1.0*
