import { NavLink } from 'react-router-dom'

const navItems = [
  { to: '/',           label: 'Home',    icon: '🏠' },
  { to: '/agenda',     label: 'Agenda',  icon: '📅' },
  { to: '/gestao',     label: 'Gestao',  icon: '👥' },
  { to: '/relatorios', label: 'Relat.',  icon: '📊' },
  { to: '/config',     label: 'Config',  icon: '⚙️' },
]

export function BottomNav() {
  return (
    <nav className="fixed bottom-0 left-0 right-0 md:hidden bg-white border-t border-gray-200 z-50">
      <div className="flex justify-around py-2">
        {navItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            end={item.to === '/'}
            className={({ isActive }) =>
              `flex flex-col items-center gap-0.5 text-xs font-medium transition-colors ${
                isActive ? 'text-indigo-600' : 'text-gray-500'
              }`
            }
          >
            <span className="text-lg">{item.icon}</span>
            <span>{item.label}</span>
          </NavLink>
        ))}
      </div>
    </nav>
  )
}
