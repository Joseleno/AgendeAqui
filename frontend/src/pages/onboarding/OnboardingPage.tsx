import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { CalendarDays, CheckCircle, Loader2 } from 'lucide-react'
import { useMutation } from '@tanstack/react-query'
import { toast } from '../../components/ui/Toast'

interface RegisterPayload {
  name: string
  slug: string
  adminEmail: string
  adminPassword: string
  adminName: string
}

interface RegisterResult {
  tenantId: string
  adminUserId: string
  trialEndsAt: string
  message: string
}

async function registerTenant(payload: RegisterPayload): Promise<RegisterResult> {
  const res = await fetch('/api/v1/onboarding/register', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  if (!res.ok) {
    const err = await res.json().catch(() => ({}))
    throw new Error(err.detail ?? 'Erro ao criar conta')
  }
  return res.json()
}

export function OnboardingPage() {
  const navigate = useNavigate()
  const [form, setForm] = useState({
    name: '',
    slug: '',
    adminName: '',
    adminEmail: '',
    adminPassword: '',
  })
  const [success, setSuccess] = useState<RegisterResult | null>(null)

  const mutation = useMutation({
    mutationFn: registerTenant,
    onSuccess: (data) => {
      setSuccess(data)
    },
    onError: (err: Error) => {
      toast('error', err.message)
    },
  })

  function handleSlugify(name: string) {
    const slug = name
      .toLowerCase()
      .replace(/\s+/g, '-')
      .replace(/[^a-z0-9-]/g, '')
      .replace(/-+/g, '-')
      .slice(0, 50)
    setForm((f) => ({ ...f, name, slug }))
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    mutation.mutate({
      name: form.name,
      slug: form.slug,
      adminEmail: form.adminEmail,
      adminPassword: form.adminPassword,
      adminName: form.adminName,
    })
  }

  if (success) {
    const trialEnd = new Date(success.trialEndsAt).toLocaleDateString('pt-BR')
    return (
      <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-brand-950 via-brand-900 to-brand-800 px-4">
        <div className="bg-white rounded-2xl shadow-modal p-8 max-w-md w-full text-center space-y-4">
          <CheckCircle className="w-12 h-12 text-green-500 mx-auto" />
          <h2 className="text-xl font-bold text-gray-900">Conta criada com sucesso!</h2>
          <p className="text-sm text-gray-600">
            Seu teste gratuito está ativo até <span className="font-semibold text-brand-700">{trialEnd}</span>.
          </p>
          <p className="text-sm text-gray-500">
            Faça login com o seu e-mail em{' '}
            <span className="font-mono text-brand-600">/{form.slug}/login</span>
          </p>
          <button
            onClick={() => navigate(`/${form.slug}/login`)}
            className="w-full rounded-xl bg-brand-600 py-2.5 text-sm font-semibold text-white hover:bg-brand-700 transition-colors"
          >
            Ir para o login
          </button>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-brand-950 via-brand-900 to-brand-800 px-4 py-10">
      <div className="w-full max-w-md space-y-6">
        <div className="text-center">
          <div className="flex items-center justify-center gap-2 mb-3">
            <CalendarDays className="w-8 h-8 text-white" />
            <span className="text-2xl font-bold text-white">AgendeAqui</span>
          </div>
          <h1 className="text-xl font-semibold text-white">Crie sua conta gratuita</h1>
          <p className="text-sm text-white/70 mt-1">14 dias de teste, sem cartão de crédito</p>
        </div>

        <div className="bg-white rounded-2xl shadow-modal p-6">
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Nome da clínica / empresa</label>
              <input
                required
                value={form.name}
                onChange={(e) => handleSlugify(e.target.value)}
                placeholder="Ex: Clínica Vida"
                className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-brand-500 focus:ring-2 focus:ring-brand-100"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Slug <span className="text-gray-400 font-normal">(URL de acesso)</span>
              </label>
              <div className="flex items-center gap-1">
                <span className="text-xs text-gray-400 shrink-0">app.com/</span>
                <input
                  required
                  value={form.slug}
                  onChange={(e) => setForm((f) => ({ ...f, slug: e.target.value.toLowerCase().replace(/[^a-z0-9-]/g, '') }))}
                  placeholder="clinica-vida"
                  className="flex-1 rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-brand-500 focus:ring-2 focus:ring-brand-100"
                />
              </div>
            </div>

            <hr className="border-gray-100" />

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Seu nome</label>
              <input
                required
                value={form.adminName}
                onChange={(e) => setForm((f) => ({ ...f, adminName: e.target.value }))}
                placeholder="Nome do administrador"
                className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-brand-500 focus:ring-2 focus:ring-brand-100"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">E-mail</label>
              <input
                required
                type="email"
                value={form.adminEmail}
                onChange={(e) => setForm((f) => ({ ...f, adminEmail: e.target.value }))}
                placeholder="voce@clinicavida.com"
                className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-brand-500 focus:ring-2 focus:ring-brand-100"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Senha</label>
              <input
                required
                type="password"
                minLength={8}
                value={form.adminPassword}
                onChange={(e) => setForm((f) => ({ ...f, adminPassword: e.target.value }))}
                placeholder="Mínimo 8 caracteres"
                className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-brand-500 focus:ring-2 focus:ring-brand-100"
              />
            </div>

            <button
              type="submit"
              disabled={mutation.isPending}
              className="w-full flex items-center justify-center gap-2 rounded-xl bg-brand-600 py-2.5 text-sm font-semibold text-white hover:bg-brand-700 disabled:opacity-50 transition-colors"
            >
              {mutation.isPending && <Loader2 className="w-4 h-4 animate-spin" />}
              {mutation.isPending ? 'Criando conta...' : 'Criar conta grátis'}
            </button>
          </form>
        </div>

        <p className="text-center text-sm text-white/60">
          Já tem uma conta?{' '}
          <Link to="/login" className="text-white hover:underline font-medium">
            Fazer login
          </Link>
        </p>
      </div>
    </div>
  )
}
