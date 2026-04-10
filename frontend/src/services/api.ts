import axios from 'axios'
import { store } from '../app/store'
import { logout } from '../features/auth/authSlice'

const api = axios.create({
  // Default to same-origin `/api` so nginx (in k8s) can proxy to the backend.
  // This avoids relying on `localhost:5001` from inside the browser environment.
  baseURL: import.meta.env.VITE_API_URL || '/api',
  headers: { 'Content-Type': 'application/json' },
})

// Attach token automatically
api.interceptors.request.use((config) => {
  const token = store.getState().auth.token
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// Handle 401 globally
api.interceptors.response.use(
  (res) => res,
  (error) => {
    if (error.response?.status === 401) {
      store.dispatch(logout())
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)

export default api
