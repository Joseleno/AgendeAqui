import { useState, useMemo } from 'react'
import { Mail, Lock } from 'lucide-react'
import { useLogin } from '../../hooks/useAuth'
import { toast } from '../../components/ui/Toast'
import { FormField } from '../../components/ui/FormField'
import { useFormValidation } from '../../hooks/useFormValidation'
import { validateRequired, validateEmail } from '../../lib/validators'

export function LoginForm() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const login = useLogin()

  const rules = useMemo(() => ({
    email: [validateRequired('Email'), validateEmail()],
    password: [validateRequired('Senha')],
  }), [])

  const { onBlur, validateAll, getError } = useFormValidation(rules)

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!validateAll({ email, password })) return
    login.mutate(
      { email, password },
      {
        onError: () => {
          toast('error', 'Email ou senha inválidos')
        },
      },
    )
  }

  const inputClass = "w-full rounded-xl border border-gray-200 bg-gray-50/50 pl-9 pr-3 py-2.5 text-sm focus:bg-white focus:border-brand-500 focus:ring-1 focus:ring-brand-500 outline-none transition-all duration-200"

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <FormField label="Email" htmlFor="email" error={getError('email')}>
        <div className="relative">
          <Mail className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            id="email"
            type="email"
            required
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            onBlur={() => onBlur('email', email)}
            className={inputClass}
            placeholder="seu@email.com"
          />
        </div>
      </FormField>

      <FormField label="Senha" htmlFor="password" error={getError('password')}>
        <div className="relative">
          <Lock className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            id="password"
            type="password"
            required
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            onBlur={() => onBlur('password', password)}
            className={inputClass}
            placeholder="********"
          />
        </div>
      </FormField>

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
