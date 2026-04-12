/**
 * Mock API — giả lập backend Auth
 * Trong production: thay bằng fetch() thật
 */

export interface AuthUser {
  id: string
  name: string
  phone: string
  email: string
  avatar?: string
}

export interface LoginPayload {
  credential: string  // email hoặc số điện thoại
  password: string
}

export interface RegisterPayload {
  name: string
  phone: string
  email: string
  password: string
}

export interface AuthResponse {
  user: AuthUser
  token: string
}

// ── Mock accounts ─────────────────────────────────
const MOCK_ACCOUNTS = [
  {
    credential: ['user@quan4.vn', '0901234567'],
    password: '123456',
    user: {
      id: 'u-001',
      name: 'Nguyễn Văn A',
      phone: '0901234567',
      email: 'user@quan4.vn',
    },
    token: 'mock-jwt-token-amthucquan4-u001',
  },
]

/** Giả lập độ trễ mạng */
const delay = (ms: number) => new Promise((r) => setTimeout(r, ms))

export async function mockLogin(payload: LoginPayload): Promise<AuthResponse> {
  await delay(900)

  const account = MOCK_ACCOUNTS.find(
    (a) =>
      a.credential.includes(payload.credential.toLowerCase().trim()) &&
      a.password === payload.password,
  )

  if (!account) {
    throw new Error('Sai tài khoản hoặc mật khẩu. Vui lòng thử lại.')
  }

  return { user: account.user, token: account.token }
}

export async function mockRegister(payload: RegisterPayload): Promise<AuthResponse> {
  await delay(1200)

  // Validate đơn giản
  if (payload.password.length < 6) {
    throw new Error('Mật khẩu phải có ít nhất 6 ký tự.')
  }
  if (!payload.email.includes('@')) {
    throw new Error('Email không hợp lệ.')
  }

  // Mock: tạo user mới
  const newUser: AuthUser = {
    id: `u-${Date.now()}`,
    name: payload.name,
    phone: payload.phone,
    email: payload.email,
  }

  return {
    user: newUser,
    token: `mock-jwt-token-${newUser.id}`,
  }
}
