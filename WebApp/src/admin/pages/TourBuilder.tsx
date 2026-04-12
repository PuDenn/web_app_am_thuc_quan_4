import { useState } from 'react'
import {
  DndContext, DragOverlay, PointerSensor, useSensor, useSensors,
  closestCenter, type DragStartEvent, type DragEndEvent,
} from '@dnd-kit/core'
import {
  SortableContext, verticalListSortingStrategy,
  useSortable, arrayMove,
} from '@dnd-kit/sortable'
import { CSS } from '@dnd-kit/utilities'
import {
  Plus, GripVertical, X, Clock, Save, AlertTriangle,
  CheckCircle, Route, MapPin, Trash2,
} from 'lucide-react'
import { useAdminStore } from '../stores/useAdminStore'
import type { AdminPOI, AdminTour } from '../types'

// ── Sortable POI item (trong Tour) ────────────────────
function SortablePoiItem({
  poi, index, onRemove,
}: { poi: AdminPOI; index: number; onRemove: () => void }) {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } =
    useSortable({ id: poi.id })

  return (
    <div
      ref={setNodeRef}
      style={{ transform: CSS.Transform.toString(transform), transition }}
      className={[
        'flex items-center gap-3 p-3 rounded-xl border bg-white transition-shadow',
        isDragging
          ? 'shadow-lg border-orange-300 opacity-80'
          : 'border-gray-200 hover:border-orange-200',
      ].join(' ')}
    >
      {/* Step number */}
      <span className="w-6 h-6 rounded-full bg-orange-500 text-white text-xs
                       flex items-center justify-center font-bold shrink-0">
        {index + 1}
      </span>

      {/* Drag handle */}
      <button {...attributes} {...listeners}
        className="text-gray-300 hover:text-gray-500 cursor-grab active:cursor-grabbing touch-none">
        <GripVertical size={16} />
      </button>

      <div className="flex-1 min-w-0">
        <p className="text-sm font-semibold text-gray-800 truncate">{poi.name}</p>
        <p className="text-xs text-gray-400 truncate">{poi.address}</p>
      </div>

      <span className={[
        'text-xs px-2 py-0.5 rounded-full font-medium shrink-0',
        poi.type === 'main' ? 'bg-orange-100 text-orange-600' : 'bg-blue-100 text-blue-600',
      ].join(' ')}>
        {poi.type === 'main' ? 'Chính' : 'Phụ'}
      </span>

      <button onClick={onRemove}
        className="text-gray-300 hover:text-red-500 transition-colors shrink-0">
        <X size={15} />
      </button>
    </div>
  )
}

// ── Available POI item (cột trái) ─────────────────────
function AvailablePoiItem({
  poi, onAdd, alreadyAdded,
}: { poi: AdminPOI; onAdd: () => void; alreadyAdded: boolean }) {
  return (
    <div className={[
      'flex items-center gap-3 p-3 rounded-xl border transition-all',
      alreadyAdded
        ? 'border-green-200 bg-green-50'
        : 'border-gray-200 bg-white hover:border-orange-300 hover:shadow-sm',
    ].join(' ')}>
      <MapPin size={15} className={alreadyAdded ? 'text-green-400' : 'text-gray-300'} />
      <div className="flex-1 min-w-0">
        <p className={`text-sm font-medium truncate ${alreadyAdded ? 'text-green-700' : 'text-gray-700'}`}>
          {poi.name}
        </p>
        <p className="text-xs text-gray-400 truncate">{poi.category}</p>
      </div>
      {alreadyAdded
        ? <CheckCircle size={16} className="text-green-400 shrink-0" />
        : (
          <button onClick={onAdd}
            className="text-orange-500 hover:bg-orange-100 p-1 rounded-lg transition-colors shrink-0">
            <Plus size={16} />
          </button>
        )
      }
    </div>
  )
}

// ── Tour Card (danh sách) ─────────────────────────────
function TourCard({
  tour, pois, onEdit, onDelete,
}: { tour: AdminTour; pois: AdminPOI[]; onEdit: () => void; onDelete: () => void }) {
  const tourPois = tour.poiIds.map((id) => pois.find((p) => p.id === id)).filter(Boolean) as AdminPOI[]
  return (
    <div className="bg-white rounded-2xl border border-gray-100 shadow-sm p-5">
      <div className="flex items-start justify-between mb-3">
        <div>
          <h3 className="font-bold text-gray-800">{tour.title}</h3>
          <p className="text-xs text-gray-400 mt-0.5">{tour.description}</p>
        </div>
        <div className="flex gap-2">
          <button onClick={onEdit}
            className="text-xs px-3 py-1.5 rounded-lg border border-orange-200 text-orange-600
                       hover:bg-orange-50 transition-colors font-medium">
            Sửa
          </button>
          <button onClick={onDelete}
            className="text-xs px-3 py-1.5 rounded-lg border border-red-200 text-red-500
                       hover:bg-red-50 transition-colors font-medium">
            <Trash2 size={13} />
          </button>
        </div>
      </div>

      <div className="flex items-center gap-3 text-xs text-gray-400 mb-3">
        <span className="flex items-center gap-1"><MapPin size={12} />{tourPois.length} POI</span>
        <span className="flex items-center gap-1"><Clock size={12} />{tour.estimatedTime} phút</span>
      </div>

      <div className="flex gap-1.5 flex-wrap">
        {tourPois.map((p) => (
          <span key={p.id} className="text-xs bg-gray-100 text-gray-600 px-2 py-0.5 rounded-full">
            {p.name}
          </span>
        ))}
      </div>
    </div>
  )
}

