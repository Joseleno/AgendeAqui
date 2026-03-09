import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { AppLayout } from './components/layout/AppLayout'
import { ToastContainer } from './components/ui/Toast'
import { LoginPage } from './pages/login/LoginPage'
import { HomePage } from './pages/home/HomePage'
import { AgendaPage } from './pages/agenda/AgendaPage'
import { GestaoPage } from './pages/GestaoPage'
import { ClientesPage } from './pages/gestao/clientes/ClientesPage'
import { ClienteDetailPage } from './pages/gestao/clientes/ClienteDetailPage'
import { ProfissionaisPage } from './pages/gestao/profissionais/ProfissionaisPage'
import { ServicosPage } from './pages/gestao/servicos/ServicosPage'
import { HorariosPage } from './pages/gestao/horarios/HorariosPage'
import { RelatoriosPage } from './pages/RelatoriosPage'
import { AtendimentosPage } from './pages/relatorios/AtendimentosPage'
import { FaturamentoPage } from './pages/relatorios/FaturamentoPage'
import { ConfigPage } from './pages/ConfigPage'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      retry: 1,
    },
  },
})

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route element={<AppLayout />}>
            <Route index element={<HomePage />} />
            <Route path="agenda" element={<AgendaPage />} />
            <Route path="gestao" element={<GestaoPage />}>
              <Route index element={<Navigate to="clientes" replace />} />
              <Route path="clientes" element={<ClientesPage />} />
              <Route path="clientes/:id" element={<ClienteDetailPage />} />
              <Route path="profissionais" element={<ProfissionaisPage />} />
              <Route path="servicos" element={<ServicosPage />} />
              <Route path="horarios" element={<HorariosPage />} />
            </Route>
            <Route path="relatorios" element={<RelatoriosPage />}>
              <Route index element={<Navigate to="atendimentos" replace />} />
              <Route path="atendimentos" element={<AtendimentosPage />} />
              <Route path="faturamento" element={<FaturamentoPage />} />
            </Route>
            <Route path="config" element={<ConfigPage />} />
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
        <ToastContainer />
      </BrowserRouter>
    </QueryClientProvider>
  )
}
