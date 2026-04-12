import { useState } from 'react'
import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import {
  LayoutDashboard, MapPin, Route, LogOut, Menu, X, ChefHat,
} from 'lucide-react'
import { useAuthStore } from '../../stores/useAuthStore'

const NAV = [
  { to: '/admin/dashboard', icon: LayoutDashboard, label: 'Tổng quan' },
  { to: '/admin/pois',      icon: MapPin,           label: 'Quản lý POI' },
  { to: '/admin/tours',     icon: Route,            label: 'Quản lý Tour' },
]

export default function AdminLayout() {
  const [sidebarOpen, setSidebarOpen] = useState(true)
  const { logout } = useAuthStore()
  const navigate = useNavigate()

  const handleLogout = () => { logout(); navigate('/login', { replace: true }) }

  return (
    <div className="flex h-screen bg-gray-50 overflow-hidden">

      {/* ── Sidebar ── */}
      <aside className={[
        'flex flex-col bg-gray-900 text-white transition-all duration-300 shrink-0',
        sidebarOpen ? 'w-56' : 'w-16',
      ].join(' ')}>

        {/* Logo */}
        <div className="flex items-center gap-3 px-4 py-5 border-b border-white/10">
          <ChefHat size={22} className="text-orange-400 shrink-0" />
          {sidebarOpen && (
            <div>
              <p className="text-sm font-bold leading-none">Ẩm Thực Q4</p>
              <p className="text-[10px] text-gray-400 mt-0.5">Admin Panel</p>
            </div>
          )}
        </div>

        {/* Nav items */}
        <nav className="flex-1 py-4 space-y-1 px-2">
          {NAV.map(({ to, icon: Icon, label }) => (
            <NavLink
              key={to}
              to={to}
              className={({ isActive }) => [
                'flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition-all',
                isActive
                  ? 'bg-orange-500 text-white'
                  : 'text-gray-400 hover:bg-white/10 hover:text-white',
              ].join(' ')}
            >
              <Icon size={18} className="shrink-0" />
              {sidebarOpen && label}
            </NavLink>
          ))}
        </nav>

        {/* Logout */}
        <div className="p-2 border-t border-white/10">
          <button
            onClick={handleLogout}
            className="flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm text-gray-400
                       hover:bg-white/10 hover:text-white transition-all w-full"
          >
            <LogOut size={18} className="shrink-0" />
            {sidebarOpen && 'Đăng xuất'}
          </button>
        </div>
      </aside>

      {/* ── Main ── */}
      <div className="flex-1 flex flex-col min-w-0 overflow-hidden">

        {/* Topbar */}
        <header className="bg-white border-b border-gray-100 px-6 py-3.5 flex items-center gap-4 shrink-0">
          <button
            onClick={() => setSidebarOpen((v) => !v)}
            className="text-gray-400 hover:text-gray-700 transition-colors"
          >
            {sidebarOpen ? <X size={20} /> : <Menu size={20} />}
          </button>
          <div className="flex-1" />
          <span className="text-xs text-gray-400 bg-gray-100 px-3 py-1 rounded-full">
            Admin Dashboard
          </span>
        </header>

        {/* Page content */}
        <main className="flex-1 overflow-y-auto p-6">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
