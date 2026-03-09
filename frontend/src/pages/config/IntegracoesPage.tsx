import { ApiKeyList } from '../../components/config/ApiKeyList'
import { WebhookList } from '../../components/config/WebhookList'

export function IntegracoesPage() {
  return (
    <div className="space-y-6">
      <div className="bg-white rounded-xl border border-gray-200 p-5">
        <ApiKeyList />
      </div>
      <div className="bg-white rounded-xl border border-gray-200 p-5">
        <WebhookList />
      </div>
    </div>
  )
}
