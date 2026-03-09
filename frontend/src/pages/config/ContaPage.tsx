import { useState, useEffect } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '../../lib/api-client'
import { authStore } from '../../store/auth-store'
import { toast } from '../../components/ui/Toast'

interface Tenant {
  id: string
  name: string
  slug: string
  status: string
  plan: string
  createdAt: string
}

export function ContaPage() {
  const { tenantId } = authStore.getState()
  const queryClient = useQueryClient()

  const { data: tenant, isLoading } = useQuery({
    queryKey: ['tenant', tenantId],
    queryFn: () => api.get<Tenant>(`/tenants/${tenantId}`),
    enabled: !!tenantId,
  })

  const [name, setName] = useState('')
  const [plan, setPlan] = useState('')

  useEffect(() => {
    if (tenant) {
      setName(tenant.name)
      setPlan(tenant.plan)
    }
  }, [tenant])

  const updateMutation = useMutation({
    mutationFn: (payload: { name: string; plan: string }) =>
      api.put(`/tenants/${tenantId}`, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tenant'] })
      toast('success', 'Conta atualizada')
    },
    onError: () => toast('error', 'Erro ao atualizar conta'),
  })

  if (isLoading) return <p className="text-sm text-gray-400">Carregando...</p>
  if (!tenant) return <p className="text-sm text-gray-400">Conta nao encontrada</p>

  return (
    <div className="space-y-4">
      <h2 className="text-lg font-semibold text-gray-800">Minha Conta</h2>

      <div className="bg-white rounded-xl border border-gray-200 p-5 space-y-4 max-w-md">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Slug</label>
          <p className="text-sm text-gray-500 bg-gray-50 rounded-lg px-3 py-2">{tenant.slug}</p>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
          <p className="text-sm text-gray-500 bg-gray-50 rounded-lg px-3 py-2">{tenant.status}</p>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Nome</label>
          <input
            value={name}
            onChange={(e) => setName(e.target.value)}
            className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-indigo-500"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Plano</label>
          <select
            value={plan}
            onChange={(e) => setPlan(e.target.value)}
            className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-indigo-500"
          >
            <option value="Free">Free</option>
            <option value="Starter">Starter</option>
            <option value="Professional">Professional</option>
            <option value="Enterprise">Enterprise</option>
          </select>
        </div>

        <button
          onClick={() => updateMutation.mutate({ name, plan })}
          disabled={updateMutation.isPending}
          className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-semibold text-white hover:bg-indigo-700 disabled:opacity-50 transition-colors"
        >
          {updateMutation.isPending ? 'Salvando...' : 'Salvar'}
        </button>
      </div>
    </div>
  )
}
