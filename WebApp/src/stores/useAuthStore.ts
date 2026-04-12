import { create } from 'zustand'
import type { AuthUser } from '../lib/mockApi'

const TOKEN_KEY = 'amthuc_token'
const USER_KEY  = 'amthuc_user'

interface AuthState {
  user:            AuthUser | null
  token:           string | null
  isAuthenticated: boolean
  isLoading:       boolean
  error:           string | null

  // Actions
  loginSuccess:  (user: AuthUser, token: string) => void
  logout:        () => void
  setLoading:    (v: boolean) => void
  setError:      (msg: string | null) => void
  hydrate:       () => void   // khôi phục session từ localStorage
}

export const useAuthStore = create<AuthState>((set) => ({
  user:            null,
  token:           null,
  isAuthenticated: false,
  isLoading:       false,
  error:           null,

  loginSuccess: (user, token) => {
    // Lưu vào localStorage để giữ session
    localStorage.setItem(TOKEN_KEY, token)
    localStorage.setItem(USER_KEY, JSON.stringify(user))
    set({ user, token, isAuthenticated: true, error: null })
  },

  logout: () => {
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(USER_KEY)
    set({ user: null, token: null, isAuthenticated: false, error: null })
  },

  setLoading: (v) => set({ isLoading: v }),
  setError:   (msg) => set({ error: msg }),

  /**
   * Gọi khi app khởi động để khôi phục session từ localStorage.
   * Đặt trong main.tsx hoặc App.tsx (useEffect một lần).
   */
  hydrate: () => {
    const token = localStorage.getItem(TOKEN_KEY)
    const raw   = localStorage.getItem(USER_KEY)
    if (token && raw) {
      try {
        const user = JSON.parse(raw) as AuthUser
        set({ user, token, isAuthenticated: true })
      } catch {
        localStorage.removeItem(TOKEN_KEY)
        localStorage.removeItem(USER_KEY)
      }
    }
  },
}))
