import { Video } from 'lucide-react'

interface TeleconsultaBadgeProps {
  className?: string
}

export function TeleconsultaBadge({ className = '' }: TeleconsultaBadgeProps) {
  return (
    <span className={`flex items-center gap-1 text-[10px] text-indigo-600 bg-indigo-50 px-1.5 py-0.5 rounded-full font-medium ${className}`}>
      <Video className="w-2.5 h-2.5" />
      Teleconsulta
    </span>
  )
}
