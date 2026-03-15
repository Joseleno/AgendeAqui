import { LayoutDashboard, CalendarDays, Users, BarChart3, Settings, Calendar, Search, CalendarPlus, ClipboardList, DollarSign } from 'lucide-react'
import type { LucideIcon } from 'lucide-react'

export interface NavItem {
  to: string
  label: string
  shortLabel: string
  icon: LucideIcon
}

export const adminNav: NavItem[] = [
  { to: '/dashboard',  label: 'Dashboard',       shortLabel: 'Home',    icon: LayoutDashboard },
  { to: '/agenda',     label: 'Agenda',          shortLabel: 'Agenda',  icon: CalendarDays },
  { to: '/gestao',     label: 'Gestão',          shortLabel: 'Gestão',  icon: Users },
  { to: '/relatorios', label: 'Relatórios',      shortLabel: 'Relat.',  icon: BarChart3 },
  { to: '/financeiro', label: 'Financeiro',       shortLabel: 'Financ.', icon: DollarSign },
  { to: '/config',     label: 'Configurações',   shortLabel: 'Config',  icon: Settings },
]

export const professionalNav: NavItem[] = [
  { to: '/meu-calendario', label: 'Meu Calendário', shortLabel: 'Calendário', icon: Calendar },
  { to: '/meus-pacientes', label: 'Meus Pacientes', shortLabel: 'Pacientes',  icon: Users },
  { to: '/minhas-stats',   label: 'Minhas Stats',   shortLabel: 'Stats',      icon: BarChart3 },
]

export const clientNav: NavItem[] = [
  { to: '/buscar',            label: 'Buscar Profissional', shortLabel: 'Buscar',     icon: Search },
  { to: '/agendar',           label: 'Agendar',             shortLabel: 'Agendar',    icon: CalendarPlus },
  { to: '/meus-agendamentos', label: 'Meus Agendamentos',   shortLabel: 'Histórico',  icon: ClipboardList },
]
