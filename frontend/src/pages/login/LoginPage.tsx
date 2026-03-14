import { Navigate, useParams, Link } from 'react-router-dom'
import { CalendarDays, AlertCircle } from 'lucide-react'
import { useAuthState } from '../../hooks/useAuth'
import { useTenantResolver } from '../../hooks/useTenant'
import { LoginForm } from './LoginForm'

export function LoginPage() {
  const { isAuthenticated } = useAuthState()
  const { slug } = useParams<{ slug?: string }>()
  const { data: tenant, isLoading, isError } = useTenantResolver(slug)

  if (isAuthenticated) return <Navigate to="/" replace />

  if (slug && isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-brand-950 via-brand-900 to-brand-800">
        <div className="w-8 h-8 border-3 border-white/30 border-t-white rounded-full animate-spin" />
      </div>
    )
  }

  if (slug && isError) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-brand-950 via-brand-900 to-brand-800 px-4">
        <div className="bg-white rounded-2xl shadow-modal p-6 max-w-sm w-full text-center">
          <AlertCircle className="w-10 h-10 text-red-500 mx-auto mb-3" />
          <h2 className="text-lg font-semibold text-gray-900 mb-1">Clinica nao encontrada</h2>
          <p className="text-sm text-gray-500">O link que voce acessou nao corresponde a nenhuma clinica cadastrada.</p>
        </div>
      </div>
    )
  }

  const registerPath = slug ? `/${slug}/register` : '/register'

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-brand-950 via-brand-900 to-brand-800 px-4">
      <div className="w-full max-w-sm animate-scale-in">
        <div className="flex items-center justify-center gap-2.5 mb-8">
          <div className="w-10 h-10 rounded-xl bg-brand-500 flex items-center justify-center shadow-lg">
            <CalendarDays className="w-5.5 h-5.5 text-white" />
          </div>
          <h1 className="text-2xl font-bold text-white tracking-tight">AgendeAqui</h1>
        </div>
        {tenant && (
          <p className="text-center text-brand-200 text-sm mb-4 font-medium">{tenant.name}</p>
        )}
        <div className="bg-white rounded-2xl shadow-modal p-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-1">Bem-vindo de volta</h2>
          <p className="text-sm text-gray-500 mb-5">Entre com suas credenciais para continuar</p>
          <LoginForm />
          {slug && (
            <p className="text-center text-sm text-gray-500 mt-4">
              Nao tem conta?{' '}
              <Link to={registerPath} className="text-brand-600 hover:text-brand-700 font-medium">Criar conta</Link>
            </p>
          )}
        </div>
        <p className="text-center text-xs text-brand-300/60 mt-6">
          &copy; 2026 AgendeAqui. Todos os direitos reservados.
        </p>
      </div>
    </div>
  )
}
