const publicVapidKey = 'BEl62iUYgUivxIkv69yViEuiBGmIYcgUic4hC7tS4OMj6O8uG-wJ6tW6m6y5q2uW5p4X5i6u7y8o9p0a1s2d3f4';

function urlBase64ToUint8Array(base64String) {
    const padding = '='.repeat((4 - base64String.length % 4) % 4);
    const base64 = (base64String + padding).replace(/\-/g, '+').replace(/_/g, '/');
    const rawData = window.atob(base64);
    const outputArray = new Uint8Array(rawData.length);
    for (let i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
}

async function aboneOl() {
    if (!('serviceWorker' in navigator)) {
        alert('Tarayıcınız bildirimleri desteklemiyor.');
        return;
    }

    const register = await navigator.serviceWorker.register('/service-worker.js', {
        scope: '/'
    });

    const permission = await Notification.requestPermission();
    if (permission !== 'granted') {
        alert('Bildirim izni verilmedi.');
        return;
    }

    const subscription = await register.pushManager.subscribe({
        userVisibleOnly: true,
        applicationServerKey: urlBase64ToUint8Array(publicVapidKey)
    });

    await fetch('/api/push/subscribe', {
        method: 'POST',
        body: JSON.stringify(subscription),
        headers: { 'Content-Type': 'application/json' }
    });

    alert('✅ Bildirimlere abone oldunuz!');
    document.getElementById('pushBtn').style.display = 'none';
}

window.addEventListener('load', () => {
    if ('serviceWorker' in navigator && 'PushManager' in window) {
        document.getElementById('pushBtn').style.display = 'block';
    }
});