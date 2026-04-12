import { create } from 'zustand'
import * as turf from '@turf/turf'
import { DOAN_VAN_BO } from '../features/tracking/roads'
import type { POI } from './useAppStore'

// ─────────────────────────────────────────────────────
// Constants
// ─────────────────────────────────────────────────────
export const WALK_SPEED_KMH       = 20          // tốc độ demo (~2.5 phút cho tour)
export const GEOFENCE_RADIUS_M    = 30          // bán kính trigger (mét)
export const DEBOUNCE_MS          = 3_000       // chờ 3s trong vùng mới trigger
export const COOLDOWN_MS          = 5 * 60_000  // cooldown 5 phút / POI
export const INTERP_MS            = 600         // thời gian lerp Marker (ms)

// Tổng chiều dài tuyến Đoàn Văn Bơ (km) — tính 1 lần
const ROUTE_LENGTH_KM = turf.length(DOAN_VAN_BO, { units: 'kilometers' })

// ─────────────────────────────────────────────────────
// Pure helpers (export để test / dùng ngoài store)
// ─────────────────────────────────────────────────────

/**
 * Snap tọa độ vào điểm gần nhất trên đường ray Đoàn Văn Bơ.
 * Luôn snap (không có threshold) vì trong simulation user
 * luôn đi trên đường này.
 */
export function snapToRoad(coords: [number, number]): [number, number] {
  const snapped = turf.nearestPointOnLine(DOAN_VAN_BO, turf.point(coords), {
    units: 'kilometers',
  })
  return snapped.geometry.coordinates as [number, number]
}

/**
 * Linear interpolation giữa 2 tọa độ.
 * t ∈ [0,1]
 */
export function lerpCoords(
  from: [number, number],
  to:   [number, number],
  t:    number,
): [number, number] {
  return [
    from[0] + (to[0] - from[0]) * t,
    from[1] + (to[1] - from[1]) * t,
  ]
}

/** Ease-in-out cubic cho chuyển động tự nhiên */
export function easeInOut(t: number): number {
  return t < 0.5 ? 2 * t * t : -1 + (4 - 2 * t) * t
}

/**
 * Tính vị trí tiếp theo trên tuyến dựa vào tốc độ + deltaTime.
 *
 * @param currentDistKm  Khoảng cách đã đi từ đầu tuyến (km)
 * @param deltaMs        Thời gian trôi qua (ms)
 * @returns { nextDistKm, coords, bearing, finished }
 */
export function advanceOnRoute(
  currentDistKm: number,
  deltaMs: number,
): {
  nextDistKm: number
  coords: [number, number]
  bearing: number
  finished: boolean
} {
  // Quãng đường đi thêm trong deltaMs ở tốc độ 5 km/h
  const deltaKm    = (WALK_SPEED_KMH / 3_600_000) * deltaMs
  const nextDistKm = currentDistKm + deltaKm
  const finished   = nextDistKm >= ROUTE_LENGTH_KM

  const clampedDist = Math.min(nextDistKm, ROUTE_LENGTH_KM)

  // Vị trí mới trên tuyến
  const pt = turf.along(DOAN_VAN_BO, clampedDist, { units: 'kilometers' })
  const coords = pt.geometry.coordinates as [number, number]

  // Bearing: hướng từ vị trí trước → vị trí mới
  const prevPt = turf.along(DOAN_VAN_BO, Math.max(0, currentDistKm), { units: 'kilometers' })
  const bearing = turf.bearing(prevPt, pt)

  return { nextDistKm: clampedDist, coords, bearing, finished }
}

// ─────────────────────────────────────────────────────
// Types
// ─────────────────────────────────────────────────────
interface GeofenceEntry {
  poiId:     string
  enteredAt: number
  timerId:   ReturnType<typeof setTimeout> | null
}

interface VisitedPOI {
  poiId:       string
  triggeredAt: number
}

interface TrackingState {
  // Vị trí hiện tại (đã snap, dùng để render Marker)
  displayPosition: [number, number] | null
  // Vị trí logic thật (dùng để tính geofence)
  snappedPosition: [number, number] | null
  // Khoảng cách đã đi trên tuyến (km) — dùng bởi SimulationEngine
  distanceKm: number
  // Hướng nhìn (độ)
  bearing: number
  // Độ chính xác giả lập (mét)
  accuracy: number

