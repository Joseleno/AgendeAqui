import { Lock, FileText, Trash2, Pencil } from 'lucide-react'
import type { ClinicalNote } from '../../hooks/useClinicalNotes'
import { formatDate, formatTime } from '../../lib/format'

interface NoteCardProps {
  note: ClinicalNote
  isAuthor: boolean
  onEdit?: (note: ClinicalNote) => void
  onDelete?: (note: ClinicalNote) => void
  onClick?: (note: ClinicalNote) => void
}

export function NoteCard({ note, isAuthor, onEdit, onDelete, onClick }: NoteCardProps) {
  return (
    <div
      className="bg-white rounded-xl border border-gray-200 p-4 hover:shadow-card-hover transition-all cursor-pointer group"
      onClick={() => onClick?.(note)}
    >
      <div className="flex items-start justify-between mb-2">
        <div className="flex items-center gap-2 min-w-0">
          <FileText className="w-4 h-4 text-brand-500 shrink-0" />
          <h4 className="text-sm font-semibold text-gray-800 truncate">{note.title}</h4>
          {note.isPrivate && (
            <span className="flex items-center gap-1 text-[10px] text-amber-600 bg-amber-50 px-1.5 py-0.5 rounded-full shrink-0">
              <Lock className="w-2.5 h-2.5" />
              Privada
            </span>
          )}
        </div>
        {isAuthor && (
          <div className="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity shrink-0">
            <button
              onClick={(e) => { e.stopPropagation(); onEdit?.(note) }}
              className="p-1 rounded text-gray-400 hover:text-brand-600 hover:bg-brand-50"
              title="Editar"
            >
              <Pencil className="w-3.5 h-3.5" />
            </button>
            <button
              onClick={(e) => { e.stopPropagation(); onDelete?.(note) }}
              className="p-1 rounded text-gray-400 hover:text-red-600 hover:bg-red-50"
              title="Excluir"
            >
              <Trash2 className="w-3.5 h-3.5" />
            </button>
          </div>
        )}
      </div>
      <p className="text-xs text-gray-500 line-clamp-2 mb-2">{note.content}</p>
      <div className="flex items-center justify-between text-[11px] text-gray-400">
        <span>{note.professionalName}</span>
        <span>{formatDate(note.createdAt)} {note.createdAt.includes('T') ? formatTime(note.createdAt.split('T')[1]) : ''}</span>
      </div>
    </div>
  )
}
