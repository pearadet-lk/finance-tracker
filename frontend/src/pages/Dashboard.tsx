import { useEffect, useMemo, useState } from 'react'
import { useAppDispatch, useAppSelector } from '../app/hooks'
import { fetchDashboard } from '../features/dashboard/dashboardSlice'
import {
  BarChart, Bar, PieChart, Pie, Cell,
  XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend
} from 'recharts'

const fmt = (n: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(n)

function StatCard({ label, value, icon, color, sub }: { label: string; value: string; icon: string; color: string; sub?: string }) {
  return (
    <div className="bg-slate-900 border border-slate-800 rounded-2xl p-6">
      <div className="flex items-center justify-between mb-4">
        <span className="text-slate-400 text-sm font-medium">{label}</span>
        <span className="text-2xl">{icon}</span>
      </div>
      <p className={`text-2xl font-bold ${color}`}>{value}</p>
      {sub && <p className="text-slate-500 text-xs mt-1">{sub}</p>}
    </div>
  )
}

export default function Dashboard() {
  const dispatch = useAppDispatch()
  const { data, loading } = useAppSelector((s) => s.dashboard)
  const currentYear = new Date().getFullYear()
  const [selectedYear, setSelectedYear] = useState(currentYear)

  const yearOptions = useMemo(
    () => Array.from({ length: 6 }, (_, i) => currentYear - i),
    [currentYear]
  )

  useEffect(() => {
    dispatch(fetchDashboard(selectedYear))
  }, [dispatch, selectedYear])

  if (loading || !data) {
    return (
      <div className="flex items-center justify-center h-full">
        <div className="animate-spin w-8 h-8 border-4 border-indigo-500 border-t-transparent rounded-full" />
      </div>
    )
  }

  return (
    <div className="p-8 space-y-8">
      <div>
        <div className="flex items-center justify-between gap-3">
          <div>
            <h2 className="text-2xl font-bold">Dashboard</h2>
            <p className="text-slate-400 text-sm mt-1">Your financial overview for {selectedYear}</p>
          </div>
          <select
            value={selectedYear}
            onChange={(e) => setSelectedYear(Number(e.target.value))}
            className="bg-slate-900 border border-slate-700 rounded-xl px-3 py-2 text-sm text-slate-200 focus:outline-none focus:border-indigo-500 transition"
          >
            {yearOptions.map((year) => (
              <option key={year} value={year}>{year}</option>
            ))}
          </select>
        </div>
      </div>

      {/* KPI Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <StatCard label="Total Income" value={fmt(data.totalIncome)} icon="📈" color="text-emerald-400" />
        <StatCard label="Total Expenses" value={fmt(data.totalExpense)} icon="📉" color="text-red-400" />
        <StatCard
          label="Balance"
          value={fmt(data.balance)}
          icon="💰"
          color={data.balance >= 0 ? 'text-indigo-400' : 'text-orange-400'}
          sub={data.balance >= 0 ? 'You are on track 🎉' : 'Over budget ⚠️'}
        />
      </div>

      {/* Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Monthly Trend */}
        <div className="bg-slate-900 border border-slate-800 rounded-2xl p-6">
          <h3 className="text-sm font-semibold text-slate-300 mb-4">Monthly Income vs Expenses</h3>
          <ResponsiveContainer width="100%" height={240}>
            <BarChart data={data.monthlyTrend} barGap={4}>
              <CartesianGrid strokeDasharray="3 3" stroke="#1e293b" />
              <XAxis dataKey="month" tick={{ fill: '#64748b', fontSize: 12 }} />
              <YAxis tick={{ fill: '#64748b', fontSize: 12 }} tickFormatter={(v) => `$${v / 1000}k`} />
              <Tooltip
                contentStyle={{ background: '#0f172a', border: '1px solid #1e293b', borderRadius: 8 }}
                labelStyle={{ color: '#e2e8f0' }}
                formatter={(v: any) => fmt(v)}
              />
              <Legend />
              <Bar dataKey="income" name="Income" fill="#10b981" radius={[4, 4, 0, 0]} />
              <Bar dataKey="expense" name="Expense" fill="#ef4444" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>

        {/* Category Breakdown */}
        <div className="bg-slate-900 border border-slate-800 rounded-2xl p-6">
          <h3 className="text-sm font-semibold text-slate-300 mb-4">Spending by Category</h3>
          <div className="flex items-center gap-6">
            <ResponsiveContainer width="50%" height={200}>
              <PieChart>
                <Pie data={data.categoryBreakdown} dataKey="amount" nameKey="category" cx="50%" cy="50%" outerRadius={80} strokeWidth={0}>
                  {data.categoryBreakdown.map((entry, i) => (
                    <Cell key={i} fill={entry.color} />
                  ))}
                </Pie>
                <Tooltip formatter={(v: any) => fmt(v)} contentStyle={{ background: '#0f172a', border: '1px solid #1e293b', borderRadius: 8 }} />
              </PieChart>
            </ResponsiveContainer>
            <ul className="flex-1 space-y-2">
              {data.categoryBreakdown.map((c, i) => (
                <li key={i} className="flex items-center justify-between text-sm">
                  <span className="flex items-center gap-2">
                    <span className="w-2.5 h-2.5 rounded-full" style={{ background: c.color }} />
                    <span className="text-slate-300">{c.category}</span>
                  </span>
                  <span className="text-slate-400">{fmt(c.amount)}</span>
                </li>
              ))}
            </ul>
          </div>
        </div>
      </div>
    </div>
  )
}
