// Service Worker para cache offline
const CACHE_NAME = 'blazor-performance-v1';
const urlsToCache = [
    '/',
    '/css/bootstrap/bootstrap.min.css',
    '/css/app.css',
    '/js/performance.js',
    '/_framework/blazor.webassembly.js'
];

self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => {
                console.log('Opened cache');
                return cache.addAll(urlsToCache);
            })
    );
});

self.addEventListener('fetch', event => {
    event.respondWith(
        caches.match(event.request)
            .then(response => {
                // Cache hit - return response
                if (response) {
                    return response;
                }
                return fetch(event.request);
            }
        )
    );
});
