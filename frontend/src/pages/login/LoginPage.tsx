import { Navigate } from 'react-router-dom'
import { useAuthState } from '../../hooks/useAuth'
import { LoginForm } from './LoginForm'

export function LoginPage() {
  const { isAuthenticated } = useAuthState()

  if (isAuthenticated) return <Navigate to="/" replace />

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 px-4">
      <div className="w-full max-w-sm">
        <h1 className="text-2xl font-bold text-indigo-600 text-center mb-8">AgendeAqui</h1>
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
          <h2 className="text-lg font-semibold text-gray-800 mb-4">Entrar</h2>
          <LoginForm />
        </div>
      </div>
    </div>
  )
}
