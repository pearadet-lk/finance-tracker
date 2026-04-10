import { createSlice, createAsyncThunk } from '@reduxjs/toolkit'
import api from '../../services/api'

export interface Transaction {
  id: string; amount: number; type: string; categoryName: string
  categoryColor: string; categoryIcon: string; description: string
  date: string; createdAt: string
}

export interface PagedResult<T> {
  items: T[]; totalCount: number; page: number; pageSize: number
  totalPages: number; hasNextPage: boolean; hasPreviousPage: boolean
}

interface TransactionState {
  pagedData: PagedResult<Transaction> | null
  loading: boolean; error: string | null
  filters: { page: number; pageSize: number; type?: string; categoryId?: string; from?: string; to?: string }
}

const initialState: TransactionState = {
  pagedData: null, loading: false, error: null,
  filters: { page: 1, pageSize: 20 },
}

export const fetchTransactions = createAsyncThunk(
  'transactions/fetchAll',
  async (params: Record<string, any>) => {
    const query = new URLSearchParams(Object.entries(params).filter(([, v]) => v != null).map(([k, v]) => [k, String(v)])).toString()
    const res = await api.get(`/transactions?${query}`)
    return res.data
  }
)

export const createTransaction = createAsyncThunk(
  'transactions/create',
  async (data: { amount: number; type: string; categoryId: string; description: string; date: string }) => {
    const res = await api.post('/transactions', data)
    return res.data
  }
)

export const deleteTransaction = createAsyncThunk(
  'transactions/delete',
  async (id: string) => {
    await api.delete(`/transactions/${id}`)
    return id
  }
)

const transactionSlice = createSlice({
  name: 'transactions',
  initialState,
  reducers: {
    setFilters(state, action) { state.filters = { ...state.filters, ...action.payload } },
    resetFilters(state) { state.filters = { page: 1, pageSize: 20 } },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchTransactions.pending, (s) => { s.loading = true; s.error = null })
      .addCase(fetchTransactions.fulfilled, (s, a) => { s.loading = false; s.pagedData = a.payload })
      .addCase(fetchTransactions.rejected, (s, a) => { s.loading = false; s.error = a.error.message || 'Failed' })
      .addCase(createTransaction.pending, (s) => { s.loading = true; s.error = null })
      .addCase(createTransaction.fulfilled, (s) => { s.loading = false })
      .addCase(createTransaction.rejected, (s, a) => { s.loading = false; s.error = a.error.message || 'Failed to create transaction' })
      .addCase(deleteTransaction.fulfilled, (s, a) => {
        if (s.pagedData) s.pagedData.items = s.pagedData.items.filter(t => t.id !== a.payload)
      })
  },
})

export const { setFilters, resetFilters } = transactionSlice.actions
export default transactionSlice.reducer
