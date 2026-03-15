import { LogOut } from 'lucide-react'
import { useLogout } from '../../hooks/useAuth'
import { NotificationBell } from '../notifications/NotificationBell'

export function Header() {
  const logout = useLogout()

  return (
    <header className="h-16 bg-white border-b border-gray-200/80 flex items-center justify-between px-4 md:px-6">
      <h2 className="text-base font-semibold text-gray-800 md:hidden">AgendeAqui</h2>
      <div className="hidden md:block" />
      <div className="flex items-center gap-2">
        <NotificationBell />
        <div className="w-px h-6 bg-gray-200 mx-1" />
        <button
          onClick={logout}
          className="flex items-center gap-2 px-3 py-1.5 rounded-lg text-sm text-gray-500 hover:text-red-600 hover:bg-red-50 transition-colors"
          title="Sair"
        >
          <LogOut className="w-4 h-4" />
          <span className="hidden sm:inline">Sair</span>
        </button>
      </div>
    </header>
  )
}
