import { useEffect, useRef, useState } from 'react'
import maplibregl from 'maplibre-gl'
import 'maplibre-gl/dist/maplibre-gl.css'
import { setupPMTilesProtocol } from '../../lib/pmtiles-protocol'
import { useAppStore, MOCK_POIS } from '../../stores/useAppStore'
import { useTrackingStore } from '../../stores/useTrackingStore'
import UserMarker from '../tracking/UserMarker'

// Tọa độ trung tâm Quận 4, TP.HCM [longitude, latitude]
const Q4_CENTER: [number, number] = [106.7025, 10.7590]

export default function MainMap() {
  const mapContainer = useRef<HTMLDivElement>(null)
  const [map, setMap] = useState<maplibregl.Map | null>(null)

  // POI pulse markers (DOM elements, quản lý ngoài React)
  const pulseMarkersRef = useRef<Record<string, maplibregl.Marker>>({})

  const { setSelectedPoi } = useAppStore()
  const pulsingPoiIds = useTrackingStore((s) => s.pulsingPoiIds)

  // ─── Khởi tạo Map ────────────────────────────────
  useEffect(() => {
    if (!mapContainer.current) return
    setupPMTilesProtocol()

    const m = new maplibregl.Map({
      container: mapContainer.current,
      style: 'https://tiles.openfreemap.org/styles/liberty',
      center: Q4_CENTER,
      zoom: 15,
    })

    m.addControl(new maplibregl.NavigationControl(), 'top-right')

    m.on('load', () => {
      // ── POI Source ──
      m.addSource('pois', {
        type: 'geojson',
        data: {
          type: 'FeatureCollection',
          features: MOCK_POIS.map((poi) => ({
            type: 'Feature' as const,
            geometry: { type: 'Point' as const, coordinates: poi.coordinates },
            properties: { id: poi.id, name: poi.name, category: poi.category },
          })),
        },
      })

      // ── CircleLayer: chấm POI ──
      m.addLayer({
        id: 'poi-circles',
        type: 'circle',
        source: 'pois',
        paint: {
          'circle-radius': 12,
          'circle-color': '#f97316',
          'circle-stroke-width': 2,
          'circle-stroke-color': '#ffffff',
        },
      })

      // ── SymbolLayer: tên POI ──
      m.addLayer({
        id: 'poi-labels',
        type: 'symbol',
        source: 'pois',
        layout: {
          'text-field': ['get', 'name'],
          'text-font': ['Open Sans Bold'],
          'text-size': 12,
          'text-offset': [0, 1.8],
          'text-anchor': 'top',
        },
        paint: {
          'text-color': '#1f2937',
          'text-halo-color': '#ffffff',
          'text-halo-width': 1.5,
        },
      })

      // ── Click POI ──
      m.on('click', 'poi-circles', (e) => {
        const feature = e.features?.[0]
        if (!feature) return
        const poi = MOCK_POIS.find((p) => p.id === feature.properties?.id)
        if (poi) {
          setSelectedPoi(poi)
          m.flyTo({ center: poi.coordinates, zoom: 16, duration: 600 })
        }
      })

      m.on('mouseenter', 'poi-circles', () => { m.getCanvas().style.cursor = 'pointer' })
      m.on('mouseleave', 'poi-circles', () => { m.getCanvas().style.cursor = '' })

      setMap(m)
    })

    return () => {
      m.remove()
      setMap(null)
    }
  }, [setSelectedPoi])

  // ─── POI Pulse Animation ──────────────────────────
  // Khi một POI được trigger geofence → thêm marker pulse
  useEffect(() => {
    if (!map) return

    // Thêm pulse marker mới
    pulsingPoiIds.forEach((poiId) => {
      if (pulseMarkersRef.current[poiId]) return // đã có rồi

      const poi = MOCK_POIS.find((p) => p.id === poiId)
      if (!poi) return

      const el = createPulseElement()
      const marker = new maplibregl.Marker({ element: el, anchor: 'center' })
        .setLngLat(poi.coordinates)
        .addTo(map)

      pulseMarkersRef.current[poiId] = marker
    })

    // Xoá pulse marker không còn active
    Object.keys(pulseMarkersRef.current).forEach((poiId) => {
      if (!pulsingPoiIds.includes(poiId)) {
        pulseMarkersRef.current[poiId].remove()
        delete pulseMarkersRef.current[poiId]
      }
    })
  }, [pulsingPoiIds, map])

  // Cleanup tất cả pulse markers khi unmount
  useEffect(() => {
    return () => {
      Object.values(pulseMarkersRef.current).forEach((m) => m.remove())
      pulseMarkersRef.current = {}
    }
  }, [])

  return (
    <div style={{ position: 'relative', width: '100%', height: '100%' }}>
      {/* MapLibre container */}
      <div
        ref={mapContainer}
        style={{ width: '100%', height: '100%' }}
      />

      {/* UserMarker: render vào map sau khi map đã load */}
      {map && <UserMarker map={map} />}
    </div>
  )
}

// ─────────────────────────────────────────────────────
// Helper: tạo DOM element cho Pulse animation
// ─────────────────────────────────────────────────────
function createPulseElement(): HTMLDivElement {
  const wrap = document.createElement('div')
  wrap.className = 'poi-pulse-wrap'

  // 3 vòng tròn với delay khác nhau (defined in CSS)
  for (let i = 0; i < 3; i++) {
    const ring = document.createElement('div')
    ring.className = 'poi-pulse-ring'
    wrap.appendChild(ring)
  }

  return wrap
}