  // Geofence state
  activeGeofences: Record<string, GeofenceEntry>
  visitedPOIs:     VisitedPOI[]
  pulsingPoiIds:   string[]

  // Actions
  setDisplayPosition: (pos: [number, number]) => void
  setSnappedPosition: (pos: [number, number], bearing: number) => void
  setDistanceKm: (d: number) => void

  enterGeofence: (poi: POI, onTrigger: (poi: POI) => void) => void
  exitGeofence:  (poiId: string) => void
  markTriggered: (poiId: string) => void

  addPulse:    (poiId: string) => void
  removePulse: (poiId: string) => void

  isOnCooldown: (poiId: string) => boolean
  reset: () => void
}

// ─────────────────────────────────────────────────────
// Store
// ─────────────────────────────────────────────────────
export const useTrackingStore = create<TrackingState>((set, get) => ({
  displayPosition:  null,
  snappedPosition:  null,
  distanceKm:       0,
  bearing:          90,   // default hướng Đông (Đoàn Văn Bơ đi từ Tây → Đông)
  accuracy:         8,
  activeGeofences:  {},
  visitedPOIs:      [],
  pulsingPoiIds:    [],

  setDisplayPosition: (pos) => set({ displayPosition: pos }),

  setSnappedPosition: (pos, bearing) =>
    set({ snappedPosition: pos, bearing }),

  setDistanceKm: (d) => set({ distanceKm: d }),

  // ── Bước vào vùng POI: bắt đầu debounce 3s ──
  enterGeofence: (poi, onTrigger) => {
    const state = get()
    if (state.activeGeofences[poi.id]) return   // timer đang chạy
    if (state.isOnCooldown(poi.id)) return       // đang cooldown

    const timerId = setTimeout(() => {
      // Sau 3 giây — kiểm tra user vẫn trong vùng
      if (!get().activeGeofences[poi.id]) return

      get().markTriggered(poi.id)
      get().addPulse(poi.id)
      onTrigger(poi)

      // Xoá khỏi active
      set((s) => {
        const g = { ...s.activeGeofences }
        delete g[poi.id]
        return { activeGeofences: g }
      })
    }, DEBOUNCE_MS)

    set((s) => ({
      activeGeofences: {
        ...s.activeGeofences,
        [poi.id]: { poiId: poi.id, enteredAt: Date.now(), timerId },
      },
    }))
  },

  // ── Rời vùng POI: huỷ debounce timer ──
  exitGeofence: (poiId) => {
    const entry = get().activeGeofences[poiId]
    if (entry?.timerId) clearTimeout(entry.timerId)
    set((s) => {
      const g = { ...s.activeGeofences }
      delete g[poiId]
      return { activeGeofences: g }
    })
  },

  // ── Đánh dấu đã trigger → cooldown 5 phút ──
  markTriggered: (poiId) =>
    set((s) => ({
      visitedPOIs: [
        ...s.visitedPOIs.filter((v) => v.poiId !== poiId),
        { poiId, triggeredAt: Date.now() },
      ],
    })),

  addPulse: (poiId) => {
    set((s) => ({
      pulsingPoiIds: s.pulsingPoiIds.includes(poiId)
        ? s.pulsingPoiIds
        : [...s.pulsingPoiIds, poiId],
    }))
    setTimeout(() => get().removePulse(poiId), 6_000)
  },

  removePulse: (poiId) =>
    set((s) => ({ pulsingPoiIds: s.pulsingPoiIds.filter((id) => id !== poiId) })),

  isOnCooldown: (poiId) => {
    const v = get().visitedPOIs.find((x) => x.poiId === poiId)
    return !!v && Date.now() - v.triggeredAt < COOLDOWN_MS
  },

  reset: () =>
    set({
      displayPosition: null,
      snappedPosition:  null,
      distanceKm:       0,
      bearing:          90,
      activeGeofences:  {},
      pulsingPoiIds:    [],
    }),
}))
