import { useEffect, useState } from 'react'
import { useAppDispatch, useAppSelector } from '../app/hooks'
import { fetchTransactions, deleteTransaction, createTransaction } from '../features/transactions/transactionSlice'
import { fetchCategories } from '../features/categories/categorySlice'

const fmt = (n: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(n)

export default function Transactions() {
  const dispatch = useAppDispatch()
  const { pagedData, loading, filters, error } = useAppSelector((s) => s.transactions)
  const categories = useAppSelector((s) => s.categories.items)

  const [showModal, setShowModal] = useState(false)
  const [typeFilter, setTypeFilter] = useState('')
  const [form, setForm] = useState({ amount: '', type: 'Expense', categoryId: '', description: '', date: new Date().toISOString().split('T')[0] })

  useEffect(() => {
    dispatch(fetchTransactions({ ...filters, type: typeFilter || undefined }))
  }, [dispatch, filters, typeFilter])

  useEffect(() => { dispatch(fetchCategories()) }, [dispatch])

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await dispatch(createTransaction({ ...form, amount: parseFloat(form.amount) })).unwrap()
      setShowModal(false)
      setForm({ amount: '', type: 'Expense', categoryId: '', description: '', date: new Date().toISOString().split('T')[0] })
      dispatch(fetchTransactions(filters))
    } catch {
      // Error state is set in Redux and shown in UI.
    }
  }

  const filteredCategories = categories.filter(c => !form.type || c.type === form.type)

  return (
    <div className="p-8 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold">Transactions</h2>
          <p className="text-slate-400 text-sm mt-1">{pagedData?.totalCount ?? 0} total records</p>
        </div>
        <button
          onClick={() => setShowModal(true)}
          className="bg-indigo-600 hover:bg-indigo-500 text-white text-sm font-medium px-5 py-2.5 rounded-xl transition-all shadow-lg shadow-indigo-500/20"
        >
          + Add Transaction
        </button>
      </div>

      {/* Filters */}
      <div className="flex gap-3">
        {['', 'Income', 'Expense'].map((t) => (
          <button
            key={t}
            onClick={() => setTypeFilter(t)}
            className={`px-4 py-2 rounded-lg text-sm font-medium transition-all ${
              typeFilter === t
                ? 'bg-indigo-600 text-white'
                : 'bg-slate-800 text-slate-400 hover:bg-slate-700'
            }`}
          >
            {t || 'All'}
          </button>
        ))}
      </div>

      {/* Table */}
      <div className="bg-slate-900 border border-slate-800 rounded-2xl overflow-hidden">
        {error && (
          <div className="mx-6 mt-6 p-3 rounded-xl bg-red-500/10 border border-red-500/30 text-red-400 text-sm">
            {error}
          </div>
        )}
        {loading ? (
          <div className="flex justify-center py-16">
            <div className="animate-spin w-8 h-8 border-4 border-indigo-500 border-t-transparent rounded-full" />
          </div>
        ) : (
          <table className="w-full">
            <thead>
              <tr className="border-b border-slate-800 text-left">
                <th className="px-6 py-4 text-xs font-semibold text-slate-500 uppercase tracking-wider">Category</th>
                <th className="px-6 py-4 text-xs font-semibold text-slate-500 uppercase tracking-wider">Description</th>
                <th className="px-6 py-4 text-xs font-semibold text-slate-500 uppercase tracking-wider">Date</th>
                <th className="px-6 py-4 text-xs font-semibold text-slate-500 uppercase tracking-wider text-right">Amount</th>
                <th className="px-6 py-4 text-xs font-semibold text-slate-500 uppercase tracking-wider"></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-800">
              {pagedData?.items.map((t) => (
                <tr key={t.id} className="hover:bg-slate-800/50 transition-colors">
                  <td className="px-6 py-4">
                    <div className="flex items-center gap-3">
                      <span
                        className="w-8 h-8 rounded-full flex items-center justify-center text-sm"
                        style={{ background: `${t.categoryColor}20`, border: `1px solid ${t.categoryColor}40` }}
                      >
                        {t.categoryIcon}
                      </span>
                      <span className="text-sm font-medium text-slate-200">{t.categoryName}</span>
                    </div>
                  </td>
                  <td className="px-6 py-4 text-sm text-slate-400">{t.description || '—'}</td>
                  <td className="px-6 py-4 text-sm text-slate-400">
                    {new Date(t.date).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })}
                  </td>
                  <td className="px-6 py-4 text-right">
                    <span className={`text-sm font-semibold ${t.type === 'Income' ? 'text-emerald-400' : 'text-red-400'}`}>
                      {t.type === 'Income' ? '+' : '-'}{fmt(t.amount)}
                    </span>
                  </td>
                  <td className="px-6 py-4 text-right">
                    <button
                      onClick={() => dispatch(deleteTransaction(t.id))}
                      className="text-slate-600 hover:text-red-400 transition-colors text-lg"
                    >
                      ✕
                    </button>
                  </td>
                </tr>
              ))}
              {!pagedData?.items.length && (
                <tr>
                  <td colSpan={5} className="px-6 py-16 text-center text-slate-500">
                    No transactions found. Add your first one!
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        )}

        {/* Pagination */}
        {pagedData && pagedData.totalPages > 1 && (
          <div className="flex items-center justify-between px-6 py-4 border-t border-slate-800">
            <p className="text-sm text-slate-500">
              Page {pagedData.page} of {pagedData.totalPages}
            </p>
            <div className="flex gap-2">
              <button
                disabled={!pagedData.hasPreviousPage}
                className="px-3 py-1.5 text-sm bg-slate-800 text-slate-300 rounded-lg disabled:opacity-40 hover:bg-slate-700 transition"
              >
                ← Prev
              </button>
              <button
                disabled={!pagedData.hasNextPage}
                className="px-3 py-1.5 text-sm bg-slate-800 text-slate-300 rounded-lg disabled:opacity-40 hover:bg-slate-700 transition"
              >
                Next →
              </button>
            </div>
          </div>
        )}
      </div>

      {/* Add Modal */}
      {showModal && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center z-50">
          <div className="bg-slate-900 border border-slate-700 rounded-2xl p-8 w-full max-w-md shadow-2xl">
            <h3 className="text-lg font-bold mb-6">New Transaction</h3>
            <form onSubmit={handleCreate} className="space-y-4">
              <div className="grid grid-cols-2 gap-3">
                {['Income', 'Expense'].map((t) => (
                  <button
                    key={t} type="button"
                    onClick={() => setForm(f => ({ ...f, type: t, categoryId: '' }))}
                    className={`py-2.5 rounded-xl text-sm font-medium transition-all ${
                      form.type === t
                        ? t === 'Income' ? 'bg-emerald-500/20 text-emerald-400 border border-emerald-500/40' : 'bg-red-500/20 text-red-400 border border-red-500/40'
                        : 'bg-slate-800 text-slate-400 border border-transparent'
                    }`}
                  >
                    {t === 'Income' ? '📈' : '📉'} {t}
                  </button>
                ))}
              </div>

              <input
                required type="number" step="0.01" placeholder="Amount"
                value={form.amount} onChange={e => setForm(f => ({ ...f, amount: e.target.value }))}
                className="w-full bg-slate-800 border border-slate-700 rounded-xl px-4 py-3 text-sm focus:outline-none focus:border-indigo-500 transition"
              />

              <select
                required value={form.categoryId} onChange={e => setForm(f => ({ ...f, categoryId: e.target.value }))}
                className="w-full bg-slate-800 border border-slate-700 rounded-xl px-4 py-3 text-sm focus:outline-none focus:border-indigo-500 transition"
              >
                <option value="">Select category</option>
                {filteredCategories.map(c => (
                  <option key={c.id} value={c.id}>{c.icon} {c.name}</option>
                ))}
              </select>

              <input
                type="text" placeholder="Description (optional)"
                value={form.description} onChange={e => setForm(f => ({ ...f, description: e.target.value }))}
                className="w-full bg-slate-800 border border-slate-700 rounded-xl px-4 py-3 text-sm focus:outline-none focus:border-indigo-500 transition"
              />

              <input
                required type="date" value={form.date} onChange={e => setForm(f => ({ ...f, date: e.target.value }))}
                className="w-full bg-slate-800 border border-slate-700 rounded-xl px-4 py-3 text-sm focus:outline-none focus:border-indigo-500 transition"
              />

              <div className="flex gap-3 pt-2">
                <button type="button" onClick={() => setShowModal(false)}
                  className="flex-1 py-3 rounded-xl text-sm bg-slate-800 text-slate-400 hover:bg-slate-700 transition">
                  Cancel
                </button>
                <button type="submit"
                  className="flex-1 py-3 rounded-xl text-sm bg-indigo-600 hover:bg-indigo-500 text-white font-medium transition shadow-lg shadow-indigo-500/20">
                  Save
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}
