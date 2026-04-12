import { useState } from 'react'
import { useNavigate, useLocation, Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import { Phone, Lock, Eye, EyeOff, LogIn, AlertCircle } from 'lucide-react'
import { useAuthStore } from '../stores/useAuthStore'
import { mockLogin } from '../lib/mockApi'

export default function LoginPage() {
  const navigate = useNavigate()
  const location = useLocation()
  const { loginSuccess, setLoading, setError, isLoading, error } = useAuthStore()

  const [credential, setCredential] = useState('')
  const [password,   setPassword]   = useState('')
  const [showPass,   setShowPass]   = useState(false)

  // Sau login → về trang trước đó hoặc /map
  const from = (location.state as { from?: { pathname: string } })?.from?.pathname ?? '/map'

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!credential.trim() || !password) {
      setError('Vui lòng nhập đầy đủ thông tin.')
      return
    }

    setLoading(true)
    setError(null)
    try {
      const { user, token } = await mockLogin({ credential, password })
      loginSuccess(user, token)
      navigate(from, { replace: true })
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Đăng nhập thất bại.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div>
      {/* Hint tài khoản demo */}
      <motion.div
        initial={{ opacity: 0, scale: 0.95 }}
        animate={{ opacity: 1, scale: 1 }}
        className="bg-orange-50 border border-orange-200 rounded-2xl p-4 mb-6"
      >
        <p className="text-xs text-orange-700 font-semibold mb-1">🧪 Tài khoản demo</p>
        <p className="text-xs text-orange-600 font-mono">user@quan4.vn / 123456</p>
      </motion.div>

      <form onSubmit={handleSubmit} className="space-y-4">

        {/* Error banner */}
        {error && (
          <motion.div
            initial={{ opacity: 0, y: -6 }}
            animate={{ opacity: 1, y: 0 }}
            className="flex items-center gap-2 bg-red-50 border border-red-200 text-red-600 text-sm rounded-xl px-4 py-3"
          >
            <AlertCircle size={16} className="flex-shrink-0" />
            {error}
          </motion.div>
        )}

        {/* Email / SĐT */}
        <div>
          <label className="block text-xs font-semibold text-gray-500 mb-1.5 ml-1">
            Email hoặc Số điện thoại
          </label>
          <div className="relative">
            <Phone size={17} className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400" />
            <input
              type="text"
              value={credential}
              onChange={(e) => setCredential(e.target.value)}
              placeholder="user@quan4.vn hoặc 0901..."
              autoComplete="username"
              className="w-full pl-11 pr-4 py-3.5 rounded-xl border border-gray-200 bg-gray-50
                         text-sm focus:outline-none focus:ring-2 focus:ring-orange-400
                         focus:border-transparent transition-all placeholder:text-gray-300"
            />
          </div>
        </div>

        {/* Password */}
        <div>
          <label className="block text-xs font-semibold text-gray-500 mb-1.5 ml-1">
            Mật khẩu
          </label>
          <div className="relative">
            <Lock size={17} className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400" />
            <input
              type={showPass ? 'text' : 'password'}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="••••••"
              autoComplete="current-password"
              className="w-full pl-11 pr-12 py-3.5 rounded-xl border border-gray-200 bg-gray-50
                         text-sm focus:outline-none focus:ring-2 focus:ring-orange-400
                         focus:border-transparent transition-all placeholder:text-gray-300"
            />
            <button
              type="button"
              onClick={() => setShowPass((v) => !v)}
              className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
            >
              {showPass ? <EyeOff size={17} /> : <Eye size={17} />}
            </button>
          </div>
        </div>

        {/* Forgot password */}
        <div className="text-right">
          <span className="text-xs text-orange-500 hover:underline cursor-pointer">
            Quên mật khẩu?
          </span>
        </div>

        {/* Submit */}
        <motion.button
          type="submit"
          disabled={isLoading}
          whileTap={{ scale: 0.97 }}
          className="w-full flex items-center justify-center gap-2
                     bg-orange-500 hover:bg-orange-600 active:bg-orange-700
                     text-white font-bold py-4 rounded-2xl text-sm
                     transition-colors shadow-lg shadow-orange-200
                     disabled:opacity-60 disabled:cursor-not-allowed"
        >
          {isLoading ? (
            <>
              <span className="w-4 h-4 border-2 border-white/40 border-t-white rounded-full animate-spin" />
              Đang đăng nhập...
            </>
          ) : (
            <>
              <LogIn size={17} />
              Đăng nhập
            </>
          )}
        </motion.button>

        {/* Divider */}
        <div className="flex items-center gap-3 my-2">
          <div className="flex-1 h-px bg-gray-100" />
          <span className="text-xs text-gray-300">hoặc</span>
          <div className="flex-1 h-px bg-gray-100" />
        </div>

        {/* Register link */}
        <p className="text-center text-sm text-gray-500">
          Chưa có tài khoản?{' '}
          <Link to="/register" className="text-orange-500 font-semibold hover:underline">
            Đăng ký ngay
          </Link>
        </p>
      </form>
    </div>
  )
}
