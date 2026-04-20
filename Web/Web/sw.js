// Service Worker v2 — xóa cache cũ
const CACHE = 'amthuc-q4-v2';
const ASSETS = ['/poi.html', '/css/style.css'];

// Xóa cache cũ khi activate
self.addEventListener('activate', e =>
  e.waitUntil(
    caches.keys().then(keys =>
      Promise.all(keys.filter(k => k !== CACHE).map(k => caches.delete(k)))
    )
  )
);

self.addEventListener('install', e => {
  self.skipWaiting();
  e.waitUntil(caches.open(CACHE).then(c => c.addAll(ASSETS).catch(() => {})));
});

self.addEventListener('fetch', e => {
  // API calls: luôn network, không cache
  if (e.request.url.includes(':5000')) return;
  // Các request khác: network first
  e.respondWith(
    fetch(e.request).catch(() => caches.match(e.request))
  );
});
