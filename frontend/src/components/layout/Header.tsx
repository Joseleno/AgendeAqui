import { useLogout } from '../../hooks/useAuth'

export function Header() {
  const logout = useLogout()

  return (
    <header className="h-14 bg-white border-b border-gray-200 flex items-center justify-between px-4">
      <h2 className="text-sm font-semibold text-gray-700 md:hidden">AgendeAqui</h2>
      <div className="flex items-center gap-3 ml-auto">
        <button
          onClick={logout}
          className="text-sm text-gray-500 hover:text-gray-700 transition-colors"
        >
          Sair
        </button>
      </div>
    </header>
  )
}
