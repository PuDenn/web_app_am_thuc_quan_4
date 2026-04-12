import { MapPin, Route, Mic, TrendingUp } from 'lucide-react'
import { useAdminStore } from '../stores/useAdminStore'

export default function DashboardPage() {
  const { pois, tours } = useAdminStore()
  const mainPois = pois.filter((p) => p.type === 'main')
  const secPois  = pois.filter((p) => p.type === 'secondary')

  const stats = [
    { label: 'Tổng POI',      value: pois.length,      icon: MapPin,     color: 'bg-orange-50 text-orange-500' },
    { label: 'Điểm chính',    value: mainPois.length,  icon: TrendingUp, color: 'bg-green-50 text-green-500'  },
    { label: 'Điểm phụ',      value: secPois.length,   icon: Mic,        color: 'bg-blue-50 text-blue-500'    },
    { label: 'Tour đang có',  value: tours.length,     icon: Route,      color: 'bg-purple-50 text-purple-500' },
  ]

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-xl font-bold text-gray-800">Tổng quan</h1>
        <p className="text-sm text-gray-400 mt-0.5">Hệ thống Ẩm Thực Quận 4</p>
      </div>

      {/* Stat cards */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        {stats.map(({ label, value, icon: Icon, color }) => (
          <div key={label} className="bg-white rounded-2xl p-5 shadow-sm border border-gray-100">
            <div className={`w-10 h-10 rounded-xl flex items-center justify-center mb-3 ${color}`}>
              <Icon size={18} />
            </div>
            <p className="text-2xl font-bold text-gray-800">{value}</p>
            <p className="text-xs text-gray-400 mt-0.5">{label}</p>
          </div>
        ))}
      </div>

      {/* Recent POIs */}
      <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
        <div className="px-5 py-4 border-b border-gray-50">
          <h2 className="font-semibold text-gray-700 text-sm">POI gần đây</h2>
        </div>
        <table className="w-full text-sm">
          <thead>
            <tr className="text-xs text-gray-400 border-b border-gray-50">
              <th className="text-left px-5 py-3 font-medium">Tên</th>
              <th className="text-left px-5 py-3 font-medium">Loại</th>
              <th className="text-left px-5 py-3 font-medium">Địa chỉ</th>
              <th className="text-left px-5 py-3 font-medium">Tọa độ</th>
            </tr>
          </thead>
          <tbody>
            {pois.slice(0, 5).map((poi) => (
              <tr key={poi.id} className="border-b border-gray-50 last:border-0 hover:bg-gray-50/50">
                <td className="px-5 py-3 font-medium text-gray-700">{poi.name}</td>
                <td className="px-5 py-3">
                  <span className={[
                    'text-xs px-2 py-0.5 rounded-full font-medium',
                    poi.type === 'main'
                      ? 'bg-orange-100 text-orange-600'
                      : 'bg-blue-100 text-blue-600',
                  ].join(' ')}>
                    {poi.type === 'main' ? 'Chính' : 'Phụ'}
                  </span>
                </td>
                <td className="px-5 py-3 text-gray-400 text-xs">{poi.address}</td>
                <td className="px-5 py-3 text-gray-400 text-xs font-mono">
                  {poi.lat.toFixed(4)}, {poi.lng.toFixed(4)}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
