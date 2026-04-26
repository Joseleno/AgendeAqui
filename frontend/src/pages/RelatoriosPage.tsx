import { NavLink, Outlet } from 'react-router-dom'
import { useTenantContext } from '../context/TenantContext'

export function RelatoriosPage() {
  const { labels, features } = useTenantContext()

  const tabs = [
    { to: '/relatorios/atendimentos', label: labels.appointments },
    { to: '/relatorios/faturamento',  label: 'Faturamento' },
    { to: '/relatorios/ranking',      label: `Ranking de ${labels.professionals}` },
    ...(features.hasTeams ? [{ to: '/relatorios/equipes', label: labels.teams }] : []),
    { to: '/relatorios/conflitos',    label: 'Conflitos' },
    { to: '/relatorios/exportar',     label: 'Exportar' },
  ]

  return (
    <div className="space-y-4">
      <nav className="flex gap-1 border-b border-gray-200 pb-px flex-wrap">
        {tabs.map((tab) => (
          <NavLink
            key={tab.to}
            to={tab.to}
            className={({ isActive }) =>
              `px-3 py-2 text-sm font-medium rounded-t-lg transition-colors ${
                isActive
                  ? 'bg-white border border-b-white border-gray-200 text-brand-600 -mb-px'
                  : 'text-gray-500 hover:text-gray-700'
              }`
            }
          >
            {tab.label}
          </NavLink>
        ))}
      </nav>
      <Outlet />
    </div>
  )
}
