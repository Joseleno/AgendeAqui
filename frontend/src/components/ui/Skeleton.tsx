interface SkeletonProps {
  className?: string
  style?: React.CSSProperties
}

function SkeletonBase({ className = '', style }: SkeletonProps) {
  return (
    <div
      className={`bg-gray-200 rounded-lg animate-pulse ${className}`}
      style={style}
      aria-hidden="true"
    />
  )
}

export function SkeletonText({
  lines = 3,
  className = '',
}: { lines?: number; className?: string }) {
  return (
    <div className={`space-y-2 ${className}`}>
      {Array.from({ length: lines }).map((_, i) => (
        <SkeletonBase
          key={i}
          className={`h-4 ${i === lines - 1 ? 'w-3/4' : 'w-full'}`}
        />
      ))}
    </div>
  )
}

export function SkeletonCard({ className = '' }: SkeletonProps) {
  return (
    <div
      className={`bg-white rounded-2xl border border-gray-100 shadow-card p-5 ${className}`}
    >
      <div className="flex items-center justify-between mb-4">
        <SkeletonBase className="h-4 w-24" />
        <SkeletonBase className="h-4 w-4 rounded-full" />
      </div>
      <SkeletonBase className="h-8 w-16 mb-2" />
      <SkeletonBase className="h-3 w-20" />
    </div>
  )
}

export function SkeletonTable({
  rows = 5,
  cols = 4,
  className = '',
}: { rows?: number; cols?: number; className?: string }) {
  return (
    <div className={`bg-white rounded-xl border border-gray-200 overflow-hidden ${className}`}>
      <div className="bg-gray-50 px-4 py-3 flex gap-4">
        {Array.from({ length: cols }).map((_, i) => (
          <SkeletonBase key={i} className="h-3 w-20" />
        ))}
      </div>
      {Array.from({ length: rows }).map((_, r) => (
        <div key={r} className="px-4 py-3 flex gap-4 border-t border-gray-100">
          {Array.from({ length: cols }).map((_, c) => (
            <SkeletonBase
              key={c}
              className={`h-4 ${c === 0 ? 'w-32' : 'w-20'}`}
            />
          ))}
        </div>
      ))}
    </div>
  )
}

export function SkeletonChart({ className = '' }: SkeletonProps) {
  return (
    <div className={`bg-white rounded-2xl border border-gray-100 shadow-card p-5 ${className}`}>
      <SkeletonBase className="h-4 w-32 mb-4" />
      <div className="flex items-end gap-2 h-48">
        {[40, 65, 45, 80, 55, 70, 50, 85, 60, 75, 48, 90].map((h, i) => (
          <SkeletonBase
            key={i}
            className="flex-1 rounded-t-md"
            style={{ height: `${h}%` } as React.CSSProperties}
          />
        ))}
      </div>
    </div>
  )
}
