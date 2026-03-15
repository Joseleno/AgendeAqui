import { useState } from 'react'
import { Plus, FileText } from 'lucide-react'
import { useClinicalNotes, useCreateClinicalNote, useUpdateClinicalNote, useDeleteClinicalNote } from '../../hooks/useClinicalNotes'
import type { ClinicalNote } from '../../hooks/useClinicalNotes'
import { NoteCard } from './NoteCard'
import { NoteEditor } from './NoteEditor'
import { NoteDetail } from './NoteDetail'
import { EmptyState } from '../ui/EmptyState'
import { SkeletonText } from '../ui/Skeleton'
import { ConfirmDialog } from '../ui/ConfirmDialog'
import { PagedList } from '../ui/PagedList'
import { useAuthState } from '../../hooks/useAuth'

interface NotesTabProps {
  clientId: string
}

export function NotesTab({ clientId }: NotesTabProps) {
  const [page, setPage] = useState(1)
  const [showEditor, setShowEditor] = useState(false)
  const [editNote, setEditNote] = useState<ClinicalNote | null>(null)
  const [viewNote, setViewNote] = useState<ClinicalNote | null>(null)
  const [deleteNote, setDeleteNote] = useState<ClinicalNote | null>(null)

  const { professionalId } = useAuthState()
  const { data, isLoading } = useClinicalNotes(clientId, page)
  const createMutation = useCreateClinicalNote()
  const updateMutation = useUpdateClinicalNote()
  const deleteMutation = useDeleteClinicalNote()

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="text-sm font-semibold text-gray-700">Prontuário</h3>
        <button
          onClick={() => setShowEditor(true)}
          className="flex items-center gap-1 rounded-lg bg-brand-600 px-3 py-1.5 text-xs font-semibold text-white hover:bg-brand-700 transition-colors"
        >
          <Plus className="w-3.5 h-3.5" />
          Nova nota
        </button>
      </div>

      {isLoading ? (
        <div className="space-y-3">
          {[1, 2, 3].map((i) => (
            <div key={i} className="bg-white rounded-xl border border-gray-200 p-4">
              <SkeletonText lines={2} />
            </div>
          ))}
        </div>
      ) : !data || data.items.length === 0 ? (
        <EmptyState
          icon={FileText}
          title="Nenhuma nota clínica"
          subtitle="Adicione a primeira nota para este paciente"
          action={{ label: 'Adicionar nota', onClick: () => setShowEditor(true) }}
        />
      ) : (
        <>
          <div className="space-y-3">
            {data.items.map((note) => (
              <NoteCard
                key={note.id}
                note={note}
                isAuthor={note.professionalId === professionalId}
                onEdit={(n) => setEditNote(n)}
                onDelete={(n) => setDeleteNote(n)}
                onClick={(n) => setViewNote(n)}
              />
            ))}
          </div>
          <PagedList page={page} totalPages={data.totalPages} onPageChange={setPage} />
        </>
      )}

      {showEditor && (
        <NoteEditor
          clientId={clientId}
          isPending={createMutation.isPending}
          onSubmit={(data) => createMutation.mutate(data, { onSuccess: () => setShowEditor(false) })}
          onClose={() => setShowEditor(false)}
        />
      )}

      {editNote && (
        <NoteEditor
          initial={editNote}
          clientId={clientId}
          isPending={updateMutation.isPending}
          onSubmit={(data) => updateMutation.mutate({ id: editNote.id, title: data.title, content: data.content, isPrivate: data.isPrivate }, { onSuccess: () => setEditNote(null) })}
          onClose={() => setEditNote(null)}
        />
      )}

      {viewNote && (
        <NoteDetail note={viewNote} onClose={() => setViewNote(null)} />
      )}

      {deleteNote && (
        <ConfirmDialog
          title="Excluir nota"
          message={`Tem certeza que deseja excluir a nota "${deleteNote.title}"?`}
          confirmLabel="Excluir"
          isPending={deleteMutation.isPending}
          onConfirm={() => deleteMutation.mutate(deleteNote.id, { onSuccess: () => setDeleteNote(null) })}
          onCancel={() => setDeleteNote(null)}
        />
      )}
    </div>
  )
}
