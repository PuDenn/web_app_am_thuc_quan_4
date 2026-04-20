export function drawLineChart(canvasId, labels, data, label = 'Lượt xem') {
  const ctx = document.getElementById(canvasId)?.getContext('2d');
  if (!ctx) return;
  if (window[canvasId + '_chart']) window[canvasId + '_chart'].destroy();
  window[canvasId + '_chart'] = new Chart(ctx, {
    type: 'line',
    data: {
      labels,
      datasets: [{
        label,
        data,
        borderColor: '#1A237E',
        backgroundColor: 'rgba(26,35,126,.08)',
        tension: 0.4,
        fill: true,
        pointBackgroundColor: '#E65100',
      }]
    },
    options: {
      responsive: true, maintainAspectRatio: false,
      plugins: { legend: { display: false } },
      scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
    }
  });
}

export function drawBarChart(canvasId, labels, datasets) {
  const ctx = document.getElementById(canvasId)?.getContext('2d');
  if (!ctx) return;
  if (window[canvasId + '_chart']) window[canvasId + '_chart'].destroy();
  window[canvasId + '_chart'] = new Chart(ctx, {
    type: 'bar',
    data: { labels, datasets },
    options: {
      responsive: true, maintainAspectRatio: false,
      plugins: { legend: { position: 'top' } },
      scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
    }
  });
}
