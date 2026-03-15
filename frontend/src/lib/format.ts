import { format as fnsFormat, parseISO } from 'date-fns'
import { ptBR } from 'date-fns/locale'

export function formatDate(dateStr: string, pattern = "dd/MM/yyyy"): string {
  const date = typeof dateStr === 'string' && dateStr.includes('T')
    ? parseISO(dateStr)
    : parseISO(dateStr + 'T00:00:00')
  return fnsFormat(date, pattern, { locale: ptBR })
}

export function formatDateLong(dateStr: string): string {
  return formatDate(dateStr, "d 'de' MMMM 'de' yyyy")
}

export function formatWeekday(dateStr: string): string {
  return formatDate(dateStr, 'EEEE')
}

export function formatTime(timeStr: string): string {
  return timeStr.slice(0, 5)
}

export function formatCurrency(value: number): string {
  return new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL',
  }).format(value)
}

export function formatPhone(phone: string): string {
  const digits = phone.replace(/\D/g, '')
  const national = digits.startsWith('55') ? digits.slice(2) : digits
  if (national.length === 11) {
    return `(${national.slice(0, 2)}) ${national.slice(2, 7)}-${national.slice(7)}`
  }
  if (national.length === 10) {
    return `(${national.slice(0, 2)}) ${national.slice(2, 6)}-${national.slice(6)}`
  }
  return phone
}
