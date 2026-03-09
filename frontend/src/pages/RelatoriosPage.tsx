import { NavLink, Outlet } from 'react-router-dom'

const tabs = [
  { to: '/relatorios/atendimentos', label: 'Atendimentos' },
  { to: '/relatorios/faturamento', label: 'Faturamento' },
]

export function RelatoriosPage() {
  return (
    <div className="space-y-4">
      <nav className="flex gap-1 border-b border-gray-200 pb-px">
        {tabs.map((tab) => (
          <NavLink
            key={tab.to}
            to={tab.to}
            className={({ isActive }) =>
              `px-3 py-2 text-sm font-medium rounded-t-lg transition-colors ${
                isActive
                  ? 'bg-white border border-b-white border-gray-200 text-indigo-600 -mb-px'
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
