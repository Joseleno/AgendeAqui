import { NavLink } from 'react-router-dom'
import { LayoutDashboard, CalendarDays, Users, BarChart3, Settings } from 'lucide-react'
import type { LucideIcon } from 'lucide-react'

const navItems: { to: string; label: string; icon: LucideIcon }[] = [
  { to: '/',           label: 'Home',    icon: LayoutDashboard },
  { to: '/agenda',     label: 'Agenda',  icon: CalendarDays },
  { to: '/gestao',     label: 'Gestao',  icon: Users },
  { to: '/relatorios', label: 'Relat.',  icon: BarChart3 },
  { to: '/config',     label: 'Config',  icon: Settings },
]

export function BottomNav() {
  return (
    <nav className="fixed bottom-0 left-0 right-0 md:hidden bg-white/95 backdrop-blur-lg border-t border-gray-200/80 z-50">
      <div className="flex justify-around py-1.5 px-1">
        {navItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            end={item.to === '/'}
            className={({ isActive }) =>
              `flex flex-col items-center gap-0.5 px-2 py-1.5 rounded-lg text-xs font-medium transition-all duration-200 ${
                isActive
                  ? 'text-brand-600 bg-brand-50'
                  : 'text-gray-400 hover:text-gray-600'
              }`
            }
          >
            <item.icon className="w-5 h-5" />
            <span className="text-[10px]">{item.label}</span>
          </NavLink>
        ))}
      </div>
    </nav>
  )
}
