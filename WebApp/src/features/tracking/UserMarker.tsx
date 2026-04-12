import { useEffect, useRef } from 'react'
import maplibregl from 'maplibre-gl'
import { useTrackingStore } from '../../stores/useTrackingStore'

interface Props { map: maplibregl.Map }

const ACC_SOURCE = 'user-accuracy-src'
const ACC_LAYER  = 'user-accuracy-fill'

/**
 * UserMarker
 * ─────────────────────────────────────────────────
 * Subscribe trực tiếp vào Zustand store (KHÔNG qua React state)
 * để cập nhật MapLibre marker mỗi frame — không bị block bởi
 * React re-render cycle.
 */
export default function UserMarker({ map }: Props) {
  const markerRef   = useRef<maplibregl.Marker | null>(null)
  const arrowRef    = useRef<HTMLDivElement | null>(null)
  const prevBearing = useRef<number>(90)

  // ─── Setup layer + subscribe Zustand (1 lần khi mount) ──
  useEffect(() => {
    // Chờ style load xong mới thêm layer
    const setupLayer = () => {
      if (map.getSource(ACC_SOURCE)) return
      map.addSource(ACC_SOURCE, {
        type: 'geojson',
        data: emptyFC(),
      })
      map.addLayer({
        id: ACC_LAYER,
        type: 'fill',
        source: ACC_SOURCE,
        paint: { 'fill-color': '#3b82f6', 'fill-opacity': 0.12 },
      })
    }

    if (map.isStyleLoaded()) {
      setupLayer()
    } else {
      map.once('load', setupLayer)
    }

    // ── Subscribe Zustand: bypass React, update marker trực tiếp ──
    const unsubscribe = useTrackingStore.subscribe((state) => {
      const pos = state.displayPosition
      if (!pos) return

      // Tạo marker lần đầu
      if (!markerRef.current) {
        const { wrap, arrow } = buildMarkerDOM()
        arrowRef.current = arrow
        markerRef.current = new maplibregl.Marker({ element: wrap, anchor: 'center' })
          .setLngLat(pos)
          .addTo(map)
      } else {
        // Cập nhật vị trí trực tiếp — không qua React
        markerRef.current.setLngLat(pos)
      }

      // Cập nhật accuracy circle
      const src = map.getSource(ACC_SOURCE) as maplibregl.GeoJSONSource | undefined
      src?.setData(accuracyCircle(pos, state.accuracy))

      // Xoay mũi tên nếu bearing thay đổi > 2°
      if (arrowRef.current && Math.abs(state.bearing - prevBearing.current) > 2) {
        prevBearing.current = state.bearing
        arrowRef.current.style.transform = `rotate(${state.bearing}deg)`
      }
    })

    return () => {
      unsubscribe()
      markerRef.current?.remove()
      markerRef.current = null
    }
  }, [map])

  // Component này không render DOM (MapLibre quản lý marker)
  return null
}

// ─────────────────────────────────────────────────────
// DOM helpers
// ─────────────────────────────────────────────────────

function buildMarkerDOM(): { wrap: HTMLDivElement; arrow: HTMLDivElement } {
  const wrap = document.createElement('div')
  wrap.className = 'user-marker-wrap'

  const arrow = document.createElement('div')
  arrow.className = 'user-marker-arrow'
  arrow.innerHTML = /* html */`
    <svg viewBox="0 0 36 36" fill="none" xmlns="http://www.w3.org/2000/svg">
      <circle cx="18" cy="18" r="16" fill="#3b82f6" stroke="white" stroke-width="3"/>
      <path d="M18 8 L24 22 L18 19 L12 22 Z" fill="white"/>
    </svg>
  `
  wrap.appendChild(arrow)
  return { wrap, arrow }
}

function emptyFC(): GeoJSON.FeatureCollection {
  return { type: 'FeatureCollection', features: [] }
}

function accuracyCircle(
  center: [number, number],
  radiusM: number,
): GeoJSON.FeatureCollection {
  const N   = 64
  const rKm = radiusM / 1000
  const pts: [number, number][] = []

  for (let i = 0; i < N; i++) {
    const angle = (i / N) * 2 * Math.PI
    pts.push([
      center[0] + (rKm / (111 * Math.cos((center[1] * Math.PI) / 180))) * Math.cos(angle),
      center[1] + (rKm / 111) * Math.sin(angle),
    ])
  }
  pts.push(pts[0])

  return {
    type: 'FeatureCollection',
    features: [{
      type: 'Feature',
      properties: {},
      geometry: { type: 'Polygon', coordinates: [pts] },
    }],
  }
}
