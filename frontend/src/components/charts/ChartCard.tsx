import type { ReactNode } from 'react'
import { SkeletonChart } from '../ui/Skeleton'

interface ChartCardProps {
  title: string
  children: ReactNode
  isLoading?: boolean
  className?: string
}

export function ChartCard({ title, children, isLoading, className = '' }: ChartCardProps) {
  if (isLoading) {
    return <SkeletonChart className={className} />
  }

  return (
    <div className={`bg-white rounded-2xl border border-gray-100 shadow-card p-5 ${className}`}>
      <h3 className="text-sm font-semibold text-gray-700 mb-4">{title}</h3>
      {children}
    </div>
  )
}
