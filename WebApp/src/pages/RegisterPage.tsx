import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import { User, Phone, Mail, Lock, Eye, EyeOff, UserPlus, AlertCircle } from 'lucide-react'
import { useAuthStore } from '../stores/useAuthStore'
import { mockRegister } from '../lib/mockApi'

export default function RegisterPage() {
  const navigate = useNavigate()
  const { loginSuccess, setLoading, setError, isLoading, error } = useAuthStore()

  const [form, setForm] = useState({
    name: '', phone: '', email: '', password: '',
  })
  const [showPass, setShowPass] = useState(false)

  const set = (key: keyof typeof form) =>
    (e: React.ChangeEvent<HTMLInputElement>) =>
      setForm((f) => ({ ...f, [key]: e.target.value }))

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    const { name, phone, email, password } = form
    if (!name || !phone || !email || !password) {
      setError('Vui lòng điền đầy đủ thông tin.')
      return
    }

    setLoading(true)
    setError(null)
    try {
      const { user, token } = await mockRegister(form)
      loginSuccess(user, token)
      navigate('/map', { replace: true })
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Đăng ký thất bại.')
    } finally {
      setLoading(false)
    }
  }

  // Shared input class
  const inputCls = `w-full pl-11 pr-4 py-3.5 rounded-xl border border-gray-200 bg-gray-50
    text-sm focus:outline-none focus:ring-2 focus:ring-orange-400
    focus:border-transparent transition-all placeholder:text-gray-300`

  const fields: {
    key: keyof typeof form
    label: string
    placeholder: string
    type: string
    icon: React.ReactNode
  }[] = [
    { key: 'name',  label: 'Họ và tên',       placeholder: 'Nguyễn Văn A',     type: 'text',     icon: <User  size={17} /> },
    { key: 'phone', label: 'Số điện thoại',    placeholder: '0901 234 567',     type: 'tel',      icon: <Phone size={17} /> },
    { key: 'email', label: 'Email',            placeholder: 'you@example.com',  type: 'email',    icon: <Mail  size={17} /> },
  ]

  return (
    <div>
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

        {/* Dynamic fields */}
        {fields.map(({ key, label, placeholder, type, icon }) => (
          <div key={key}>
            <label className="block text-xs font-semibold text-gray-500 mb-1.5 ml-1">
              {label}
            </label>
            <div className="relative">
              <span className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400">
                {icon}
              </span>
              <input
                type={type}
                value={form[key]}
                onChange={set(key)}
                placeholder={placeholder}
                className={inputCls}
              />
            </div>
          </div>
        ))}

        {/* Password */}
        <div>
          <label className="block text-xs font-semibold text-gray-500 mb-1.5 ml-1">
            Mật khẩu
          </label>
          <div className="relative">
            <Lock size={17} className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400" />
            <input
              type={showPass ? 'text' : 'password'}
              value={form.password}
              onChange={set('password')}
              placeholder="Ít nhất 6 ký tự"
              autoComplete="new-password"
              className={`${inputCls} pr-12`}
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

        {/* Submit */}
        <motion.button
          type="submit"
          disabled={isLoading}
          whileTap={{ scale: 0.97 }}
          className="w-full flex items-center justify-center gap-2
                     bg-orange-500 hover:bg-orange-600 active:bg-orange-700
                     text-white font-bold py-4 rounded-2xl text-sm
                     transition-colors shadow-lg shadow-orange-200
                     disabled:opacity-60 disabled:cursor-not-allowed mt-2"
        >
          {isLoading ? (
            <>
              <span className="w-4 h-4 border-2 border-white/40 border-t-white rounded-full animate-spin" />
              Đang tạo tài khoản...
            </>
          ) : (
            <>
              <UserPlus size={17} />
              Tạo tài khoản
            </>
          )}
        </motion.button>

        {/* Login link */}
        <p className="text-center text-sm text-gray-500 pt-1">
          Đã có tài khoản?{' '}
          <Link to="/login" className="text-orange-500 font-semibold hover:underline">
            Đăng nhập
          </Link>
        </p>
      </form>
    </div>
  )
}