// ── Tour Editor Modal ─────────────────────────────────
function TourEditorModal({
  initial, allPois, onSave, onClose,
}: {
  initial: AdminTour | null
  allPois: AdminPOI[]
  onSave: (data: Omit<AdminTour, 'id' | 'createdAt'>) => void
  onClose: () => void
}) {
  const [title, setTitle]       = useState(initial?.title ?? '')
  const [desc, setDesc]         = useState(initial?.description ?? '')
  const [time, setTime]         = useState(initial?.estimatedTime ?? 60)
  const [poiIds, setPoiIds]     = useState<string[]>(initial?.poiIds ?? [])
  const [errors, setErrors]     = useState<string[]>([])
  const [activeId, setActiveId] = useState<string | null>(null)

  const sensors = useSensors(useSensor(PointerSensor, {
    activationConstraint: { distance: 5 },
  }))

  const tourPois = poiIds.map((id) => allPois.find((p) => p.id === id)).filter(Boolean) as AdminPOI[]

  // EC02: Kiểm tra trùng lặp POI
  const addPoi = (id: string) => {
    if (poiIds.includes(id)) {
      setErrors(['EC02: POI này đã có trong Tour. Mỗi POI chỉ xuất hiện 1 lần.'])
      setTimeout(() => setErrors([]), 3000)
      return
    }
    setPoiIds((prev) => [...prev, id])
    setErrors([])
  }

  const removePoi = (id: string) => setPoiIds((prev) => prev.filter((p) => p !== id))

  const handleDragStart = (e: DragStartEvent) => setActiveId(e.active.id as string)

  const handleDragEnd = (e: DragEndEvent) => {
    setActiveId(null)
    const { active, over } = e
    if (!over || active.id === over.id) return
    setPoiIds((ids) => {
      const from = ids.indexOf(active.id as string)
      const to   = ids.indexOf(over.id as string)
      return arrayMove(ids, from, to)
    })
  }

  const validate = () => {
    const errs: string[] = []
    if (!title.trim()) errs.push('Tiêu đề Tour không được để trống')
    // BR03: tối thiểu 2 POI
    if (poiIds.length < 2) errs.push('BR03: Tour phải có tối thiểu 2 POI')
    setErrors(errs)
    return errs.length === 0
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (validate()) onSave({ title, description: desc, poiIds, estimatedTime: time })
  }

  const dragPoi = activeId ? allPois.find((p) => p.id === activeId) : null

  return (
    <div className="fixed inset-0 z-50 flex items-start justify-center bg-black/40 backdrop-blur-sm overflow-y-auto p-4">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-3xl my-4">

        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100">
          <h2 className="font-bold text-gray-800 flex items-center gap-2">
            <Route size={18} className="text-orange-500" />
            {initial ? 'Sửa Tour' : 'Tạo Tour mới'}
          </h2>
          <button onClick={onClose}><X size={20} className="text-gray-400" /></button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-5">

          {/* Error banner */}
          {errors.length > 0 && (
            <div className="bg-red-50 border border-red-200 rounded-xl px-4 py-3 space-y-1">
              {errors.map((e) => (
                <div key={e} className="flex items-center gap-2 text-sm text-red-600">
                  <AlertTriangle size={14} className="shrink-0" /> {e}
                </div>
              ))}
            </div>
          )}

          {/* Meta */}
          <div className="grid grid-cols-3 gap-3">
            <div className="col-span-2">
              <label className="label-xs">Tiêu đề Tour *</label>
              <input value={title} onChange={(e) => setTitle(e.target.value)}
                placeholder="Sáng sớm Quận 4" className="input-base w-full" />
            </div>
            <div>
              <label className="label-xs">Thời lượng (phút)</label>
              <input type="number" value={time} min={10}
                onChange={(e) => setTime(parseInt(e.target.value) || 60)}
                className="input-base w-full" />
            </div>
          </div>
          <div>
            <label className="label-xs">Mô tả</label>
            <input value={desc} onChange={(e) => setDesc(e.target.value)}
              placeholder="Hành trình ăn sáng..." className="input-base w-full" />
          </div>

          {/* Two-column DnD builder */}
          <div className="grid grid-cols-2 gap-4">

            {/* Left: Available POIs */}
            <div>
              <p className="label-xs mb-2">Danh sách POI có sẵn</p>
              <div className="space-y-2 max-h-72 overflow-y-auto pr-1">
                {allPois.map((poi) => (
                  <AvailablePoiItem
                    key={poi.id} poi={poi}
                    alreadyAdded={poiIds.includes(poi.id)}
                    onAdd={() => addPoi(poi.id)}
                  />
                ))}
              </div>
            </div>

            {/* Right: Tour route (sortable) */}
            <div>
              <p className="label-xs mb-2">
                Lộ trình Tour{' '}
                <span className={`font-bold ${poiIds.length < 2 ? 'text-red-400' : 'text-green-500'}`}>
                  ({poiIds.length} / tối thiểu 2)
                </span>
              </p>

              <DndContext
                sensors={sensors}
                collisionDetection={closestCenter}
                onDragStart={handleDragStart}
                onDragEnd={handleDragEnd}
              >
                <SortableContext items={poiIds} strategy={verticalListSortingStrategy}>
                  <div className="space-y-2 min-h-32 max-h-72 overflow-y-auto pr-1">
                    {tourPois.length === 0 ? (
                      <div className="border-2 border-dashed border-gray-200 rounded-xl
                                      py-10 text-center text-gray-300 text-xs">
                        <Route size={24} className="mx-auto mb-2" />
                        Click ➕ để thêm POI vào Tour
                      </div>
                    ) : (
                      tourPois.map((poi, i) => (
                        <SortablePoiItem key={poi.id} poi={poi} index={i}
                          onRemove={() => removePoi(poi.id)} />
                      ))
                    )}
                  </div>
                </SortableContext>

                {/* Drag overlay */}
                <DragOverlay>
                  {dragPoi && (
                    <div className="flex items-center gap-3 p-3 rounded-xl border border-orange-300
                                    bg-white shadow-xl opacity-90">
                      <GripVertical size={16} className="text-gray-400" />
                      <span className="text-sm font-semibold text-gray-800">{dragPoi.name}</span>
                    </div>
                  )}
                </DragOverlay>
              </DndContext>
            </div>
          </div>

          {/* Actions */}
          <div className="flex gap-3 pt-2 border-t border-gray-100">
            <button type="button" onClick={onClose}
              className="flex-1 py-3 rounded-xl border border-gray-200 text-sm text-gray-600
                         hover:bg-gray-50 font-medium transition-colors">
              Huỷ
            </button>
            <button type="submit"
              className="flex-1 py-3 rounded-xl bg-orange-500 hover:bg-orange-600
                         text-white text-sm font-bold transition-colors flex items-center justify-center gap-2">
              <Save size={15} /> Lưu Tour
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

// ── Main Page ─────────────────────────────────────────
export default function TourBuilder() {
  const { tours, pois, addTour, updateTour, deleteTour } = useAdminStore()
  const [modal, setModal]   = useState<AdminTour | 'new' | null>(null)
  const [delId, setDelId]   = useState<string | null>(null)

  const handleSave = (data: Omit<AdminTour, 'id' | 'createdAt'>) => {
    if (modal === 'new') {
      addTour({ ...data, id: `tour-${Date.now()}`, createdAt: new Date().toISOString() })
    } else if (modal && typeof modal === 'object') {
      updateTour(modal.id, data)
    }
    setModal(null)
  }

  return (
    <div className="space-y-5">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-bold text-gray-800">Quản lý Tour</h1>
          <p className="text-xs text-gray-400 mt-0.5">{tours.length} tour đang có</p>
        </div>
        <button onClick={() => setModal('new')}
          className="flex items-center gap-2 bg-orange-500 hover:bg-orange-600
                     text-white text-sm font-semibold px-4 py-2.5 rounded-xl transition-colors shadow-sm">
          <Plus size={16} /> Tạo Tour
        </button>
      </div>

      {tours.length === 0 ? (
        <div className="bg-white rounded-2xl border border-dashed border-gray-200 py-20 text-center">
          <Route size={36} className="mx-auto text-gray-200 mb-3" />
          <p className="text-sm text-gray-400">Chưa có Tour nào. Tạo Tour đầu tiên!</p>
        </div>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2">
          {tours.map((t) => (
            <TourCard key={t.id} tour={t} pois={pois}
              onEdit={() => setModal(t)}
              onDelete={() => setDelId(t.id)} />
          ))}
        </div>
      )}

      {modal !== null && (
        <TourEditorModal
          initial={modal === 'new' ? null : modal}
          allPois={pois}
          onSave={handleSave}
          onClose={() => setModal(null)}
        />
      )}

      {delId && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-white rounded-2xl p-6 shadow-2xl w-80 text-center">
            <div className="w-12 h-12 bg-red-100 rounded-full flex items-center justify-center mx-auto mb-3">
              <Trash2 size={22} className="text-red-500" />
            </div>
            <h3 className="font-bold text-gray-800 mb-1">Xóa Tour này?</h3>
            <p className="text-xs text-gray-400 mb-4">Hành động này không thể hoàn tác.</p>
            <div className="flex gap-3">
              <button onClick={() => setDelId(null)}
                className="flex-1 py-2.5 rounded-xl border border-gray-200 text-sm font-medium text-gray-600">
                Huỷ
              </button>
              <button onClick={() => { deleteTour(delId); setDelId(null) }}
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
