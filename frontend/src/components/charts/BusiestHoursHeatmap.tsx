import { useMemo } from 'react'
import type { HourSlot } from '../../hooks/useAdvancedReports'

interface BusiestHoursHeatmapProps {
  data: HourSlot[]
}

const DAYS = ['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb']
const HOURS = Array.from({ length: 14 }, (_, i) => i + 7) // 7h to 20h

function getIntensity(count: number, maxCount: number): string {
  if (maxCount === 0 || count === 0) return 'bg-gray-50 text-gray-300'
  const ratio = count / maxCount
  if (ratio >= 0.75) return 'bg-brand-600 text-white'
  if (ratio >= 0.5) return 'bg-brand-400 text-white'
  if (ratio >= 0.25) return 'bg-brand-200 text-brand-800'
  return 'bg-brand-100 text-brand-600'
}

export function BusiestHoursHeatmap({ data }: BusiestHoursHeatmapProps) {
  const { countMap, maxCount } = useMemo(() => {
    const map = new Map<string, number>()
    let max = 0
    for (const slot of data) {
      const key = `${slot.dayOfWeek}-${slot.hour}`
      map.set(key, slot.count)
      if (slot.count > max) max = slot.count
    }
    return { countMap: map, maxCount: max }
  }, [data])

  return (
    <div className="overflow-x-auto">
      <div className="min-w-[600px]">
        {/* Header - Hours */}
        <div className="grid gap-1" style={{ gridTemplateColumns: `60px repeat(${HOURS.length}, 1fr)` }}>
          <div />
          {HOURS.map((h) => (
            <div key={h} className="text-center text-[10px] text-gray-500 font-medium py-1">
              {h}h
            </div>
          ))}
        </div>

        {/* Body - Days x Hours */}
        {DAYS.map((day, dayIdx) => (
          <div
            key={dayIdx}
            className="grid gap-1 mt-1"
            style={{ gridTemplateColumns: `60px repeat(${HOURS.length}, 1fr)` }}
          >
            <div className="text-xs text-gray-600 font-medium flex items-center">
              {day}
            </div>
            {HOURS.map((hour) => {
              const count = countMap.get(`${dayIdx}-${hour}`) ?? 0
              const intensity = getIntensity(count, maxCount)
              return (
                <div
                  key={hour}
                  className={`${intensity} rounded-md aspect-square flex items-center justify-center text-xs font-semibold transition-all hover:scale-110 cursor-default`}
                  title={`${day} ${hour}h: ${count} agendamento${count !== 1 ? 's' : ''}`}
                >
                  {count > 0 ? count : ''}
                </div>
              )
            })}
          </div>
        ))}

        {/* Legend */}
        <div className="flex items-center gap-2 mt-4 justify-end">
          <span className="text-[10px] text-gray-500">Menos</span>
          <div className="w-4 h-4 rounded bg-gray-50 border border-gray-200" />
          <div className="w-4 h-4 rounded bg-brand-100" />
          <div className="w-4 h-4 rounded bg-brand-200" />
          <div className="w-4 h-4 rounded bg-brand-400" />
          <div className="w-4 h-4 rounded bg-brand-600" />
          <span className="text-[10px] text-gray-500">Mais</span>
        </div>
      </div>
    </div>
  )
}
