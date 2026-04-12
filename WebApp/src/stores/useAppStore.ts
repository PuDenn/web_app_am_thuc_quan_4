import { create } from 'zustand'

export interface POI {
  id: string
  name: string
  category: string
  description: string
  address: string
  hours: string
  priceRange: string
  audioFile?: string
  coordinates: [number, number]
}

interface AppState {
  userPosition: [number, number] | null
  setUserPosition: (pos: [number, number]) => void
  selectedPoi: POI | null
  setSelectedPoi: (poi: POI | null) => void
  isAudioPlaying: boolean
  currentAudioPoi: POI | null
  setAudioPlaying: (playing: boolean) => void
  triggerAudio: (poi: POI) => void
  stopAudio: () => void
  isSimulating: boolean
  setSimulating: (val: boolean) => void
}

export const useAppStore = create<AppState>((set) => ({
  userPosition: null,
  setUserPosition: (pos) => set({ userPosition: pos }),
  selectedPoi: null,
  setSelectedPoi: (poi) => set({ selectedPoi: poi }),
  isAudioPlaying: false,
  currentAudioPoi: null,
  setAudioPlaying: (playing) => set({ isAudioPlaying: playing }),
  triggerAudio: (poi) => set({ isAudioPlaying: true, currentAudioPoi: poi }),
  stopAudio: () => set({ isAudioPlaying: false, currentAudioPoi: null }),
  isSimulating: false,
  setSimulating: (val) => set({ isSimulating: val }),
}))

/**
 * 5 POI dọc tuyến Đoàn Văn Bơ.
 * Mỗi POI nằm ~18m về phía Bắc hoặc Nam của đường ray
 * → trong bán kính geofence 30m, nhưng vẫn thấy offset trực quan.
 */
export const MOCK_POIS: POI[] = [
  {
    id: 'poi-1',
    name: 'Cơm Tấm Bà Út',
    category: 'Cơm Tấm',
    description: 'Cơm tấm sườn bì chả, nước mắm pha chuẩn vị Sài Gòn xưa.',
    address: '32 Hoàng Diệu, Quận 4',
    hours: '06:00 - 14:00',
    priceRange: '35k - 65k',
    coordinates: [106.7000, 10.7573],  // điểm 07 trên tuyến (giao Hoàng Diệu)
  },
  {
    id: 'poi-2',
    name: 'Bánh Mì Huỳnh Hoa',
    category: 'Bánh Mì',
    description: 'Bánh mì chả lụa thịt nguội nổi danh, thường có hàng dài chờ mua.',
    address: '26 Đoàn Văn Bơ, Quận 4',
    hours: '06:00 - 14:00',
    priceRange: '30k - 45k',
    coordinates: [106.7012, 10.7575],  // điểm 11 trên tuyến
  },
  {
    id: 'poi-3',
    name: 'Ốc Đào',
    category: 'Ốc',
    description: 'Quán ốc nổi tiếng Quận 4, đặc biệt với ốc len xào dừa và nghêu hấp sả.',
    address: '5 Đoàn Văn Bơ, Quận 4',
    hours: '17:00 - 23:00',
    priceRange: '50k - 150k',
    coordinates: [106.7021, 10.7577],  // điểm 14 trên tuyến (giao Vĩnh Khánh)
  },
  {
    id: 'poi-4',
    name: 'Trà Sữa Phúc Long',
    category: 'Trà Sữa',
    description: 'Thương hiệu trà sữa Việt nổi tiếng với trà truyền thống và cà phê.',
    address: '10 Đoàn Văn Bơ, Quận 4',
    hours: '07:00 - 22:00',
    priceRange: '29k - 65k',
    coordinates: [106.7033, 10.7580],  // điểm 18 trên tuyến
  },
  {
    id: 'poi-5',
    name: 'Phở 24',
    category: 'Phở',
    description: 'Phở bò tái nạm gân chuẩn vị, nước dùng ninh từ xương bò 8 tiếng.',
    address: '8 Nguyễn Tất Thành, Quận 4',
    hours: '06:00 - 10:30',
    priceRange: '45k - 75k',
    coordinates: [106.7048, 10.7583],  // điểm 23 trên tuyến
  },
]
