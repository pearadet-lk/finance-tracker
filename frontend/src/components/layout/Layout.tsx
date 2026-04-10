import { Outlet, NavLink, useNavigate } from 'react-router-dom'
import { useAppDispatch, useAppSelector } from '../../app/hooks'
import { logout } from '../../features/auth/authSlice'

export default function Layout() {
  const dispatch = useAppDispatch()
  const navigate = useNavigate()
  const user = useAppSelector((s) => s.auth.user)

  const handleLogout = () => {
    dispatch(logout())
    navigate('/login')
  }

  const navClass = ({ isActive }: { isActive: boolean }) =>
    `flex items-center gap-3 px-4 py-3 rounded-xl text-sm font-medium transition-all ${
      isActive
        ? 'bg-indigo-600 text-white shadow-lg shadow-indigo-500/30'
        : 'text-slate-400 hover:bg-slate-800 hover:text-white'
    }`

  return (
    <div className="flex h-screen bg-slate-950 text-white">
      {/* Sidebar */}
      <aside className="w-64 flex flex-col bg-slate-900 border-r border-slate-800 p-6">
        <div className="mb-10">
          <h1 className="text-xl font-bold text-white flex items-center gap-2">
            <span className="text-2xl">💰</span> Finance
          </h1>
          <p className="text-xs text-slate-500 mt-1">Personal Finance Tracker</p>
        </div>

        <nav className="flex flex-col gap-1 flex-1">
          <NavLink to="/" end className={navClass}>
            <span>📊</span> Dashboard
          </NavLink>
          <NavLink to="/transactions" className={navClass}>
            <span>💳</span> Transactions
          </NavLink>
        </nav>

        <div className="border-t border-slate-800 pt-4">
          <div className="flex items-center gap-3 mb-3">
            <div className="w-9 h-9 rounded-full bg-indigo-600 flex items-center justify-center text-sm font-bold">
              {user?.firstName?.[0]}{user?.lastName?.[0]}
            </div>
            <div>
              <p className="text-sm font-medium">{user?.firstName} {user?.lastName}</p>
              <p className="text-xs text-slate-500">{user?.email}</p>
            </div>
          </div>
          <button
            onClick={handleLogout}
            className="w-full text-left px-4 py-2 text-sm text-slate-400 hover:text-red-400 hover:bg-slate-800 rounded-lg transition-all"
          >
            🚪 Sign out
          </button>
        </div>
      </aside>

      {/* Main content */}
      <main className="flex-1 overflow-auto">
        <Outlet />
      </main>
    </div>
  )
}
