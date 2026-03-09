interface PagedListProps {
  page: number
  totalPages: number
  onPageChange: (page: number) => void
}

export function PagedList({ page, totalPages, onPageChange }: PagedListProps) {
  if (totalPages <= 1) return null

  return (
    <div className="flex items-center justify-center gap-2 mt-4">
      <button
        disabled={page <= 1}
        onClick={() => onPageChange(page - 1)}
        className="px-3 py-1 text-sm rounded-lg border border-gray-300 text-gray-600 hover:bg-gray-50 disabled:opacity-40"
      >
        Anterior
      </button>
      <span className="text-sm text-gray-600">
        {page} de {totalPages}
      </span>
      <button
        disabled={page >= totalPages}
        onClick={() => onPageChange(page + 1)}
        className="px-3 py-1 text-sm rounded-lg border border-gray-300 text-gray-600 hover:bg-gray-50 disabled:opacity-40"
      >
        Proximo
      </button>
    </div>
  )
}
