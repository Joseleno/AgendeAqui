import { useSearchParams, useNavigate } from 'react-router-dom'
import { Video, Clock, User, ArrowLeft } from 'lucide-react'

function isValidMeetingUrl(url: string): boolean {
  try {
    const parsed = new URL(url)
    return (parsed.protocol === 'https:' || parsed.protocol === 'http:') &&
      parsed.hostname.endsWith('meet.jit.si')
  } catch {
    return false
  }
}

export function TeleconsultaPage() {
  const [params] = useSearchParams()
  const navigate = useNavigate()
  const rawUrl = params.get('url')
  const meetingUrl = rawUrl && isValidMeetingUrl(rawUrl) ? rawUrl : null
  const clientName = params.get('client') ?? 'Paciente'
  const serviceName = params.get('service') ?? 'Consulta'

  if (!meetingUrl) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[60vh] text-center">
        <Video className="w-12 h-12 text-gray-300 mb-4" />
        <h2 className="text-lg font-semibold text-gray-800 mb-1">Link não encontrado</h2>
        <p className="text-sm text-gray-500 mb-4">O link da teleconsulta não foi fornecido.</p>
        <button onClick={() => navigate(-1)} className="text-sm text-brand-600 hover:text-brand-800 font-medium">
          Voltar
        </button>
      </div>
    )
  }

  return (
    <div className="space-y-4 page-enter">
      <div className="flex items-center justify-between">
        <button
          onClick={() => navigate(-1)}
          className="flex items-center gap-1 text-sm text-brand-600 hover:text-brand-800"
        >
          <ArrowLeft className="w-4 h-4" />
          Voltar
        </button>
        <div className="flex items-center gap-3 text-sm text-gray-600">
          <div className="flex items-center gap-1">
            <User className="w-4 h-4 text-gray-400" />
            {clientName}
          </div>
          <div className="flex items-center gap-1">
            <Clock className="w-4 h-4 text-gray-400" />
            {serviceName}
          </div>
        </div>
      </div>

      <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
        <div className="bg-indigo-50 px-4 py-2 flex items-center gap-2">
          <Video className="w-4 h-4 text-indigo-600" />
          <span className="text-sm font-medium text-indigo-700">Teleconsulta em andamento</span>
        </div>
        <iframe
          src={meetingUrl}
          className="w-full aspect-video min-h-[500px]"
          allow="camera; microphone; fullscreen; display-capture"
          title="Teleconsulta"
        />
      </div>
    </div>
  )
}
