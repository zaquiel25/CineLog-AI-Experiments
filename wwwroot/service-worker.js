/**
 * FrameRoute Service Worker
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
