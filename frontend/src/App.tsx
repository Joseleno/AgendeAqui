import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { AppLayout } from './components/layout/AppLayout'
import { RoleGuard } from './components/layout/RoleGuard'
import { HomeRedirect } from './components/layout/HomeRedirect'
import { ToastContainer } from './components/ui/Toast'
import { LoginPage } from './pages/login/LoginPage'
import { RegisterPage } from './pages/paciente/RegisterPage'
import { HomePage } from './pages/home/HomePage'
import { AgendaPage } from './pages/agenda/AgendaPage'
import { ClinicCalendarPage } from './pages/agenda/ClinicCalendarPage'
import { GestaoPage } from './pages/GestaoPage'
import { ClientesPage } from './pages/gestao/clientes/ClientesPage'
import { ClienteDetailPage } from './pages/gestao/clientes/ClienteDetailPage'
import { ProfissionaisPage } from './pages/gestao/profissionais/ProfissionaisPage'
import { ServicosPage } from './pages/gestao/servicos/ServicosPage'
import { HorariosPage } from './pages/gestao/horarios/HorariosPage'
import { AusenciasPage } from './pages/gestao/horarios/AusenciasPage'
import { RelatoriosPage } from './pages/RelatoriosPage'
import { AtendimentosPage } from './pages/relatorios/AtendimentosPage'
import { FaturamentoPage } from './pages/relatorios/FaturamentoPage'
import { RankingPage } from './pages/relatorios/RankingPage'
import { ConflitosPage } from './pages/relatorios/ConflitosPage'
import { ExportarPage } from './pages/relatorios/ExportarPage'
import { ConfigPage } from './pages/ConfigPage'
import { IntegracoesPage } from './pages/config/IntegracoesPage'
import { NotificacoesPage } from './pages/config/NotificacoesPage'
import { ContaPage } from './pages/config/ContaPage'
import { MeuCalendarioPage } from './pages/profissional/MeuCalendarioPage'
import { MinhasStatsPage } from './pages/profissional/MinhasStatsPage'
import { MeusPacientesPage } from './pages/profissional/MeusPacientesPage'
import { BuscarProfissionalPage } from './pages/paciente/BuscarProfissionalPage'
import { AgendarPage } from './pages/paciente/AgendarPage'
import { MeusAgendamentosPage } from './pages/paciente/MeusAgendamentosPage'

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
          {/* Public routes (with tenant slug) */}
          <Route path="/:slug/login" element={<LoginPage />} />
          <Route path="/:slug/register" element={<RegisterPage />} />
          {/* Legacy routes without slug */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />

          <Route element={<AppLayout />}>
            <Route index element={<HomeRedirect />} />

            {/* Admin routes */}
            <Route element={<RoleGuard roles={['Admin']} />}>
              <Route path="dashboard" element={<HomePage />} />
              <Route path="agenda" element={<AgendaPage />} />
              <Route path="agenda/clinica" element={<ClinicCalendarPage />} />
              <Route path="gestao" element={<GestaoPage />}>
                <Route index element={<Navigate to="clientes" replace />} />
                <Route path="clientes" element={<ClientesPage />} />
                <Route path="clientes/:id" element={<ClienteDetailPage />} />
                <Route path="profissionais" element={<ProfissionaisPage />} />
                <Route path="servicos" element={<ServicosPage />} />
                <Route path="horarios" element={<HorariosPage />} />
                <Route path="ausencias" element={<AusenciasPage />} />
              </Route>
              <Route path="relatorios" element={<RelatoriosPage />}>
                <Route index element={<Navigate to="atendimentos" replace />} />
                <Route path="atendimentos" element={<AtendimentosPage />} />
                <Route path="faturamento" element={<FaturamentoPage />} />
                <Route path="ranking" element={<RankingPage />} />
                <Route path="conflitos" element={<ConflitosPage />} />
                <Route path="exportar" element={<ExportarPage />} />
              </Route>
              <Route path="config" element={<ConfigPage />}>
                <Route index element={<Navigate to="integracoes" replace />} />
                <Route path="integracoes" element={<IntegracoesPage />} />
                <Route path="notificacoes" element={<NotificacoesPage />} />
                <Route path="conta" element={<ContaPage />} />
              </Route>
            </Route>

            {/* Professional routes */}
            <Route element={<RoleGuard roles={['Admin', 'Professional']} />}>
              <Route path="meu-calendario" element={<MeuCalendarioPage />} />
              <Route path="minhas-stats" element={<MinhasStatsPage />} />
              <Route path="meus-pacientes" element={<MeusPacientesPage />} />
            </Route>

            {/* Client routes */}
            <Route element={<RoleGuard roles={['Client']} />}>
              <Route path="buscar" element={<BuscarProfissionalPage />} />
              <Route path="agendar" element={<AgendarPage />} />
              <Route path="meus-agendamentos" element={<MeusAgendamentosPage />} />
            </Route>
          </Route>

          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
        <ToastContainer />
      </BrowserRouter>
    </QueryClientProvider>
  )
}
