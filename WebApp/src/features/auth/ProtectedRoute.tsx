import { Navigate, useLocation } from 'react-router-dom'
import { useAuthStore } from '../../stores/useAuthStore'
import type { ReactNode } from 'react'

interface Props {
  children: ReactNode
}

/**
 * ProtectedRoute
 * ─────────────────────────────────────────────────
 * Bảo vệ các trang yêu cầu đăng nhập.
 * Nếu chưa auth → redirect về /login, lưu lại URL để
 * sau login xong có thể navigate về đúng trang.
 */
export default function ProtectedRoute({ children }: Props) {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  const location = useLocation()

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />
  }

  return <>{children}</>
}
