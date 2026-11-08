const CACHE_NAME = 'minibus-takip-v1';
const ASSETS_TO_CACHE = [
    '/',
    '/manifest.json',
    '/lib/bootstrap/dist/css/bootstrap.min.css',
    '/lib/bootstrap/dist/js/bootstrap.bundle.min.js',
    '/lib/jquery/dist/jquery.min.js',
    '/icons/icon-192x192.png'
];

// 1. KURULUM (Dosyaları önbelleğe al)
self.addEventListener('install', (event) => {
    event.waitUntil(
        caches.open(CACHE_NAME).then((cache) => {
            return cache.addAll(ASSETS_TO_CACHE);
        })
    );
    self.skipWaiting();
});

// 2. AKTİVASYON (Eski önbellekleri temizle)
self.addEventListener('activate', (event) => {
    event.waitUntil(
        caches.keys().then((cacheNames) => {
            return Promise.all(
                cacheNames.map((cache) => {
                    if (cache !== CACHE_NAME) {
                        return caches.delete(cache);
                    }
                })
            );
        })
    );
    self.clients.claim();
});

// 3. İSTEK YAKALAMA (İnternet yoksa önbellekten getir)
self.addEventListener('fetch', (event) => {
    // Sadece GET isteklerini önbelleğe al
    if (event.request.method !== 'GET') return;

    event.respondWith(
        caches.match(event.request).then((cachedResponse) => {
            if (cachedResponse) {
                return cachedResponse; // Önbellekte varsa oradan getir
            }
            return fetch(event.request); // Yoksa internetten getir
        })
    );
});

// 4. PUSH BİLDİRİMİ (Önceki adımdan kalma)
self.addEventListener('push', function (e) {
    const data = e.data.json();
    const options = {
        body: data.body,
        icon: '/icons/icon-192x192.png',
        badge: '/icons/icon-192x192.png',
        data: { url: data.url || '/' }
    };
    e.waitUntil(
        self.registration.showNotification(data.title, options)
    );
});

// 5. BİLDİRİME TIKLAMA
self.addEventListener('notificationclick', function (e) {
    e.notification.close();
    e.waitUntil(
        clients.openWindow(e.notification.data.url)
    );
});