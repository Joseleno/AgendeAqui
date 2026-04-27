import { useState, useEffect } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { Palette } from 'lucide-react'
import { api } from '../../lib/api-client'
import { toast } from '../../components/ui/Toast'
import { useTenantContext } from '../../context/TenantContext'

const PRESET_COLORS = [
  { label: 'Violeta (padrão)', value: '#6366f1' },
  { label: 'Azul', value: '#3b82f6' },
  { label: 'Verde', value: '#22c55e' },
  { label: 'Laranja', value: '#f97316' },
  { label: 'Rosa', value: '#ec4899' },
  { label: 'Ciano', value: '#06b6d4' },
  { label: 'Âmbar', value: '#f59e0b' },
  { label: 'Vermelho', value: '#ef4444' },
]

export function TemaPage() {
  const { theme } = useTenantContext()
  const queryClient = useQueryClient()

  const [primaryColor, setPrimaryColor] = useState(theme.primaryColor)
  const [logoUrl, setLogoUrl] = useState(theme.logoUrl ?? '')
  const [faviconUrl, setFaviconUrl] = useState(theme.faviconUrl ?? '')

  useEffect(() => {
    setPrimaryColor(theme.primaryColor)
    setLogoUrl(theme.logoUrl ?? '')
    setFaviconUrl(theme.faviconUrl ?? '')
  }, [theme])

  const mutation = useMutation({
    mutationFn: () =>
      api.put('/tenant/theme', {
        primaryColor,
        logoUrl: logoUrl || null,
        faviconUrl: faviconUrl || null,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tenant', 'context'] })
      toast('success', 'Tema atualizado')
    },
    onError: () => toast('error', 'Erro ao salvar tema'),
  })

  return (
    <div className="space-y-4">
      <h2 className="text-lg font-semibold text-gray-800 flex items-center gap-2">
        <Palette className="w-5 h-5 text-brand-500" />
        Tema e identidade visual
      </h2>

      <div className="bg-white rounded-xl border border-gray-200 p-5 space-y-5 max-w-lg">
        {/* Color presets */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">Cor principal</label>
          <div className="flex flex-wrap gap-2 mb-3">
            {PRESET_COLORS.map((c) => (
              <button
                key={c.value}
                onClick={() => setPrimaryColor(c.value)}
                title={c.label}
                className="w-8 h-8 rounded-full border-2 transition-transform hover:scale-110"
                style={{
                  backgroundColor: c.value,
                  borderColor: primaryColor === c.value ? '#1e293b' : 'transparent',
                }}
              />
            ))}
          </div>
          <div className="flex items-center gap-2">
            <input
              type="color"
              value={primaryColor}
              onChange={(e) => setPrimaryColor(e.target.value)}
              className="w-10 h-10 rounded cursor-pointer border border-gray-300"
            />
            <input
              type="text"
              value={primaryColor}
              onChange={(e) => {
                if (/^#[0-9a-fA-F]{0,6}$/.test(e.target.value))
                  setPrimaryColor(e.target.value)
              }}
              className="w-28 rounded-lg border border-gray-300 px-2 py-1.5 text-sm font-mono outline-none focus:border-brand-500"
            />
            <div
              className="w-8 h-8 rounded-full border border-gray-200"
              style={{ backgroundColor: primaryColor }}
            />
          </div>
        </div>

        {/* Logo URL */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">URL do logotipo</label>
          <input
            type="url"
            value={logoUrl}
            onChange={(e) => setLogoUrl(e.target.value)}
            placeholder="https://sua-empresa.com/logo.png"
            className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-brand-500"
          />
          {logoUrl && (
            <img
              src={logoUrl}
              alt="Preview do logotipo"
              className="mt-2 h-12 object-contain rounded border border-gray-100 bg-gray-50 p-1"
              onError={(e) => { (e.target as HTMLImageElement).style.display = 'none' }}
            />
          )}
        </div>

        {/* Favicon URL */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">URL do favicon</label>
          <input
            type="url"
            value={faviconUrl}
            onChange={(e) => setFaviconUrl(e.target.value)}
            placeholder="https://sua-empresa.com/favicon.ico"
            className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-brand-500"
          />
        </div>

        <button
          onClick={() => mutation.mutate()}
          disabled={mutation.isPending}
          className="rounded-lg bg-brand-600 px-4 py-2 text-sm font-semibold text-white hover:bg-brand-700 disabled:opacity-50 transition-colors"
        >
          {mutation.isPending ? 'Salvando...' : 'Salvar tema'}
        </button>
      </div>
    </div>
  )
}
