import { useState, useEffect, useRef, useCallback } from 'react'
import maplibregl from 'maplibre-gl'
import 'maplibre-gl/dist/maplibre-gl.css'
import { Plus, Pencil, Trash2, MapPin, Filter, AlertTriangle, WifiOff, X, Save, Crosshair } from 'lucide-react'
import { useAdminStore } from '../stores/useAdminStore'
import type { AdminPOI, PoiType } from '../types'
import { POI_TYPE_LABEL } from '../types'
import { setupPMTilesProtocol } from '../../lib/pmtiles-protocol'

const Q4_CENTER: [number, number] = [106.7025, 10.7575]
const EMPTY_FORM: Omit<AdminPOI, 'id' | 'createdAt'> = {
  name: '', type: 'main', lat: 0, lng: 0,
  description: '', address: '', hours: '', priceRange: '',
  audioUrl: '', category: '',
}

// ── Mini map for picking coordinates ─────────────────
function PickerMap({
  lat, lng, onPick, mapError, setMapError,
}: {
  lat: number; lng: number
  onPick: (lat: number, lng: number) => void
  mapError: boolean; setMapError: (v: boolean) => void
}) {
  const containerRef = useRef<HTMLDivElement>(null)
  const mapRef       = useRef<maplibregl.Map | null>(null)
  const markerRef    = useRef<maplibregl.Marker | null>(null)

  useEffect(() => {
    if (!containerRef.current || mapRef.current) return
    setupPMTilesProtocol()

    let m: maplibregl.Map
    try {
      m = new maplibregl.Map({
        container: containerRef.current,
        style: 'https://tiles.openfreemap.org/styles/liberty',
        center: (lat && lng) ? [lng, lat] : Q4_CENTER,
        zoom: 16,
      })

      m.on('error', () => setMapError(true))

      m.on('load', () => {
        setMapError(false)
        // Click để chọn tọa độ
        m.on('click', (e) => {
          const { lng: lngVal, lat: latVal } = e.lngLat
          onPick(parseFloat(latVal.toFixed(6)), parseFloat(lngVal.toFixed(6)))
        })
        m.getCanvas().style.cursor = 'crosshair'

        // Nếu đã có tọa độ, đặt marker
        if (lat && lng) {
          markerRef.current = new maplibregl.Marker({ color: '#f97316' })
            .setLngLat([lng, lat]).addTo(m)
        }
      })
      mapRef.current = m
    } catch {
      setMapError(true)
    }

    return () => { m?.remove(); mapRef.current = null }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  // Cập nhật marker khi lat/lng thay đổi
  useEffect(() => {
    if (!mapRef.current || !lat || !lng) return
    if (markerRef.current) {
      markerRef.current.setLngLat([lng, lat])
    } else {
      markerRef.current = new maplibregl.Marker({ color: '#f97316' })
        .setLngLat([lng, lat]).addTo(mapRef.current)
    }
    mapRef.current.flyTo({ center: [lng, lat], zoom: 16, duration: 400 })
  }, [lat, lng])

  // EC03: Map server lỗi → fallback UI
  if (mapError) {
    return (
      <div className="h-48 rounded-xl border-2 border-dashed border-red-200 bg-red-50
                      flex flex-col items-center justify-center gap-2 text-red-400">
        <WifiOff size={24} />
        <p className="text-xs font-medium">Không thể tải bản đồ</p>
        <p className="text-xs text-red-300">Nhập tọa độ thủ công bên dưới</p>
      </div>
    )
  }

  return (
    <div className="relative">
      <div ref={containerRef} className="h-48 rounded-xl overflow-hidden border border-gray-200" />
      <div className="absolute top-2 left-2 bg-white/90 backdrop-blur-sm text-xs
                      px-2.5 py-1.5 rounded-lg flex items-center gap-1.5 text-gray-600 shadow-sm">
        <Crosshair size={12} className="text-orange-500" />
        Click lên bản đồ để chọn vị trí
      </div>
    </div>
  )
}

// ── POI Editor Modal ──────────────────────────────────
function PoiModal({
  initial, onSave, onClose,
}: {
  initial?: AdminPOI | null
  onSave: (data: Omit<AdminPOI, 'id' | 'createdAt'>) => void
  onClose: () => void
}) {
  const [form, setForm]       = useState<Omit<AdminPOI, 'id' | 'createdAt'>>(
    initial ? { ...initial } : { ...EMPTY_FORM }
  )
  const [errors, setErrors]   = useState<Record<string, string>>({})
  const [mapError, setMapError] = useState(false)

  const set = (key: keyof typeof form) =>
    (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) =>
      setForm((f) => ({ ...f, [key]: e.target.value }))

  const handlePick = useCallback((lat: number, lng: number) => {
    setForm((f) => ({ ...f, lat, lng }))
    setErrors((e) => ({ ...e, coords: '' }))
  }, [])

  const validate = () => {
    const e: Record<string, string> = {}
    if (!form.name.trim())     e.name = 'Tên POI không được để trống'
    // EC01: thiếu tọa độ
    if (!form.lat || !form.lng) e.coords = 'BR01: POI phải có tọa độ — click lên bản đồ hoặc nhập tay'
    if (!form.address.trim())  e.address = 'Địa chỉ không được để trống'
    setErrors(e)
    return Object.keys(e).length === 0
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (validate()) onSave(form)
  }

  const inputCls = (err?: string) => [
    'w-full px-3 py-2.5 rounded-xl border text-sm transition-all',
    'focus:outline-none focus:ring-2 focus:ring-orange-400 focus:border-transparent',
    err ? 'border-red-300 bg-red-50' : 'border-gray-200 bg-gray-50',
  ].join(' ')

  return (
    <div className="fixed inset-0 z-50 flex items-start justify-center bg-black/40 backdrop-blur-sm overflow-y-auto p-4">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-lg my-4">

        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100">
          <h2 className="font-bold text-gray-800">
            {initial ? '✏️ Sửa POI' : '➕ Thêm POI mới'}
          </h2>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-700">
            <X size={20} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-4">

          {/* Tên + Loại */}
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="label-xs">Tên POI *</label>
              <input value={form.name} onChange={set('name')}
                placeholder="Cơm Tấm Bà Út" className={inputCls(errors.name)} />
              {errors.name && <p className="err-txt">{errors.name}</p>}
            </div>
            <div>
              <label className="label-xs">Loại điểm *</label>
              <select value={form.type}
                onChange={(e) => setForm((f) => ({ ...f, type: e.target.value as PoiType }))}
                className={inputCls()}>
                {Object.entries(POI_TYPE_LABEL).map(([k, v]) => (
                  <option key={k} value={k}>{v}</option>
                ))}
              </select>
            </div>
          </div>

          {/* Map picker */}
          <div>
            <label className="label-xs">Vị trí trên bản đồ *</label>
            <PickerMap lat={form.lat} lng={form.lng}
              onPick={handlePick} mapError={mapError} setMapError={setMapError} />

            {/* EC01 error highlight */}
            {errors.coords && (
              <div className="flex items-start gap-2 mt-2 bg-red-50 border border-red-200
                              rounded-xl px-3 py-2.5 text-xs text-red-600">
                <AlertTriangle size={14} className="shrink-0 mt-0.5" />
                {errors.coords}
              </div>
            )}

            {/* Manual input fallback (EC03) */}
            <div className="grid grid-cols-2 gap-2 mt-2">
              <div>
                <label className="label-xs">Latitude</label>
                <input type="number" step="any" value={form.lat || ''}
                  onChange={(e) => setForm((f) => ({ ...f, lat: parseFloat(e.target.value) || 0 }))}
                  placeholder="10.7573" className={inputCls(errors.coords)} />
              </div>
              <div>
                <label className="label-xs">Longitude</label>
                <input type="number" step="any" value={form.lng || ''}
                  onChange={(e) => setForm((f) => ({ ...f, lng: parseFloat(e.target.value) || 0 }))}
                  placeholder="106.7000" className={inputCls(errors.coords)} />
              </div>
            </div>
          </div>

          {/* Địa chỉ + Giờ + Giá */}
          <div>
            <label className="label-xs">Địa chỉ *</label>
            <input value={form.address} onChange={set('address')}
              placeholder="32 Hoàng Diệu, Quận 4" className={inputCls(errors.address)} />
            {errors.address && <p className="err-txt">{errors.address}</p>}
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="label-xs">Giờ mở cửa</label>
              <input value={form.hours} onChange={set('hours')}
                placeholder="06:00 - 14:00" className={inputCls()} />
            </div>
            <div>
              <label className="label-xs">Giá (VNĐ)</label>
              <input value={form.priceRange} onChange={set('priceRange')}
                placeholder="35k - 65k" className={inputCls()} />
            </div>
          </div>

          <div>
            <label className="label-xs">Mô tả</label>
            <textarea value={form.description} onChange={set('description')}
              rows={2} placeholder="Mô tả ngắn về địa điểm..."
              className={`${inputCls()} resize-none`} />
          </div>

          <div>
            <label className="label-xs">URL Audio</label>
            <input value={form.audioUrl} onChange={set('audioUrl')}
              placeholder="https://..." className={inputCls()} />
          </div>

          {/* Actions */}
          <div className="flex gap-3 pt-2">
            <button type="button" onClick={onClose}
              className="flex-1 py-3 rounded-xl border border-gray-200 text-sm text-gray-600
                         hover:bg-gray-50 transition-colors font-medium">
              Huỷ
            </button>
            <button type="submit"
              className="flex-1 py-3 rounded-xl bg-orange-500 hover:bg-orange-600
                         text-white text-sm font-bold transition-colors flex items-center justify-center gap-2">
              <Save size={15} /> Lưu POI
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

// ── Main Page ─────────────────────────────────────────
export default function PoiManagement() {
  const { pois, addPoi, updatePoi, deletePoi } = useAdminStore()
  const [filterType, setFilterType] = useState<PoiType | 'all'>('all')
  const [modal, setModal]           = useState<'new' | AdminPOI | null>(null)
  const [deleteConfirm, setDeleteConfirm] = useState<string | null>(null)

  const filtered = filterType === 'all'
    ? pois : pois.filter((p) => p.type === filterType)

  const handleSave = (data: Omit<AdminPOI, 'id' | 'createdAt'>) => {
    if (modal === 'new') {
      addPoi({ ...data, id: `poi-${Date.now()}`, createdAt: new Date().toISOString() })
    } else if (modal && typeof modal === 'object') {
      updatePoi(modal.id, data)
    }
    setModal(null)
  }

  return (
    <div className="space-y-5">

      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-bold text-gray-800">Quản lý POI</h1>
          <p className="text-xs text-gray-400 mt-0.5">{pois.length} địa điểm</p>
        </div>
        <button onClick={() => setModal('new')}
          className="flex items-center gap-2 bg-orange-500 hover:bg-orange-600
                     text-white text-sm font-semibold px-4 py-2.5 rounded-xl transition-colors shadow-sm">
          <Plus size={16} /> Thêm POI
        </button>
      </div>

      {/* Filter tabs */}
      <div className="flex gap-2">
        {([['all', 'Tất cả'], ['main', '🍽️ Điểm chính'], ['secondary', '🚻 Điểm phụ']] as const).map(([v, label]) => (
          <button key={v} onClick={() => setFilterType(v)}
            className={[
              'px-4 py-2 rounded-xl text-sm font-medium transition-all',
              filterType === v
                ? 'bg-orange-500 text-white shadow-sm'
                : 'bg-white text-gray-500 border border-gray-200 hover:border-orange-300',
            ].join(' ')}>
            <Filter size={13} className="inline mr-1.5" />
            {label}
          </button>
        ))}
      </div>

      {/* Table */}
      <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="text-xs text-gray-400 border-b border-gray-50 bg-gray-50/50">
              {['Tên POI', 'Loại', 'Địa chỉ', 'Tọa độ', 'Giờ', ''].map((h) => (
                <th key={h} className="text-left px-5 py-3 font-medium">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {filtered.map((poi) => (
              <tr key={poi.id} className="border-b border-gray-50 last:border-0 hover:bg-orange-50/30 transition-colors">
                <td className="px-5 py-3.5">
                  <p className="font-semibold text-gray-800">{poi.name}</p>
                  <p className="text-xs text-gray-400">{poi.category}</p>
                </td>
                <td className="px-5 py-3.5">
                  <span className={[
                    'text-xs px-2.5 py-1 rounded-full font-semibold',
                    poi.type === 'main'
                      ? 'bg-orange-100 text-orange-600'
                      : 'bg-blue-100 text-blue-600',
                  ].join(' ')}>
                    {poi.type === 'main' ? 'Chính' : 'Phụ'}
                  </span>
                </td>
                <td className="px-5 py-3.5 text-gray-500 text-xs max-w-[140px] truncate">{poi.address}</td>
                <td className="px-5 py-3.5 font-mono text-xs text-gray-400">
                  {poi.lat.toFixed(4)},<br />{poi.lng.toFixed(4)}
                </td>
                <td className="px-5 py-3.5 text-gray-400 text-xs">{poi.hours}</td>
                <td className="px-5 py-3.5">
                  <div className="flex items-center gap-2">
                    <button onClick={() => setModal(poi)}
                      className="p-1.5 rounded-lg text-gray-400 hover:bg-orange-100 hover:text-orange-600 transition-colors">
                      <Pencil size={14} />
                    </button>
                    <button onClick={() => setDeleteConfirm(poi.id)}
                      className="p-1.5 rounded-lg text-gray-400 hover:bg-red-100 hover:text-red-600 transition-colors">
                      <Trash2 size={14} />
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        {filtered.length === 0 && (
          <div className="py-16 text-center text-gray-300">
            <MapPin size={32} className="mx-auto mb-2" />
            <p className="text-sm">Không có POI nào</p>
          </div>
        )}
      </div>

      {/* POI Modal */}
      {modal !== null && (
        <PoiModal
          initial={modal === 'new' ? null : modal}
          onSave={handleSave}
          onClose={() => setModal(null)}
        />
      )}

      {/* Delete confirm */}
      {deleteConfirm && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-white rounded-2xl p-6 shadow-2xl w-80">
            <div className="text-center mb-4">
              <div className="w-12 h-12 bg-red-100 rounded-full flex items-center justify-center mx-auto mb-3">
                <Trash2 size={22} className="text-red-500" />
              </div>
              <h3 className="font-bold text-gray-800">Xóa POI này?</h3>
              <p className="text-xs text-gray-400 mt-1">POI sẽ bị xóa khỏi tất cả Tour liên quan.</p>
            </div>
            <div className="flex gap-3">
              <button onClick={() => setDeleteConfirm(null)}
                className="flex-1 py-2.5 rounded-xl border border-gray-200 text-sm font-medium text-gray-600">
                Huỷ
              </button>
              <button onClick={() => { deletePoi(deleteConfirm); setDeleteConfirm(null) }}
                className="flex-1 py-2.5 rounded-xl bg-red-500 text-white text-sm font-bold">
                Xóa
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
