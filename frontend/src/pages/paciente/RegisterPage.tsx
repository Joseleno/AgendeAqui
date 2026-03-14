import { useState } from 'react'
import { Navigate, Link } from 'react-router-dom'
import { CalendarDays, User, Mail, Phone, Lock } from 'lucide-react'
import { useAuthState } from '../../hooks/useAuth'
import { useRegisterPatient } from '../../hooks/usePatientPortal'
import { toast } from '../../components/ui/Toast'

export function RegisterPage() {
  const { isAuthenticated } = useAuthState()
  const register = useRegisterPatient()

  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [phone, setPhone] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')

  if (isAuthenticated) return <Navigate to="/meus-agendamentos" replace />

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (password.length < 6) {
      toast('error', 'A senha deve ter no mínimo 6 caracteres')
      return
    }
    if (password !== confirmPassword) {
      toast('error', 'As senhas não coincidem')
      return
    }
    register.mutate({ name: name.trim(), email: email.trim(), phone: phone.trim(), password })
  }

  const inputClass = "w-full rounded-xl border border-gray-200 bg-gray-50/50 pl-9 pr-3 py-2.5 text-sm focus:bg-white focus:border-brand-500 focus:ring-1 focus:ring-brand-500 outline-none transition-all duration-200"

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
          <h2 className="text-lg font-semibold text-gray-900 mb-1">Criar conta</h2>
          <p className="text-sm text-gray-500 mb-5">Cadastre-se para agendar suas consultas</p>
          <form onSubmit={handleSubmit} className="space-y-3.5">
            <div className="relative">
              <User className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
              <input required value={name} onChange={(e) => setName(e.target.value)} className={inputClass} placeholder="Nome completo" />
            </div>
            <div className="relative">
              <Mail className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
              <input type="email" required value={email} onChange={(e) => setEmail(e.target.value)} className={inputClass} placeholder="seu@email.com" />
            </div>
            <div className="relative">
              <Phone className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
              <input required value={phone} onChange={(e) => setPhone(e.target.value)} className={inputClass} placeholder="(11) 99999-9999" />
            </div>
            <div className="relative">
              <Lock className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
              <input type="password" required minLength={6} value={password} onChange={(e) => setPassword(e.target.value)} className={inputClass} placeholder="Senha (min. 6 caracteres)" />
            </div>
            <div className="relative">
              <Lock className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
              <input type="password" required minLength={6} value={confirmPassword} onChange={(e) => setConfirmPassword(e.target.value)} className={inputClass} placeholder="Confirmar senha" />
            </div>
            <button
              type="submit"
              disabled={register.isPending}
              className="w-full rounded-xl bg-brand-600 px-4 py-2.5 text-sm font-semibold text-white hover:bg-brand-700 disabled:opacity-50 transition-all duration-200 shadow-sm hover:shadow-md active:scale-[0.98]"
            >
              {register.isPending ? 'Cadastrando...' : 'Criar conta'}
            </button>
          </form>
          <p className="text-center text-sm text-gray-500 mt-4">
            Já tem conta?{' '}
            <Link to="/login" className="text-brand-600 hover:text-brand-700 font-medium">Entrar</Link>
          </p>
        </div>
        <p className="text-center text-xs text-brand-300/60 mt-6">
          &copy; 2026 AgendeAqui. Todos os direitos reservados.
        </p>
      </div>
    </div>
  )
}
