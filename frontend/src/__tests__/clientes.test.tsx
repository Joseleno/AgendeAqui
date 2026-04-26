import { describe, it, expect, beforeAll, afterAll, afterEach, beforeEach, vi } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { setupServer } from 'msw/node'
import { http, HttpResponse } from 'msw'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter } from 'react-router-dom'
import { handlers } from './handlers'
import { ClientesPage } from '../pages/gestao/clientes/ClientesPage'
import { authStore } from '../store/auth-store'
import { ToastContainer } from '../components/ui/Toast'

vi.mock('../lib/signalr', () => ({
  getConnection: () => ({ on: vi.fn(), off: vi.fn(), start: vi.fn().mockResolvedValue(undefined), stop: vi.fn().mockResolvedValue(undefined), state: 'Disconnected' }),
  startConnection: vi.fn().mockResolvedValue(undefined),
  stopConnection: vi.fn().mockResolvedValue(undefined),
}))

const server = setupServer(...handlers)

beforeAll(() => server.listen({ onUnhandledRequest: 'bypass' }))
afterEach(() => server.resetHandlers())
afterAll(() => server.close())

function renderClientes() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <ClientesPage />
        <ToastContainer />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

function fakeJwt(payload: Record<string, string>) {
  const header = btoa(JSON.stringify({ alg: 'HS256' }))
  const body = btoa(JSON.stringify(payload))
  return `${header}.${body}.sig`
}

describe('ClientesPage', () => {
  beforeEach(() => {
    const token = fakeJwt({ tenant_id: 'tenant-1', role: 'Admin' })
    authStore.setTokens(token, 'fake-refresh-token')
  })

  it('renders page heading', () => {
    renderClientes()
    expect(screen.getByText('Clientes')).toBeInTheDocument()
  })

  it('shows novo button', () => {
    renderClientes()
    expect(screen.getByText('+ Novo')).toBeInTheDocument()
  })

  it('loads and displays clients from API', async () => {
    renderClientes()

    await waitFor(() => {
      expect(screen.getByText('Ana Costa')).toBeInTheDocument()
      expect(screen.getByText('Bruno Lima')).toBeInTheDocument()
      expect(screen.getByText('Carla Souza')).toBeInTheDocument()
    })
  })

  it('shows email column for listed clients', async () => {
    renderClientes()

    await waitFor(() => {
      expect(screen.getByText('ana@test.com')).toBeInTheDocument()
    })
  })

  it('filters clients by search text', async () => {
    const user = userEvent.setup()
    renderClientes()

    await waitFor(() => expect(screen.getByText('Ana Costa')).toBeInTheDocument())

    const searchInput = screen.getByPlaceholderText('Buscar cliente...')
    await user.type(searchInput, 'Bruno')

    await waitFor(() => {
      expect(screen.queryByText('Ana Costa')).not.toBeInTheDocument()
      expect(screen.getByText('Bruno Lima')).toBeInTheDocument()
    })
  })

  it('shows empty state when search yields no results', async () => {
    const user = userEvent.setup()
    renderClientes()

    await waitFor(() => expect(screen.getByText('Ana Costa')).toBeInTheDocument())

    const searchInput = screen.getByPlaceholderText('Buscar cliente...')
    await user.type(searchInput, 'xyznotfound')

    await waitFor(() => {
      expect(screen.getByText('Nenhum cliente encontrado')).toBeInTheDocument()
    })
  })

  it('opens create modal when + Novo is clicked', async () => {
    const user = userEvent.setup()
    renderClientes()

    await user.click(screen.getByText('+ Novo'))

    expect(screen.getByRole('dialog')).toBeInTheDocument()
  })

  it('shows empty state when API returns no clients', async () => {
    server.use(
      http.get('/api/v1/clients', () =>
        HttpResponse.json({ items: [], page: 1, pageSize: 10, totalCount: 0, totalPages: 0 }),
      ),
    )

    renderClientes()

    await waitFor(() => {
      expect(screen.getByText('Nenhum cliente encontrado')).toBeInTheDocument()
    })
  })
})
