import { useState } from 'react'
import { Lock, Globe } from 'lucide-react'
import type { ClinicalNote } from '../../hooks/useClinicalNotes'

interface NoteEditorProps {
  initial?: ClinicalNote
  clientId: string
  appointmentId?: string
  isPending: boolean
  onSubmit: (data: { clientId: string; appointmentId?: string; title: string; content: string; isPrivate: boolean }) => void
  onClose: () => void
}

export function NoteEditor({ initial, clientId, appointmentId, isPending, onSubmit, onClose }: NoteEditorProps) {
  const [title, setTitle] = useState(initial?.title ?? '')
  const [content, setContent] = useState(initial?.content ?? '')
  const [isPrivate, setIsPrivate] = useState(initial?.isPrivate ?? false)

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!title.trim() || !content.trim()) return
    onSubmit({
      clientId,
      appointmentId: appointmentId ?? initial?.appointmentId ?? undefined,
      title: title.trim(),
      content: content.trim(),
      isPrivate,
    })
  }

  return (
    <div className="fixed inset-0 z-50 flex items-end md:items-center justify-center">
      <div className="absolute inset-0 bg-black/30" onClick={onClose} />
      <div className="relative bg-white w-full md:max-w-lg md:rounded-xl rounded-t-xl shadow-xl p-5 max-h-[85vh] overflow-auto">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold text-gray-800">
            {initial ? 'Editar nota' : 'Nova nota clínica'}
          </h3>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600 text-xl">&times;</button>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Título</label>
            <input
              required
              maxLength={200}
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-brand-500 focus:ring-1 focus:ring-brand-500"
              placeholder="Ex: Avaliação inicial, Evolução, Prescrição..."
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Conteúdo</label>
            <textarea
              required
              value={content}
              onChange={(e) => setContent(e.target.value)}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-brand-500 focus:ring-1 focus:ring-brand-500 resize-none"
              rows={8}
              placeholder="Descreva as observações clínicas..."
            />
          </div>

          <div className="flex items-center gap-3">
            <button
              type="button"
              onClick={() => setIsPrivate(!isPrivate)}
              className={`flex items-center gap-2 px-3 py-2 rounded-lg border text-sm transition-colors ${
                isPrivate
                  ? 'border-amber-300 bg-amber-50 text-amber-700'
                  : 'border-gray-300 bg-white text-gray-600'
              }`}
            >
              {isPrivate ? <Lock className="w-4 h-4" /> : <Globe className="w-4 h-4" />}
              {isPrivate ? 'Privada (só eu vejo)' : 'Visível para equipe'}
            </button>
          </div>

          <div className="flex gap-2 pt-2">
            <button
              type="button"
              onClick={onClose}
              className="flex-1 rounded-lg border border-gray-300 py-2 text-sm text-gray-600 hover:bg-gray-50"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={isPending || !title.trim() || !content.trim()}
              className="flex-1 rounded-lg bg-brand-600 py-2 text-sm text-white font-medium hover:bg-brand-700 disabled:opacity-50"
            >
              {isPending ? 'Salvando...' : initial ? 'Salvar' : 'Adicionar'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
