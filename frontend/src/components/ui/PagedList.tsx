import { ChevronLeft, ChevronRight } from 'lucide-react'

interface PagedListProps {
  page: number
  totalPages: number
  onPageChange: (page: number) => void
}

export function PagedList({ page, totalPages, onPageChange }: PagedListProps) {
  if (totalPages <= 1) return null

  return (
    <div className="flex items-center justify-center gap-3 mt-4">
      <button
        disabled={page <= 1}
        onClick={() => onPageChange(page - 1)}
        className="flex items-center gap-1 px-3 py-1.5 text-sm rounded-xl border border-gray-200 text-gray-600 hover:bg-gray-50 disabled:opacity-40 transition-colors"
      >
        <ChevronLeft className="w-4 h-4" />
        Anterior
      </button>
      <span className="text-sm text-gray-500 tabular-nums">
        {page} de {totalPages}
      </span>
      <button
        disabled={page >= totalPages}
        onClick={() => onPageChange(page + 1)}
        className="flex items-center gap-1 px-3 py-1.5 text-sm rounded-xl border border-gray-200 text-gray-600 hover:bg-gray-50 disabled:opacity-40 transition-colors"
      >
        Proximo
        <ChevronRight className="w-4 h-4" />
      </button>
    </div>
  )
}
