interface PaymentBadgeProps {
  status: string
  className?: string
}

const statusStyles: Record<string, { bg: string; text: string; label: string }> = {
  Pending: { bg: 'bg-amber-50', text: 'text-amber-700', label: 'Pendente' },
  Paid: { bg: 'bg-green-50', text: 'text-green-700', label: 'Pago' },
  Refunded: { bg: 'bg-gray-100', text: 'text-gray-600', label: 'Estornado' },
}

export function PaymentBadge({ status, className = '' }: PaymentBadgeProps) {
  const style = statusStyles[status] ?? statusStyles.Pending
  return (
    <span className={`${style.bg} ${style.text} text-xs px-2 py-0.5 rounded-full font-medium ${className}`}>
      {style.label}
    </span>
  )
}
