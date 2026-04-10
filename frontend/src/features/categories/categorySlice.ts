import { createSlice, createAsyncThunk } from '@reduxjs/toolkit'
import api from '../../services/api'

export interface Category { id: string; name: string; type: string; icon: string; color: string }

export const fetchCategories = createAsyncThunk(
  'categories/fetchAll',
  async (type?: string) => {
    const res = await api.get(`/categories${type ? `?type=${type}` : ''}`)
    return res.data
  }
)

const categorySlice = createSlice({
  name: 'categories',
  initialState: { items: [] as Category[], loading: false },
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchCategories.pending, (s) => { s.loading = true })
      .addCase(fetchCategories.fulfilled, (s, a) => { s.loading = false; s.items = a.payload })
  },
})

export default categorySlice.reducer
