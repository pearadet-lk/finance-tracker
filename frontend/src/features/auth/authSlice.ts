import { createSlice, createAsyncThunk } from '@reduxjs/toolkit'
import api from '../../services/api'

interface User { id: string; email: string; firstName: string; lastName: string }
interface AuthState { user: User | null; token: string | null; loading: boolean; error: string | null }

const initialState: AuthState = {
  user: JSON.parse(localStorage.getItem('user') || 'null'),
  token: localStorage.getItem('token'),
  loading: false,
  error: null,
}

export const login = createAsyncThunk('auth/login', async (creds: { email: string; password: string }) => {
  const res = await api.post('/auth/login', creds)
  return res.data
})

export const register = createAsyncThunk('auth/register', async (data: { email: string; password: string; firstName: string; lastName: string }) => {
  const res = await api.post('/auth/register', data)
  return res.data
})

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    logout(state) {
      state.user = null
      state.token = null
      localStorage.removeItem('token')
      localStorage.removeItem('user')
    },
  },
  extraReducers: (builder) => {
    const handleAuth = (state: AuthState, action: any) => {
      state.loading = false
      state.token = action.payload.token
      state.user = action.payload.user
      localStorage.setItem('token', action.payload.token)
      localStorage.setItem('user', JSON.stringify(action.payload.user))
    }
    builder
      .addCase(login.pending, (s) => { s.loading = true; s.error = null })
      .addCase(login.fulfilled, handleAuth)
      .addCase(login.rejected, (s, a) => { s.loading = false; s.error = a.error.message || 'Login failed' })
      .addCase(register.pending, (s) => { s.loading = true; s.error = null })
      .addCase(register.fulfilled, handleAuth)
      .addCase(register.rejected, (s, a) => { s.loading = false; s.error = a.error.message || 'Register failed' })
  },
})

export const { logout } = authSlice.actions
export default authSlice.reducer
