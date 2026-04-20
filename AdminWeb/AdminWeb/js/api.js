import { authHeader } from './auth.js';
const API = 'http://192.168.1.5:5000';

async function req(path, opts = {}) {
  const res = await fetch(`${API}${path}`, {
    ...opts, headers: { ...authHeader(), ...(opts.headers || {}) }
  });
  if (!res.ok) throw new Error(await res.text());
  return res.status === 204 ? null : res.json();
}

// ── Stats ──────────────────────────────────────────
export const getOverview   = ()      => req('/api/admin/stats/overview');
export const getTopPoi     = (n=5)   => req(`/api/admin/stats/top-poi?limit=${n}`);
export const getTimeline   = (f,t)   => req(`/api/admin/stats/timeline?from=${f}&to=${t}`);
export const getPoiStats   = (id)    => req(`/api/admin/stats/poi/${id}`);

// ── POIs ───────────────────────────────────────────
export const getPois       = ()      => req('/api/pois');
export const createPoi     = (data)  => req('/api/pois', { method:'POST', body:JSON.stringify(data) });
export const updatePoi     = (id,d)  => req(`/api/pois/${id}`, { method:'PUT', body:JSON.stringify(d) });
export const deletePoi     = (id)    => req(`/api/pois/${id}`, { method:'DELETE' });

// ── Users ──────────────────────────────────────────
export const getUsers      = ()      => req('/api/users');
export const createUser    = (data)  => req('/api/users/create', { method:'POST', body:JSON.stringify(data) });
export const toggleUser    = (id)    => req(`/api/users/${id}/toggle`, { method:'PATCH' });
export const deleteUser    = (id)    => req(`/api/users/${id}`, { method:'DELETE' });
