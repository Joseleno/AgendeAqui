import { Navigate } from 'react-router-dom'
import { CalendarDays } from 'lucide-react'
import { useAuthState } from '../../hooks/useAuth'
import { LoginForm } from './LoginForm'

export function LoginPage() {
  const { isAuthenticated } = useAuthState()

  if (isAuthenticated) return <Navigate to="/" replace />

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-brand-950 via-brand-900 to-brand-800 px-4">
      <div className="w-full max-w-sm animate-scale-in">
        <div className="flex items-center justify-center gap-2.5 mb-8">
          <div className="w-10 h-10 rounded-xl bg-brand-500 flex items-center justify-center shadow-lg">
            <CalendarDays className="w-5.5 h-5.5 text-white" />
          </div>
          <h1 className="text-2xl font-bold text-white tracking-tight">AgendeAqui</h1>
        </div>
        <div className="bg-white rounded-2xl shadow-modal p-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-1">Bem-vindo de volta</h2>
          <p className="text-sm text-gray-500 mb-5">Entre com suas credenciais para continuar</p>
          <LoginForm />
        </div>
        <p className="text-center text-xs text-brand-300/60 mt-6">
          &copy; 2026 AgendeAqui. Todos os direitos reservados.
        </p>
      </div>
    </div>
  )
}
