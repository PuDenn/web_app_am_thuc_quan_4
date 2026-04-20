// Lấy API_BASE từ config.js (được load trước trong poi.html)
// Nếu không có config → fallback về localhost
var API_BASE = (typeof CONFIG !== 'undefined')
  ? CONFIG.API_URL
  : (location.hostname === 'localhost' || location.hostname === '127.0.0.1')
    ? 'http://localhost:5000'
    : 'http://' + location.hostname + ':5000';

async function fetchPoi(id) {
  var url = API_BASE + '/api/pois/' + id;
  var res = await fetch(url);
  if (!res.ok) throw new Error('Không tìm thấy quán (HTTP ' + res.status + ')\nURL: ' + url);
  return res.json();
}

function logEvent(poiId, type) {
  fetch(API_BASE + '/api/log', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ poiId: parseInt(poiId), type: type, source: 'web' })
  }).catch(function() {});
}
