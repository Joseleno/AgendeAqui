import { describe, it, expect, beforeAll, afterAll, afterEach, beforeEach, vi } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { setupServer } from 'msw/node'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter, Routes, Route } from 'react-router-dom'
import { handlers } from './handlers'
import { AgendaPage } from '../pages/agenda/AgendaPage'
import { authStore } from '../store/auth-store'
import { ToastContainer } from '../components/ui/Toast'

// Mock SignalR to avoid URL resolution errors in jsdom
vi.mock('../lib/signalr', () => ({
  getConnection: () => ({
    on: vi.fn(),
    off: vi.fn(),
    start: vi.fn().mockResolvedValue(undefined),
    stop: vi.fn().mockResolvedValue(undefined),
    state: 'Disconnected',
  }),
  startConnection: vi.fn().mockResolvedValue(undefined),
  stopConnection: vi.fn().mockResolvedValue(undefined),
}))

const server = setupServer(...handlers)

beforeAll(() => server.listen({ onUnhandledRequest: 'bypass' }))
afterEach(() => server.resetHandlers())
afterAll(() => server.close())

function renderAgenda() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={['/agenda']}>
        <Routes>
          <Route path="/agenda" element={<AgendaPage />} />
        </Routes>
        <ToastContainer />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('AgendaPage', () => {
  beforeEach(() => {
    authStore.setTokens('fake-jwt-token', 'fake-refresh-token')
    authStore.setTenantId('tenant-1')
  })

  it('renders day view by default with appointments', async () => {
    renderAgenda()

    await waitFor(() => {
      expect(screen.getByText('Maria Santos')).toBeInTheDocument()
      expect(screen.getByText('Joao Lima')).toBeInTheDocument()
    })
  })

  it('shows view mode switcher', () => {
    renderAgenda()

    expect(screen.getByText('Dia')).toBeInTheDocument()
    expect(screen.getByText('Semana')).toBeInTheDocument()
    expect(screen.getByText('Mes')).toBeInTheDocument()
  })

  it('can switch to week view', async () => {
    const user = userEvent.setup()
    renderAgenda()

    await user.click(screen.getByText('Semana'))

    await waitFor(() => {
      expect(screen.getByText('Maria Santos')).toBeInTheDocument()
    })
  })

  it('can switch to month view', async () => {
    const user = userEvent.setup()
    renderAgenda()

    await user.click(screen.getByText('Mes'))

    const today = new Date().getDate()
    await waitFor(() => {
      expect(screen.getByText(String(today))).toBeInTheDocument()
    })
  })

  it('shows new appointment button', () => {
    renderAgenda()

    expect(screen.getByText('+ Novo')).toBeInTheDocument()
  })

  it('shows today button', () => {
    renderAgenda()

    expect(screen.getByText('Hoje')).toBeInTheDocument()
  })
})
