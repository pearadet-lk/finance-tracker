import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useAppDispatch, useAppSelector } from '../app/hooks'
import { register } from '../features/auth/authSlice'

export default function Register() {
  const dispatch = useAppDispatch()
  const { loading, error } = useAppSelector((s) => s.auth)
  const [form, setForm] = useState({ email: '', password: '', firstName: '', lastName: '' })

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    dispatch(register(form))
  }

  const set = (key: string) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm(f => ({ ...f, [key]: e.target.value }))

  return (
    <div className="min-h-screen bg-slate-950 flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        <div className="text-center mb-10">
          <span className="text-5xl">💰</span>
          <h1 className="text-3xl font-bold text-white mt-4">Finance</h1>
          <p className="text-slate-400 mt-2">Create your account</p>
        </div>

        <div className="bg-slate-900 border border-slate-800 rounded-2xl p-8">
          {error && (
            <div className="mb-4 p-3 rounded-xl bg-red-500/10 border border-red-500/30 text-red-400 text-sm">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-sm text-slate-400 mb-1.5">First Name</label>
                <input required type="text" value={form.firstName} onChange={set('firstName')}
                  className="w-full bg-slate-800 border border-slate-700 rounded-xl px-4 py-3 text-sm text-white focus:outline-none focus:border-indigo-500 transition"
                  placeholder="John" />
              </div>
              <div>
                <label className="block text-sm text-slate-400 mb-1.5">Last Name</label>
                <input required type="text" value={form.lastName} onChange={set('lastName')}
                  className="w-full bg-slate-800 border border-slate-700 rounded-xl px-4 py-3 text-sm text-white focus:outline-none focus:border-indigo-500 transition"
                  placeholder="Doe" />
              </div>
            </div>
            <div>
              <label className="block text-sm text-slate-400 mb-1.5">Email</label>
              <input required type="email" value={form.email} onChange={set('email')}
                className="w-full bg-slate-800 border border-slate-700 rounded-xl px-4 py-3 text-sm text-white focus:outline-none focus:border-indigo-500 transition"
                placeholder="you@example.com" />
            </div>
            <div>
              <label className="block text-sm text-slate-400 mb-1.5">Password</label>
              <input required type="password" value={form.password} onChange={set('password')}
                className="w-full bg-slate-800 border border-slate-700 rounded-xl px-4 py-3 text-sm text-white focus:outline-none focus:border-indigo-500 transition"
                placeholder="Min. 8 characters" minLength={8} />
            </div>
            <button type="submit" disabled={loading}
              className="w-full py-3 mt-2 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white font-medium text-sm transition-all shadow-lg shadow-indigo-500/20 disabled:opacity-60">
              {loading ? 'Creating account...' : 'Create Account'}
            </button>
          </form>

          <p className="text-center text-sm text-slate-500 mt-6">
            Already have an account?{' '}
            <Link to="/login" className="text-indigo-400 hover:text-indigo-300 font-medium">Sign in</Link>
          </p>
        </div>
      </div>
    </div>
  )
}
