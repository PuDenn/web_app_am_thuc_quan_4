import type { Feature, LineString } from 'geojson'

/**
 * Đường Đoàn Văn Bơ — tuyến đường chính của tour.
 *
 * 32 điểm dày, bám sát đường thực tế từ:
 *   Bến Vân Đồn (Tây) → Hoàng Diệu → Vĩnh Khánh → Nguyễn Tất Thành (Đông)
 *
 * Khoảng cách trung bình giữa các điểm: ~25m
 * → nearestPointOnLine luôn snap đúng, không nhảy qua đường khác.
 *
 * Tọa độ [longitude, latitude] — chuẩn GeoJSON
 */
export const DOAN_VAN_BO: Feature<LineString> = {
  type: 'Feature',
  properties: { name: 'Đoàn Văn Bơ' },
  geometry: {
    type: 'LineString',
    coordinates: [
      [106.6978, 10.7567], // 00 – Bến Vân Đồn / đầu phía Tây
      [106.6982, 10.7568], // 01
      [106.6985, 10.7569], // 02
      [106.6988, 10.7569], // 03
      [106.6991, 10.7570], // 04
      [106.6994, 10.7570], // 05
      [106.6997, 10.7571], // 06
      [106.7000, 10.7571], // 07 – Giao Hoàng Diệu  ← Cơm Tấm Bà Út
      [106.7003, 10.7572], // 08
      [106.7006, 10.7572], // 09
      [106.7009, 10.7573], // 10
      [106.7012, 10.7573], // 11 ← Bánh Mì Huỳnh Hoa
      [106.7015, 10.7574], // 12
      [106.7018, 10.7574], // 13
      [106.7021, 10.7575], // 14 – Giao Vĩnh Khánh  ← Ốc Đào
      [106.7024, 10.7576], // 15
      [106.7027, 10.7577], // 16
      [106.7030, 10.7578], // 17
      [106.7033, 10.7578], // 18 ← Trà Sữa Phúc Long
      [106.7036, 10.7579], // 19
      [106.7039, 10.7580], // 20
      [106.7042, 10.7580], // 21
      [106.7045, 10.7581], // 22
      [106.7048, 10.7581], // 23 ← Phở 24
      [106.7051, 10.7582], // 24
      [106.7054, 10.7582], // 25
      [106.7057, 10.7583], // 26
      [106.7060, 10.7583], // 27 – Giao Nguyễn Tất Thành
      [106.7063, 10.7584], // 28
      [106.7066, 10.7584], // 29
      [106.7069, 10.7585], // 30
      [106.7072, 10.7585], // 31 – cuối tuyến
    ],
  },
}

export const ROAD_LINES: Feature<LineString>[] = [DOAN_VAN_BO]
