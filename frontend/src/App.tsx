import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { AppLayout } from './components/layout/AppLayout'
import { ToastContainer } from './components/ui/Toast'
import { LoginPage } from './pages/login/LoginPage'
import { HomePage } from './pages/home/HomePage'
import { AgendaPage } from './pages/agenda/AgendaPage'
import { GestaoPage } from './pages/GestaoPage'
import { RelatoriosPage } from './pages/RelatoriosPage'
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
            <Route path="gestao" element={<GestaoPage />} />
            <Route path="relatorios" element={<RelatoriosPage />} />
            <Route path="config" element={<ConfigPage />} />
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
        <ToastContainer />
      </BrowserRouter>
    </QueryClientProvider>
  )
}
