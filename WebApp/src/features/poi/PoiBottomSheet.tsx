import { MapPin, Clock, DollarSign, X, Headphones } from 'lucide-react'
import { useAppStore } from '../../stores/useAppStore'

export default function PoiBottomSheet() {
  const { selectedPoi, setSelectedPoi, triggerAudio, isAudioPlaying, currentAudioPoi } =
    useAppStore()

  if (!selectedPoi) return null

  const isThisPoiPlaying = isAudioPlaying && currentAudioPoi?.id === selectedPoi.id

  return (
    // Overlay toàn màn hình, chỉ vùng sheet bắt sự kiện click
    <div className="absolute inset-0 z-10 flex items-end pointer-events-none">
      <div
        className="w-full bg-white rounded-t-2xl shadow-2xl p-5 pointer-events-auto"
        style={{ maxHeight: '65vh', overflowY: 'auto' }}
      >
        {/* Header: tên + badge category + nút đóng */}
        <div className="flex items-start justify-between mb-3">
          <div className="flex-1">
            <span className="inline-block bg-orange-100 text-orange-600 text-xs font-bold px-2 py-0.5 rounded-full mb-1">
              {selectedPoi.category}
            </span>
            <h2 className="text-xl font-bold text-gray-900">{selectedPoi.name}</h2>
          </div>
          <button
            onClick={() => setSelectedPoi(null)}
            className="p-1.5 rounded-full hover:bg-gray-100 text-gray-500 ml-2"
          >
            <X size={20} />
          </button>
        </div>

        {/* Mô tả */}
        <p className="text-gray-600 text-sm mb-4 leading-relaxed">{selectedPoi.description}</p>

        {/* Thông tin chi tiết */}
        <div className="space-y-2 mb-4">
          <div className="flex items-center gap-2 text-sm text-gray-500">
            <MapPin size={15} className="text-orange-500" />
            <span>{selectedPoi.address}</span>
          </div>
          <div className="flex items-center gap-2 text-sm text-gray-500">
            <Clock size={15} className="text-orange-500" />
            <span>{selectedPoi.hours}</span>
          </div>
          <div className="flex items-center gap-2 text-sm text-gray-500">
            <DollarSign size={15} className="text-orange-500" />
            <span>{selectedPoi.priceRange}</span>
          </div>
        </div>

        {/* Nút Audio */}
        <button
          onClick={() => triggerAudio(selectedPoi)}
          className={`
            w-full flex items-center justify-center gap-2
            py-3 rounded-xl font-semibold text-sm
            transition-all active:scale-95
            ${isThisPoiPlaying
              ? 'bg-green-500 text-white'
              : 'bg-orange-500 text-white hover:bg-orange-600'}
          `}
        >
          <Headphones size={18} />
          {isThisPoiPlaying ? '🎵 Đang phát audio...' : 'Nghe Audio Giới Thiệu'}
        </button>
      </div>
    </div>
  )
}
