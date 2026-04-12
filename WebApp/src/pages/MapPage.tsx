import { useNavigate } from 'react-router-dom'
import { LogOut } from 'lucide-react'
import { useAuthStore } from '../stores/useAuthStore'
import MainMap from '../features/map/MainMap'
import PoiBottomSheet from '../features/poi/PoiBottomSheet'
import AudioPlayer from '../features/audio/AudioPlayer'
import SimulationTool from '../features/tracking/SimulationTool'

export default function MapPage() {
  const { user, logout } = useAuthStore()
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/login', { replace: true })
  }

  return (
    <div className="relative bg-gray-100 overflow-hidden" style={{ width: '100vw', height: '100dvh' }}>

      {/* Layer 1: Bản đồ full screen */}
      <div className="absolute inset-0">
        <MainMap />
      </div>

      {/* Layer 2: Header */}
      <div className="absolute top-0 left-0 right-0 z-10 px-4 pt-4 pb-8
                      bg-gradient-to-b from-black/50 to-transparent pointer-events-none">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-white text-lg font-bold drop-shadow">🍜 Ẩm Thực Quận 4</h1>
            <p className="text-white/60 text-xs">Xin chào, {user?.name ?? 'Bạn'} 👋</p>
          </div>
          {/* Logout button — pointer-events-auto để bắt click */}
          <button
            onClick={handleLogout}
            className="pointer-events-auto flex items-center gap-1.5 bg-black/30 backdrop-blur-sm
                       text-white/80 text-xs px-3 py-2 rounded-full hover:bg-black/50 transition-colors"
          >
            <LogOut size={13} />
            Đăng xuất
          </button>
        </div>
      </div>

      {/* Layer 3: Simulation button */}
      <div className="absolute top-20 left-4 z-10">
        <SimulationTool />
      </div>

      {/* Layer 4: Bottom Sheet */}
      <PoiBottomSheet />

      {/* Layer 5: Audio Player */}
      <AudioPlayer />
    </div>
  )
}
