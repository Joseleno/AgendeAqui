import { NavLink } from 'react-router-dom'

const navItems = [
  { to: '/',           label: 'Home',       icon: '🏠' },
  { to: '/agenda',     label: 'Agenda',     icon: '📅' },
  { to: '/gestao',     label: 'Gestao',     icon: '👥' },
  { to: '/relatorios', label: 'Relatorios', icon: '📊' },
  { to: '/config',     label: 'Config',     icon: '⚙️' },
]

export function Sidebar() {
  return (
    <aside className="hidden md:flex md:w-56 flex-col bg-white border-r border-gray-200 min-h-screen">
      <div className="p-4 border-b border-gray-200">
        <h1 className="text-lg font-bold text-indigo-600">AgendeAqui</h1>
      </div>
      <nav className="flex-1 p-2 space-y-1">
        {navItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            end={item.to === '/'}
            className={({ isActive }) =>
              `flex items-center gap-3 px-3 py-2 rounded-lg text-sm font-medium transition-colors ${
                isActive
                  ? 'bg-indigo-50 text-indigo-700'
                  : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900'
              }`
            }
          >
            <span>{item.icon}</span>
            <span>{item.label}</span>
          </NavLink>
        ))}
      </nav>
    </aside>
  )
}
