import { NavLink } from 'react-router-dom'
import { LayoutDashboard, CalendarDays, Users, BarChart3, Settings } from 'lucide-react'
import type { LucideIcon } from 'lucide-react'

const navItems: { to: string; label: string; icon: LucideIcon }[] = [
  { to: '/',           label: 'Dashboard',   icon: LayoutDashboard },
  { to: '/agenda',     label: 'Agenda',      icon: CalendarDays },
  { to: '/gestao',     label: 'Gestão',      icon: Users },
  { to: '/relatorios', label: 'Relatórios',  icon: BarChart3 },
  { to: '/config',     label: 'Configurações', icon: Settings },
]

export function Sidebar() {
  return (
    <aside className="hidden md:flex md:w-60 flex-col bg-brand-950 min-h-screen">
      <div className="p-5 border-b border-brand-800/50">
        <div className="flex items-center gap-2.5">
          <div className="w-8 h-8 rounded-lg bg-brand-500 flex items-center justify-center">
            <CalendarDays className="w-4.5 h-4.5 text-white" />
          </div>
          <h1 className="text-lg font-bold text-white tracking-tight">AgendeAqui</h1>
        </div>
      </div>
      <nav className="flex-1 p-3 space-y-1">
        {navItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            end={item.to === '/'}
            className={({ isActive }) =>
              `flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-all duration-200 ${
                isActive
                  ? 'bg-brand-500/15 text-brand-300 shadow-sm'
                  : 'text-brand-300/60 hover:bg-brand-800/40 hover:text-brand-200'
              }`
            }
          >
            <item.icon className="w-[18px] h-[18px] shrink-0" />
            <span>{item.label}</span>
          </NavLink>
        ))}
      </nav>
      <div className="p-3 border-t border-brand-800/50">
        <div className="px-3 py-2 text-xs text-brand-400/50">
          v1.0.0
        </div>
      </div>
    </aside>
  )
}
