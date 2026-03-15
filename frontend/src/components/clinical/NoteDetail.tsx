import { Lock, FileText } from 'lucide-react'
import type { ClinicalNote } from '../../hooks/useClinicalNotes'
import { formatDate, formatTime } from '../../lib/format'

interface NoteDetailProps {
  note: ClinicalNote
  onClose: () => void
}

export function NoteDetail({ note, onClose }: NoteDetailProps) {
  return (
    <div className="fixed inset-0 z-50 flex items-end md:items-center justify-center">
      <div className="absolute inset-0 bg-black/30" onClick={onClose} />
      <div className="relative bg-white w-full md:max-w-lg md:rounded-xl rounded-t-xl shadow-xl p-5 max-h-[80vh] overflow-auto">
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center gap-2">
            <FileText className="w-5 h-5 text-brand-500" />
            <h3 className="text-lg font-semibold text-gray-800">{note.title}</h3>
          </div>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600 text-xl">&times;</button>
        </div>

        <div className="flex items-center gap-3 text-xs text-gray-500 mb-4">
          <span>{note.professionalName}</span>
          <span>&middot;</span>
          <span>{formatDate(note.createdAt)}</span>
          {note.createdAt.includes('T') && (
            <>
              <span>&middot;</span>
              <span>{formatTime(note.createdAt.split('T')[1])}</span>
            </>
          )}
          {note.isPrivate && (
            <span className="flex items-center gap-1 text-amber-600 bg-amber-50 px-1.5 py-0.5 rounded-full">
              <Lock className="w-2.5 h-2.5" />
              Privada
            </span>
          )}
        </div>

        <div className="prose prose-sm max-w-none text-gray-700 whitespace-pre-wrap">
          {note.content}
        </div>

        {note.updatedAt !== note.createdAt && (
          <p className="text-[11px] text-gray-400 mt-4">
            Atualizado em {formatDate(note.updatedAt)}
          </p>
        )}
      </div>
    </div>
  )
}
