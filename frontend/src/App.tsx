import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { Provider } from 'react-redux'
import { store } from './app/store'
import Layout from './components/layout/Layout'
import Dashboard from './pages/Dashboard'
import Transactions from './pages/Transactions'
import Login from './pages/Login'
import Register from './pages/Register'
import { useAppSelector } from './app/hooks'

function PrivateRoute({ children }: { children: React.ReactNode }) {
  const token = useAppSelector((s) => s.auth.token)
  return token ? <>{children}</> : <Navigate to="/login" replace />
}

function AppRoutes() {
  const token = useAppSelector((s) => s.auth.token)
  return (
    <Routes>
      <Route path="/login" element={token ? <Navigate to="/" /> : <Login />} />
      <Route path="/register" element={token ? <Navigate to="/" /> : <Register />} />
      <Route path="/" element={<PrivateRoute><Layout /></PrivateRoute>}>
        <Route index element={<Dashboard />} />
        <Route path="transactions" element={<Transactions />} />
      </Route>
    </Routes>
  )
}

export default function App() {
  return (
    <Provider store={store}>
      <BrowserRouter>
        <AppRoutes />
      </BrowserRouter>
    </Provider>
  )
}
