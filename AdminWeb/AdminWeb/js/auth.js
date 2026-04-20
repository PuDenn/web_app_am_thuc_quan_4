const API = 'http://192.168.1.5:5000';

export function getToken() { return localStorage.getItem('adminToken'); }
export function getUser()  { return localStorage.getItem('adminUser'); }

export function requireAuth() {
  if (!getToken()) { location.href = 'index.html'; return false; }
  return true;
}

export function logout() {
  localStorage.removeItem('adminToken');
  localStorage.removeItem('adminUser');
  location.href = 'index.html';
}

export async function login(username, password) {
  const res = await fetch(`${API}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, password }),
  });
  if (!res.ok) throw new Error('Sai tài khoản hoặc mật khẩu');
  const data = await res.json();
  if (data.role !== 'Admin') throw new Error('Tài khoản không có quyền Admin');
  localStorage.setItem('adminToken', data.token);
  localStorage.setItem('adminUser', data.username);
  return data;
}

export function authHeader() {
  return { 'Authorization': `Bearer ${getToken()}`, 'Content-Type': 'application/json' };
}
