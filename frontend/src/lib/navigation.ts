import { LayoutDashboard, CalendarDays, Users, BarChart3, Settings, Calendar, Search, CalendarPlus, ClipboardList, DollarSign, UsersRound, Globe } from 'lucide-react'
import type { LucideIcon } from 'lucide-react'
import type { TenantLabels } from '../context/TenantContext'
import { DEFAULT_LABELS } from '../context/TenantContext'

export interface NavItem {
  to: string
  label: string
  shortLabel: string
  icon: LucideIcon
}

export function buildAdminNav(labels: TenantLabels = DEFAULT_LABELS): NavItem[] {
  return [
    { to: '/dashboard',  label: 'Dashboard',              shortLabel: 'Home',    icon: LayoutDashboard },
    { to: '/agenda',     label: 'Agenda',                 shortLabel: 'Agenda',  icon: CalendarDays },
    { to: '/gestao',     label: 'Gestão',                 shortLabel: 'Gestão',  icon: Users },
    { to: '/gerencial',  label: `Relatório de ${labels.teams}`, shortLabel: labels.teams, icon: UsersRound },
    { to: '/relatorios', label: 'Relatórios',             shortLabel: 'Relat.',  icon: BarChart3 },
    { to: '/financeiro', label: 'Financeiro',             shortLabel: 'Financ.', icon: DollarSign },
    { to: '/config',     label: 'Configurações',          shortLabel: 'Config',  icon: Settings },
  ]
}

export function buildProfessionalNav(labels: TenantLabels = DEFAULT_LABELS): NavItem[] {
  return [
    { to: '/meu-calendario', label: `Meu Calendário`,         shortLabel: 'Calendário', icon: Calendar },
    { to: '/meus-pacientes', label: `Meus ${labels.clients}`, shortLabel: labels.clients, icon: Users },
    { to: '/minhas-stats',   label: 'Minhas Stats',           shortLabel: 'Stats',       icon: BarChart3 },
  ]
}

export function buildClientNav(labels: TenantLabels = DEFAULT_LABELS): NavItem[] {
  return [
    { to: '/buscar',            label: `Buscar ${labels.professional}`, shortLabel: 'Buscar',    icon: Search },
    { to: '/agendar',           label: `Agendar`,                       shortLabel: 'Agendar',   icon: CalendarPlus },
    { to: '/meus-agendamentos', label: `Meus ${labels.appointments}`,   shortLabel: 'Histórico', icon: ClipboardList },
  ]
}

export function buildPlatformNav(): NavItem[] {
  return [
    { to: '/plataforma', label: 'Plataforma', shortLabel: 'Plataforma', icon: Globe },
  ]
}

// Legacy static exports — kept for backward compatibility during migration
export const adminNav        = buildAdminNav()
export const professionalNav = buildProfessionalNav()
export const clientNav       = buildClientNav()
