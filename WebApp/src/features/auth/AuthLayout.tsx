import { Outlet, Link, useLocation } from 'react-router-dom'
import { motion, AnimatePresence } from 'framer-motion'

/**
 * AuthLayout
 * ─────────────────────────────────────────────────
 * Shell bọc trang Login / Register.
 * AnimatePresence đảm bảo transition mượt khi chuyển giữa 2 trang.
 */
export default function AuthLayout() {
  const location = useLocation()

  return (
    <div className="min-h-screen flex flex-col bg-gradient-to-br from-orange-50 via-white to-orange-50">

      {/* Header */}
      <header className="pt-12 pb-6 text-center px-6">
        <motion.div
          initial={{ scale: 0.8, opacity: 0 }}
          animate={{ scale: 1, opacity: 1 }}
          transition={{ duration: 0.5, ease: 'backOut' }}
        >
          <div className="text-5xl mb-2">🍜</div>
          <h1 className="text-2xl font-extrabold text-orange-500 tracking-tight">
            Ẩm Thực Quận 4
          </h1>
          <p className="text-sm text-gray-400 mt-1">Tour guide ẩm thực trong lòng bàn tay</p>
        </motion.div>
      </header>

      {/* Tab switcher Login / Register */}
      <div className="flex mx-6 mb-6 bg-gray-100 rounded-2xl p-1">
        {[
          { label: 'Đăng nhập', to: '/login' },
          { label: 'Đăng ký',   to: '/register' },
        ].map(({ label, to }) => {
          const active = location.pathname === to
          return (
            <Link
              key={to}
              to={to}
              className={[
                'flex-1 text-center text-sm font-semibold py-2.5 rounded-xl transition-all',
                active
                  ? 'bg-white text-orange-500 shadow-sm'
                  : 'text-gray-400 hover:text-gray-600',
              ].join(' ')}
            >
              {label}
            </Link>
          )
        })}
      </div>

      {/* Animated page content */}
      <div className="flex-1 px-6 pb-10 overflow-hidden">
        <AnimatePresence mode="wait">
          <motion.div
            key={location.pathname}
            initial={{ opacity: 0, y: 18 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -18 }}
            transition={{ duration: 0.22, ease: 'easeOut' }}
          >
            <Outlet />
          </motion.div>
        </AnimatePresence>
      </div>
    </div>
  )
}
