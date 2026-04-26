import { NavLink } from 'react-router-dom'
import { useAuthState } from '../../hooks/useAuth'
import { useTenantContext } from '../../context/TenantContext'
import { buildAdminNav, buildProfessionalNav, buildClientNav } from '../../lib/navigation'

export function BottomNav() {
  const { isAdmin, isProfessional } = useAuthState()
  const { labels } = useTenantContext()

  const navItems = isAdmin
    ? buildAdminNav(labels)
    : isProfessional
    ? buildProfessionalNav(labels)
    : buildClientNav(labels)

  return (
    <nav className="fixed bottom-0 left-0 right-0 md:hidden bg-white/95 backdrop-blur-lg border-t border-gray-200/80 z-50">
      <div className="flex justify-around py-1.5 px-1">
        {navItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              `flex flex-col items-center gap-0.5 px-2 py-1.5 rounded-lg text-xs font-medium transition-all duration-200 ${
                isActive
                  ? 'text-brand-600 bg-brand-50'
                  : 'text-gray-400 hover:text-gray-600'
              }`
            }
          >
            <item.icon className="w-5 h-5" />
            <span className="text-[10px]">{item.shortLabel}</span>
          </NavLink>
        ))}
      </div>
    </nav>
  )
}
