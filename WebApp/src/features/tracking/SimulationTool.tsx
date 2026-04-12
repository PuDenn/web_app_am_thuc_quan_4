import { useEffect, useRef } from 'react'
import * as turf from '@turf/turf'
import { Play, Square } from 'lucide-react'
import { useAppStore, MOCK_POIS } from '../../stores/useAppStore'
import {
  useTrackingStore,
  advanceOnRoute,
  snapToRoad,
  GEOFENCE_RADIUS_M,
} from '../../stores/useTrackingStore'

// Tính vị trí mới mỗi 100ms → 10 updates/giây, đủ mượt, đủ nhanh
const STEP_MS   = 100
const LERP_MS   = 90

export default function SimulationTool() {
  const { isSimulating, setSimulating, triggerAudio, setSelectedPoi } = useAppStore()

  const rafRef       = useRef<number | null>(null)
  const inZoneRef    = useRef<Set<string>>(new Set())

  // Vị trí logic (snapped) — mục tiêu lerp
  const fromRef      = useRef<[number, number] | null>(null)
  const toRef        = useRef<[number, number] | null>(null)
  const stepStartRef = useRef<number>(0)

  // Thời điểm bước tiếp theo cần tính vị trí mới
  const nextStepRef  = useRef<number>(0)

  const start = () => {
    useTrackingStore.getState().reset()
    inZoneRef.current.clear()
    fromRef.current  = null
    toRef.current    = null

    setSimulating(true)

    const loop = (now: number) => {
      const store = useTrackingStore.getState()

      // ── Bước 1: Tính vị trí mới mỗi STEP_MS ──────────────
      if (now >= nextStepRef.current) {
        nextStepRef.current = now + STEP_MS

        const { nextDistKm, coords, bearing, finished } =
          advanceOnRoute(store.distanceKm, STEP_MS)

        const snapped = snapToRoad(coords)

        store.setDistanceKm(nextDistKm)
        store.setSnappedPosition(snapped, bearing)

        // Chuẩn bị lerp từ vị trí hiện tại → vị trí mới
        fromRef.current  = store.displayPosition ?? snapped
        toRef.current    = snapped
        stepStartRef.current = now

        // ── Bước 2: Geofence check ───────────────────────────
        const nowInZone = new Set<string>()
        MOCK_POIS.forEach((poi) => {
          const distM =
            turf.distance(
              turf.point(snapped),
              turf.point(poi.coordinates),
              { units: 'kilometers' },
            ) * 1000

          if (distM < GEOFENCE_RADIUS_M) {
            nowInZone.add(poi.id)
            if (!inZoneRef.current.has(poi.id)) {
              store.enterGeofence(poi, (p) => {
                triggerAudio(p)
                setSelectedPoi(p)
              })
            }
          } else {
            if (inZoneRef.current.has(poi.id)) {
              store.exitGeofence(poi.id)
            }
          }
        })
        inZoneRef.current = nowInZone

        if (finished) {
          stop()
          return
        }
      }

      // ── Bước 3: Lerp display position mỗi frame ──────────
      if (fromRef.current && toRef.current) {
        const elapsed = now - stepStartRef.current
        const t = Math.min(elapsed / LERP_MS, 1)
        // ease-out: nhanh đầu, chậm cuối
        const eased = 1 - Math.pow(1 - t, 3)

        const display: [number, number] = [
          fromRef.current[0] + (toRef.current[0] - fromRef.current[0]) * eased,
          fromRef.current[1] + (toRef.current[1] - fromRef.current[1]) * eased,
        ]
        store.setDisplayPosition(display)
      }

      rafRef.current = requestAnimationFrame(loop)
    }

    // Khởi động: tính bước đầu tiên ngay lập tức
    nextStepRef.current = 0
    rafRef.current = requestAnimationFrame(loop)
  }

  const stop = () => {
    if (rafRef.current) {
      cancelAnimationFrame(rafRef.current)
      rafRef.current = null
    }
    inZoneRef.current.forEach((id) =>
      useTrackingStore.getState().exitGeofence(id)
    )
    inZoneRef.current.clear()
    setSimulating(false)
  }

  useEffect(() => () => {
    if (rafRef.current) cancelAnimationFrame(rafRef.current)
  }, [])

  return (
    <button
      onClick={isSimulating ? stop : start}
      className={[
        'flex items-center gap-2 px-4 py-2 rounded-full font-semibold text-sm shadow-lg',
        'transition-all active:scale-95 select-none',
        isSimulating
          ? 'bg-red-500 text-white hover:bg-red-600'
          : 'bg-blue-500 text-white hover:bg-blue-600',
      ].join(' ')}
    >
      {isSimulating
        ? <><Square size={16} />Dừng Tour</>
        : <><Play size={16} />Start Tour Simulation</>}
    </button>
  )
}
