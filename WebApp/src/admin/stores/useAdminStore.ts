import { create } from 'zustand'
import type { AdminPOI, AdminTour } from '../types'

// ── Seed mock data ────────────────────────────────────
const SEED_POIS: AdminPOI[] = [
  {
    id: 'poi-1', name: 'Cơm Tấm Bà Út', type: 'main',
    lat: 10.7573, lng: 106.7000,
    description: 'Cơm tấm sườn bì chả chuẩn vị Sài Gòn',
    address: '32 Hoàng Diệu, Q4', hours: '06:00-14:00', priceRange: '35k-65k',
    audioUrl: '', category: 'Cơm Tấm', createdAt: '2025-01-10T08:00:00Z',
  },
  {
    id: 'poi-2', name: 'Bánh Mì Huỳnh Hoa', type: 'main',
    lat: 10.7575, lng: 106.7012,
    description: 'Bánh mì chả lụa thịt nguội nổi danh',
    address: '26 Đoàn Văn Bơ, Q4', hours: '06:00-14:00', priceRange: '30k-45k',
    audioUrl: '', category: 'Bánh Mì', createdAt: '2025-01-11T08:00:00Z',
  },
  {
    id: 'poi-3', name: 'Ốc Đào', type: 'main',
    lat: 10.7577, lng: 106.7021,
    description: 'Ốc len xào dừa, nghêu hấp sả',
    address: '5 Đoàn Văn Bơ, Q4', hours: '17:00-23:00', priceRange: '50k-150k',
    audioUrl: '', category: 'Ốc', createdAt: '2025-01-12T08:00:00Z',
  },
  {
    id: 'poi-4', name: 'WC Công Cộng Hoàng Diệu', type: 'secondary',
    lat: 10.7570, lng: 106.7003,
    description: 'Nhà vệ sinh công cộng, sạch sẽ',
    address: 'Hoàng Diệu, Q4', hours: '06:00-22:00', priceRange: 'Miễn phí',
    audioUrl: '', category: 'Tiện ích', createdAt: '2025-01-13T08:00:00Z',
  },
  {
    id: 'poi-5', name: 'Trà Sữa Phúc Long', type: 'main',
    lat: 10.7580, lng: 106.7033,
    description: 'Trà sữa và cà phê Việt Nam',
    address: '10 Đoàn Văn Bơ, Q4', hours: '07:00-22:00', priceRange: '29k-65k',
    audioUrl: '', category: 'Trà Sữa', createdAt: '2025-01-14T08:00:00Z',
  },
]

const SEED_TOURS: AdminTour[] = [
  {
    id: 'tour-1', title: 'Sáng sớm Quận 4',
    description: 'Hành trình ăn sáng từ 6h-9h',
    poiIds: ['poi-1', 'poi-2'], estimatedTime: 60,
    createdAt: '2025-02-01T06:00:00Z',
  },
]

// ─────────────────────────────────────────────────────
interface AdminState {
  pois:  AdminPOI[]
  tours: AdminTour[]

  // POI CRUD
  addPoi:    (poi: AdminPOI) => void
  updatePoi: (id: string, data: Partial<AdminPOI>) => void
  deletePoi: (id: string) => void

  // Tour CRUD
  addTour:    (tour: AdminTour) => void
  updateTour: (id: string, data: Partial<AdminTour>) => void
  deleteTour: (id: string) => void
}

export const useAdminStore = create<AdminState>((set) => ({
  pois:  SEED_POIS,
  tours: SEED_TOURS,

  addPoi:    (poi)         => set((s) => ({ pois: [...s.pois, poi] })),
  updatePoi: (id, data)    => set((s) => ({
    pois: s.pois.map((p) => p.id === id ? { ...p, ...data } : p),
  })),
  deletePoi: (id)          => set((s) => ({
    pois: s.pois.filter((p) => p.id !== id),
    // Xóa POI khỏi tất cả Tour liên quan
    tours: s.tours.map((t) => ({ ...t, poiIds: t.poiIds.filter((pid) => pid !== id) })),
  })),

  addTour:    (tour)    => set((s) => ({ tours: [...s.tours, tour] })),
  updateTour: (id, data) => set((s) => ({
    tours: s.tours.map((t) => t.id === id ? { ...t, ...data } : t),
  })),
  deleteTour: (id) => set((s) => ({ tours: s.tours.filter((t) => t.id !== id) })),
}))
