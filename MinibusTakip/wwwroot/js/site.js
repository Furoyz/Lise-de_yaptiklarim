// ==========================================
// 1. KARANLIK / AYDINLIK MOD MANTIĞI
// ==========================================
(function () {
    const themeToggle = document.getElementById('themeToggle');
    const htmlElement = document.documentElement;
    const savedTheme = localStorage.getItem('theme') || 'light';

    htmlElement.setAttribute('data-theme', savedTheme);
    updateThemeIcon(savedTheme);

    if (themeToggle) {
        themeToggle.addEventListener('click', () => {
            const current = htmlElement.getAttribute('data-theme');
            const next = current === 'light' ? 'dark' : 'light';
            htmlElement.setAttribute('data-theme', next);
            localStorage.setItem('theme', next);
            updateThemeIcon(next);
        });
    }

    function updateThemeIcon(theme) {
        if (!themeToggle) return;
        const icon = themeToggle.querySelector('i');
        icon.className = theme === 'dark' ? 'fa-solid fa-sun' : 'fa-solid fa-moon';
    }
})();

// ==========================================
// 2. ŞOFÖR PANELİ (GPS KONTROLÜ)
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    const btnKonumPaylas = document.getElementById('btnKonumPaylas');
    if (!btnKonumPaylas) return; // Bu sayfada değilsek çık.

    let watchId = null;
    let isSharing = false;
    let wakeLock = null;

    async function baslat() {
        if (!navigator.geolocation) return alert('Tarayıcınız konumu desteklemiyor.');

        if ('wakeLock' in navigator) {
            try { wakeLock = await navigator.wakeLock.request('screen'); } catch (e) { }
        }

        isSharing = true;
        btnKonumPaylas.innerHTML = '<i class="fa-solid fa-stop me-2"></i> Paylaşımı Durdur';
        btnKonumPaylas.classList.replace('btn-success', 'btn-danger');

        watchId = navigator.geolocation.watchPosition(
            (pos) => {
                fetch('/Sofor/KonumGuncelleApi', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ Enlem: pos.coords.latitude, Boylam: pos.coords.longitude })
                });
            },
            (err) => console.warn(err.message),
            { enableHighAccuracy: true, maximumAge: 0, timeout: 10000 }
        );
    }

    function durdur() {
        if (watchId !== null) navigator.geolocation.clearWatch(watchId);
        if (wakeLock !== null) wakeLock.release();
        isSharing = false;
        btnKonumPaylas.innerHTML = '<i class="fa-solid fa-location-arrow me-2"></i> Konum Paylaşımını Başlat';
        btnKonumPaylas.classList.replace('btn-danger', 'btn-success');
    }

    btnKonumPaylas.addEventListener('click', () => isSharing ? durdur() : baslat());
});

// ==========================================
// 3. YOLCU PANELİ (HARİTA VE LİSTE)
// ==========================================
document.addEventListener("DOMContentLoaded", function () {
    const haritaElement = document.getElementById('harita');
    const stopIdInput = document.getElementById('seciliStopId');

    if (!haritaElement || !stopIdInput || !stopIdInput.value) return;

    // Haritayı Başlat
    var harita = L.map('harita', { zoomControl: false }).setView([39.92077, 32.85411], 14);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { attribution: '© OpenStreetMap' }).addTo(harita);

    var minibusIconHTML = '<div class="bus-marker" style="width:40px; height:40px;"><i class="fa-solid fa-bus-simple" style="color:white; font-size:1.1rem;"></i></div>';
    var minibusIcon = L.divIcon({ html: minibusIconHTML, className: '', iconSize: [40, 40], iconAnchor: [20, 20] });
    var markers = {};

    async function yenile() {
        try {
            const r = await fetch('/Yolcu/ApiMinibusler');
            if (!r.ok) return;
            const data = await r.json();

            const timeEl = document.getElementById('sonGuncelleme');
            if (timeEl) timeEl.innerHTML = '<i class="fa-solid fa-circle text-success me-1" style="font-size:6px;"></i> Canlı: ' + new Date().toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit', second: '2-digit' });

            const list = document.getElementById('minibusListesi');
            if (list) {
                if (!data.length) {
                    list.innerHTML = '<div class="premium-card text-center text-muted py-4">Şu an yaklaşan minibüs yok.</div>';
                } else {
                    list.innerHTML = data.map(function (m) {
                        var durumBadge = m.durum ? '<span class="badge bg-warning text-dark mt-2">' + m.durum + '</span>' : '';
                        var eskiSaat = (m.planlananSaat !== m.tahminiSaat) ? '<div class="text-muted text-decoration-line-through mt-1" style="font-size:0.75rem;">' + m.planlananSaat + '</div>' : '';

                        return '<div class="premium-card mb-3 p-3 d-flex align-items-center justify-content-between">' +
                            '<div>' +
                            '<h5 class="fw-bold mb-1" style="letter-spacing:-0.5px;">' + m.plaka + '</h5>' +
                            '<div class="text-muted small mb-1"><i class="fa-regular fa-user me-1"></i> ' + m.soforAd + '</div>' +
                            '<div class="small text-muted text-truncate" style="max-width:140px;"><i class="fa-solid fa-location-dot text-danger me-1"></i> ' + m.konumMetni + '</div>' +
                            durumBadge + '</div>' +
                            '<div class="text-end">' +
                            '<div class="fs-3 fw-bold text-dark lh-1" style="color: var(--text-main) !important;">' + m.tahminiSaat + '</div>' + eskiSaat +
                            '</div></div>';
                    }).join('');
                }
            }

            data.forEach(function (m) {
                if (m.enlem && m.boylam) {
                    if (markers[m.vehicleId]) {
                        markers[m.vehicleId].setLatLng([m.enlem, m.boylam]);
                    } else {
                        markers[m.vehicleId] = L.marker([m.enlem, m.boylam], { icon: minibusIcon }).addTo(harita)
                            .bindPopup('<b>' + m.plaka + '</b><br>' + m.soforAd + '<br>Saat: ' + m.tahminiSaat);
                    }
                }
            });

            var markerArray = Object.values(markers);
            if (markerArray.length > 0) harita.fitBounds(new L.featureGroup(markerArray).getBounds().pad(0.3));
        } catch (e) { console.error(e); }
    }

    yenile();
    setInterval(yenile, 5000);
});