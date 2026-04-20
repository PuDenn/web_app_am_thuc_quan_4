export function initMap(lat, lng, name) {
  const mapEl = document.getElementById('map');
  if (!mapEl) return;

  // Dùng Leaflet.js + OpenStreetMap (giống Mapsui nhưng cho web)
  if (!window.L) {
    const link = document.createElement('link');
    link.rel = 'stylesheet';
    link.href = 'https://unpkg.com/leaflet@1.9.4/dist/leaflet.css';
    document.head.appendChild(link);

    const script = document.createElement('script');
    script.src = 'https://unpkg.com/leaflet@1.9.4/dist/leaflet.js';
    script.onload = () => renderMap(lat, lng, name);
    document.head.appendChild(script);
  } else {
    renderMap(lat, lng, name);
  }
}

function renderMap(lat, lng, name) {
  const map = L.map('map').setView([lat, lng], 17);

  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution: '© OpenStreetMap contributors',
    maxZoom: 19,
  }).addTo(map);

  // Marker màu cam giống app MAUI
  const icon = L.divIcon({
    className: '',
    html: `<div style="
      width:36px;height:36px;border-radius:50%;
      background:#f97316;border:3px solid white;
      display:flex;align-items:center;justify-content:center;
      font-size:16px;box-shadow:0 2px 8px rgba(0,0,0,.4)">🍽️</div>`,
    iconSize: [36, 36],
    iconAnchor: [18, 18],
  });

  L.marker([lat, lng], { icon })
    .addTo(map)
    .bindPopup(`<b>${name}</b>`)
    .openPopup();

  // Nút mở Google Maps
  const btn = document.createElement('button');
  btn.textContent = '🗺️ Mở Google Maps';
  btn.style.cssText = `
    position:absolute;bottom:calc(80px + 16px);left:50%;transform:translateX(-50%);
    background:#f97316;color:white;border:none;border-radius:20px;
    padding:10px 20px;font-size:14px;cursor:pointer;z-index:999;
    font-family:'Be Vietnam Pro',sans-serif;`;
  btn.onclick = () => window.open(
    `https://www.google.com/maps/dir/?api=1&destination=${lat},${lng}`, '_blank');
  document.body.appendChild(btn);
}
