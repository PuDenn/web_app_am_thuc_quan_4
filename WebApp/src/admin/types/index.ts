// ─────────────────────────────────────────────────────
// Data Schema
// ─────────────────────────────────────────────────────

/** BR02: Phân loại POI */
export type PoiType = 'main' | 'secondary'

export const POI_TYPE_LABEL: Record<PoiType, string> = {
  main:      '🍽️ Điểm chính',
  secondary: '🚻 Điểm phụ',
}

export interface AdminPOI {
  id:          string
  name:        string
  type:        PoiType
  lat:         number        // BR01: bắt buộc
  lng:         number        // BR01: bắt buộc
  description: string
  address:     string
  hours:       string
  priceRange:  string
  audioUrl:    string        // URL file audio giới thiệu
  category:    string        // Ốc / Cơm Tấm / Trà Sữa…
  createdAt:   string        // ISO date string
}

export interface AdminTour {
  id:            string
  title:         string
  description:   string
  poiIds:        string[]    // thứ tự đã sắp xếp (BR03: tối thiểu 2)
  estimatedTime: number      // phút
  createdAt:     string
}

// ─────────────────────────────────────────────────────
// Screen List (Artifacts)
// ─────────────────────────────────────────────────────
/**
 * SCREEN LIST — Admin Dashboard
 *
 * 1. /admin/login          — Đăng nhập admin
 * 2. /admin/dashboard      — Overview: tổng POI, Tour, hoạt động gần đây
 * 3. /admin/pois           — POI List (filter Chính/Phụ, search, CRUD)
 * 4. /admin/pois/new       — POI Editor + MapLibre pick-location
 * 5. /admin/pois/:id/edit  — POI Editor (edit mode)
 * 6. /admin/tours          — Tour List
 * 7. /admin/tours/new      — Tour Builder (DnD)
 * 8. /admin/tours/:id/edit — Tour Builder (edit mode)
 *
 * BUSINESS RULES
 * BR01: POI phải có lat/lng hợp lệ trước khi save
 * BR02: POI.type = 'main' | 'secondary'
 * BR03: Tour.poiIds.length >= 2; drag-drop để sắp xếp
 *
 * EDGE CASES
 * EC01: Lưu POI thiếu tọa độ → validation error, highlight map
 * EC02: Tạo Tour với POI trùng lặp → toast warning, dedup tự động
 * EC03: Map server lỗi → fallback UI, disable pick-location, cho nhập tay
 */
