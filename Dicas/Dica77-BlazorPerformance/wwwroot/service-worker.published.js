// Service Worker para produção (versão publicada)
const CACHE_NAME = 'blazor-performance-v1';

// Assets serão inseridos aqui durante o build
const assetsManifest = self.__WB_MANIFEST;

const urlsToCache = [
    '/',
    ...assetsManifest.map(asset => asset.url)
];

self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => cache.addAll(urlsToCache))
    );
});

self.addEventListener('fetch', event => {
    event.respondWith(
        caches.match(event.request)
            .then(response => response || fetch(event.request))
    );
});
