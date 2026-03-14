import { useState } from 'react'
import { Mail, Lock } from 'lucide-react'
import { useLogin } from '../../hooks/useAuth'
import { toast } from '../../components/ui/Toast'

export function LoginForm() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const login = useLogin()

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    login.mutate(
      { email, password },
      {
        onError: () => {
          toast('error', 'Email ou senha invalidos')
        },
      },
    )
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div>
        <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-1.5">
          Email
        </label>
        <div className="relative">
          <Mail className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            id="email"
            type="email"
            required
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            className="w-full rounded-xl border border-gray-200 bg-gray-50/50 pl-9 pr-3 py-2.5 text-sm focus:bg-white focus:border-brand-500 focus:ring-1 focus:ring-brand-500 outline-none transition-all duration-200"
            placeholder="seu@email.com"
          />
        </div>
      </div>

      <div>
        <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-1.5">
          Senha
        </label>
        <div className="relative">
          <Lock className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            id="password"
            type="password"
            required
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            className="w-full rounded-xl border border-gray-200 bg-gray-50/50 pl-9 pr-3 py-2.5 text-sm focus:bg-white focus:border-brand-500 focus:ring-1 focus:ring-brand-500 outline-none transition-all duration-200"
            placeholder="********"
          />
        </div>
      </div>

      <button
        type="submit"
        disabled={login.isPending}
        className="w-full rounded-xl bg-brand-600 px-4 py-2.5 text-sm font-semibold text-white hover:bg-brand-700 disabled:opacity-50 transition-all duration-200 shadow-sm hover:shadow-md active:scale-[0.98]"
      >
        {login.isPending ? 'Entrando...' : 'Entrar'}
      </button>
    </form>
  )
}
