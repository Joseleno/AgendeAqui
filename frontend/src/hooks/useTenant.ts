import { useQuery } from '@tanstack/react-query'
import { authStore } from '../store/auth-store'

interface TenantInfo {
  id: string
  name: string
}

export function useTenantResolver(slug: string | undefined) {
  return useQuery({
    queryKey: ['tenant', 'resolve', slug],
    queryFn: async () => {
      const res = await fetch(`/api/v1/auth/tenant/${slug}`)
      if (!res.ok) throw new Error('Tenant not found')
      const data = await res.json() as TenantInfo
      authStore.setTenantId(data.id)
      return data
    },
    enabled: !!slug,
    staleTime: Infinity,
  })
}
