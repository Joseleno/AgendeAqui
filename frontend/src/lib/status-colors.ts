export const statusColors: Record<string, { bg: string; text: string; label: string }> = {
  Scheduled:  { bg: 'bg-yellow-100', text: 'text-yellow-800', label: 'Agendado' },
  Confirmed:  { bg: 'bg-green-100',  text: 'text-green-800',  label: 'Confirmado' },
  InProgress: { bg: 'bg-blue-100',   text: 'text-blue-800',   label: 'Em andamento' },
  Completed:  { bg: 'bg-gray-100',   text: 'text-gray-800',   label: 'Concluido' },
  Cancelled:  { bg: 'bg-red-100',    text: 'text-red-800',    label: 'Cancelado' },
  NoShow:     { bg: 'bg-gray-800',   text: 'text-gray-100',   label: 'Faltou' },
}

export function getStatusStyle(status: string) {
  return statusColors[status] ?? { bg: 'bg-gray-100', text: 'text-gray-600', label: status }
}
