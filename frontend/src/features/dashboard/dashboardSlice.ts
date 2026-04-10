import { createSlice, createAsyncThunk } from '@reduxjs/toolkit'
import api from '../../services/api'

export interface DashboardData {
  totalIncome: number; totalExpense: number; balance: number
  monthlyTrend: { month: string; income: number; expense: number }[]
  categoryBreakdown: { category: string; color: string; amount: number; count: number }[]
}

interface DashboardState { data: DashboardData | null; loading: boolean; error: string | null }

export const fetchDashboard = createAsyncThunk(
  'dashboard/fetch',
  async (year?: number) => {
    const res = await api.get(`/dashboard${year ? `?year=${year}` : ''}`)
    return res.data
  }
)

const dashboardSlice = createSlice({
  name: 'dashboard',
  initialState: { data: null, loading: false, error: null } as DashboardState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchDashboard.pending, (s) => { s.loading = true; s.error = null })
      .addCase(fetchDashboard.fulfilled, (s, a) => { s.loading = false; s.data = a.payload })
      .addCase(fetchDashboard.rejected, (s, a) => { s.loading = false; s.error = a.error.message || 'Failed' })
  },
})

export default dashboardSlice.reducer
