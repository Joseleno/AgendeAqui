# Frontend Clinic Profiles Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement frontend pages for all Phase 1-4 backend endpoints: role-aware navigation, admin dashboard with real API data, professional dashboard (calendar, stats, patients), patient portal (register, search, book, history), and admin reports (ranking, conflicts, clinic calendar, CSV export).

**Architecture:** Role-based routing using JWT claims (`role`, `professional_id`, `client_id`) decoded from `authStore`. Three layout variants share the same `AppLayout` but show different sidebar items per role. New hooks call Phase 1-4 API endpoints. Pages follow existing patterns (TanStack Query + Tailwind + Lucide + Recharts).

**Tech Stack:** React 19, TanStack Query v5, React Router v7, Tailwind CSS v4 (brand teal), Lucide React, Recharts, date-fns, Zustand-style auth store.

---

## File Structure

### Auth / Role Infrastructure
- **Modify:** `frontend/src/store/auth-store.ts` — decode JWT to extract `role`, `professional_id`, `client_id`
- **Modify:** `frontend/src/hooks/useAuth.ts` — expose `role`, `professionalId`, `clientId`, `isAdmin`, `isProfessional`, `isClient`
- **Modify:** `frontend/src/components/layout/Sidebar.tsx` — role-filtered nav items
- **Modify:** `frontend/src/components/layout/BottomNav.tsx` — role-filtered nav items
- **Create:** `frontend/src/components/layout/RoleGuard.tsx` — redirects if role doesn't match

### New Hooks (API integration)
- **Create:** `frontend/src/hooks/useDashboard.ts` — `useDashboardOverview()`, `useProfessionalRanking()`, `useScheduleConflicts()`, `useClinicCalendar()`, `useExportCsv()`
- **Create:** `frontend/src/hooks/useAbsences.ts` — `useAbsences()`, `useCreateAbsence()`, `useDeleteAbsence()`
- **Create:** `frontend/src/hooks/useMyCalendar.ts` — `useMyCalendar()`, `useMyFilledSlots()`, `useMyStats()`
- **Create:** `frontend/src/hooks/useMyPatients.ts` — `useMyPatients()`
- **Create:** `frontend/src/hooks/usePatientPortal.ts` — `useRegisterPatient()`, `useSearchProfessionals()`, `useAvailableProfessionals()`, `useMyAppointments()`
- **Modify:** `frontend/src/hooks/useReports.ts` — add `professionalId` filter to attendance report
- **Modify:** `frontend/src/lib/api-client.ts` — add `getText()` method for CSV download

### Admin Pages (existing + new)
- **Modify:** `frontend/src/pages/home/HomePage.tsx` — use `useDashboardOverview()` API instead of counting client-side
- **Modify:** `frontend/src/pages/home/MetricCards.tsx` — accept dashboard overview response with 6 metrics
- **Create:** `frontend/src/pages/relatorios/RankingPage.tsx` — professional ranking table
- **Create:** `frontend/src/pages/relatorios/ConflitosPage.tsx` — schedule conflicts list
- **Create:** `frontend/src/pages/relatorios/ExportarPage.tsx` — CSV export form
- **Create:** `frontend/src/pages/agenda/ClinicCalendarPage.tsx` — all-professionals daily calendar (admin)
- **Modify:** `frontend/src/pages/RelatoriosPage.tsx` — add Ranking, Conflitos, Exportar tabs
- **Modify:** `frontend/src/pages/relatorios/AtendimentosPage.tsx` — add professionalId filter dropdown
- **Create:** `frontend/src/pages/gestao/horarios/AusenciasPage.tsx` — absences CRUD

### Professional Pages
- **Create:** `frontend/src/pages/profissional/MeuCalendarioPage.tsx` — personal calendar with absences
- **Create:** `frontend/src/pages/profissional/MinhasStatsPage.tsx` — personal stats
- **Create:** `frontend/src/pages/profissional/MeusPacientesPage.tsx` — patients list

### Patient Pages
- **Create:** `frontend/src/pages/paciente/RegisterPage.tsx` — self-registration form
- **Create:** `frontend/src/pages/paciente/BuscarProfissionalPage.tsx` — search professionals
- **Create:** `frontend/src/pages/paciente/AgendarPage.tsx` — book appointment flow
- **Create:** `frontend/src/pages/paciente/MeusAgendamentosPage.tsx` — appointment history

### Router
- **Modify:** `frontend/src/App.tsx` — add all new routes with role guards

### Charts
- **Create:** `frontend/src/components/charts/RankingChart.tsx` — horizontal bar chart for professional ranking

---

## Task 1: Auth Role Infrastructure

**Files:**
- Modify: `frontend/src/store/auth-store.ts`
- Modify: `frontend/src/hooks/useAuth.ts`
- Create: `frontend/src/components/layout/RoleGuard.tsx`

- [ ] **Step 1: Update auth-store to decode JWT claims**

Add `role`, `professionalId`, `clientId` to `AuthState`. On `setTokens()`, decode JWT payload to extract `role`, `professional_id`, `client_id` claims.

```typescript
interface AuthState {
  accessToken: string | null
  refreshToken: string | null
  tenantId: string | null
  role: string | null
  professionalId: string | null
  clientId: string | null
}
```

Add `decodeToken()` helper that parses JWT payload and extracts claims. Call it in `setTokens()`.

- [ ] **Step 2: Update useAuth hook to expose role info**

Add computed properties: `isAdmin`, `isProfessional`, `isClient`, `professionalId`, `clientId`, `role`.

- [ ] **Step 3: Create RoleGuard component**

```tsx
// Wraps <Outlet /> — redirects to "/" if user role doesn't match
export function RoleGuard({ roles }: { roles: string[] }) {
  const { role } = useAuthState()
  if (!role || !roles.includes(role)) return <Navigate to="/" replace />
  return <Outlet />
}
```

- [ ] **Step 4: Run frontend build to verify**

```bash
cd frontend && npm run build
```

- [ ] **Step 5: Commit**

```
feat(frontend): adicionar role infrastructure no auth store
```

---

## Task 2: Role-Aware Navigation

**Files:**
- Modify: `frontend/src/components/layout/Sidebar.tsx`
- Modify: `frontend/src/components/layout/BottomNav.tsx`

- [ ] **Step 1: Update Sidebar with role-filtered items**

Nav items per role:
- **Admin:** Dashboard, Agenda, Gestao, Relatorios, Config (current items)
- **Professional:** Meu Calendario, Meus Pacientes, Minhas Stats, Ausencias
- **Client:** Buscar Profissional, Agendar, Meus Agendamentos

Use `useAuthState()` to get `role` and filter `navItems` array.

Add new Lucide icons: `Stethoscope`, `UserSearch`, `Calendar`, `ClipboardList`.

- [ ] **Step 2: Update BottomNav with same role filtering**

Same logic, same items.

- [ ] **Step 3: Run build**

- [ ] **Step 4: Commit**

```
feat(frontend): navegacao role-aware sidebar e bottom nav
```

---

## Task 3: API Client + New Hooks

**Files:**
- Modify: `frontend/src/lib/api-client.ts`
- Create: `frontend/src/hooks/useDashboard.ts`
- Create: `frontend/src/hooks/useAbsences.ts`
- Create: `frontend/src/hooks/useMyCalendar.ts`
- Create: `frontend/src/hooks/useMyPatients.ts`
- Create: `frontend/src/hooks/usePatientPortal.ts`
- Modify: `frontend/src/hooks/useReports.ts`

- [ ] **Step 1: Add getText to api-client**

```typescript
getText: (path: string) => request<string>(path, {}, 'text'),
```

Modify `request()` to accept optional `responseType` parameter. When `'text'`, use `response.text()` instead of `response.json()`.

- [ ] **Step 2: Create useDashboard hook**

Interfaces: `DashboardOverview`, `ProfessionalRanking`, `ScheduleConflict`, `ClinicCalendar`, `ProfessionalDay`, `ClinicAppointment`.

Hooks: `useDashboardOverview()`, `useProfessionalRanking(from, to)`, `useScheduleConflicts(date)`, `useClinicCalendar(date)`, `useExportCsv()` (mutation that downloads file).

- [ ] **Step 3: Create useAbsences hook**

Interface: `Absence` (id, professionalId, date, startTime, endTime, reason, isFullDay).
Hooks: `useAbsences(professionalId, from, to)`, `useCreateAbsence()`, `useDeleteAbsence()`.

- [ ] **Step 4: Create useMyCalendar hook**

Interfaces: `CalendarDay`, `CalendarAppointment`, `CalendarAbsence`, `FilledSlots`, `ProfessionalStats`.
Hooks: `useMyCalendar(dateFrom, dateTo)`, `useMyFilledSlots(date)`, `useMyStats(from, to)`.

- [ ] **Step 5: Create useMyPatients hook**

Interface: `PatientSummary` (clientId, name, email, phone, lastAppointment, totalAppointments).
Hook: `useMyPatients(page, pageSize, search)`.

- [ ] **Step 6: Create usePatientPortal hook**

Interfaces: `ProfessionalSummary`, `ProfessionalAvailability`, `MyAppointment`.
Hooks: `useRegisterPatient()` (mutation), `useSearchProfessionals(name, specialty, serviceId)`, `useAvailableProfessionals(serviceId, date)`, `useMyAppointments(page, status)`.

- [ ] **Step 7: Update useReports — add professionalId param**

```typescript
export function useAttendanceReport(from: string, to: string, professionalId?: string) {
  const params = new URLSearchParams({ from, to })
  if (professionalId) params.set('professionalId', professionalId)
  return useQuery({
    queryKey: ['reports', 'attendance', from, to, professionalId],
    queryFn: () => api.get<AttendanceReport>(`/reports/attendance?${params}`),
    enabled: !!from && !!to,
  })
}
```

- [ ] **Step 8: Run build**

- [ ] **Step 9: Commit**

```
feat(frontend): hooks para todos os endpoints das fases 1-4
```

---

## Task 4: Admin Dashboard (HomePage) — Real API Data

**Files:**
- Modify: `frontend/src/pages/home/HomePage.tsx`
- Modify: `frontend/src/pages/home/MetricCards.tsx`
- Modify: `frontend/src/pages/home/AlertsList.tsx`

- [ ] **Step 1: Update HomePage to use useDashboardOverview**

Replace the current client-side counting approach. Use `useDashboardOverview()` for the 6 metrics. Keep `useAppointments` for the upcoming list and alerts (they need individual appointment data).

- [ ] **Step 2: Update MetricCards to show 6 metrics**

Cards: Agendamentos Hoje, Agendamentos Semana, Profissionais Ativos, Pacientes Registrados, Cancelados Hoje, Faltas Hoje.

Use Lucide icons: `CalendarCheck`, `CalendarDays`, `Stethoscope`, `Users`, `XCircle`, `AlertTriangle`.

Accept `DashboardOverview` interface instead of raw appointments array.

- [ ] **Step 3: Run build**

- [ ] **Step 4: Commit**

```
feat(frontend): dashboard admin com dados reais da API
```

---

## Task 5: Admin Reports — Ranking, Conflicts, Export

**Files:**
- Modify: `frontend/src/pages/RelatoriosPage.tsx`
- Modify: `frontend/src/pages/relatorios/AtendimentosPage.tsx`
- Create: `frontend/src/pages/relatorios/RankingPage.tsx`
- Create: `frontend/src/pages/relatorios/ConflitosPage.tsx`
- Create: `frontend/src/pages/relatorios/ExportarPage.tsx`
- Create: `frontend/src/components/charts/RankingChart.tsx`

- [ ] **Step 1: Add tabs to RelatoriosPage**

Add: Ranking, Conflitos, Exportar tabs. Routes: `/relatorios/ranking`, `/relatorios/conflitos`, `/relatorios/exportar`.

- [ ] **Step 2: Update AtendimentosPage with professionalId filter**

Add a professional dropdown select at the top. Use `useProfessionals()` to populate options. Pass selected `professionalId` to `useAttendanceReport()`.

- [ ] **Step 3: Create RankingPage**

DateRangePicker + table with columns: Profissional, Especialidade, Total, Concluidos, Cancelados, Faltas, Taxa Conclusao, Taxa Falta. Sorted by completed DESC. Use `useProfessionalRanking(from, to)`. Include `RankingChart` horizontal bar chart.

- [ ] **Step 4: Create RankingChart**

Recharts horizontal BarChart showing completed/cancelled/noShow stacked per professional (same pattern as AttendanceChart).

- [ ] **Step 5: Create ConflitosPage**

Date picker (single date). Table: Profissional, Horario 1 (inicio-fim), Cliente 1, Horario 2 (inicio-fim), Cliente 2. Red highlight. Use `useScheduleConflicts(date)`. Empty state: green checkmark "Nenhum conflito detectado".

- [ ] **Step 6: Create ExportarPage**

DateRangePicker + optional professional dropdown + "Exportar CSV" button. On click, call `useExportCsv()` mutation that downloads the file via `api.getText()` and triggers browser download using Blob + URL.createObjectURL.

- [ ] **Step 7: Run build**

- [ ] **Step 8: Commit**

```
feat(frontend): relatorios admin (ranking, conflitos, exportar CSV)
```

---

## Task 6: Admin — Clinic Calendar + Absences

**Files:**
- Create: `frontend/src/pages/agenda/ClinicCalendarPage.tsx`
- Create: `frontend/src/pages/gestao/horarios/AusenciasPage.tsx`
- Modify: `frontend/src/pages/GestaoPage.tsx`
- Modify: `frontend/src/App.tsx` (partial — clinic calendar route)

- [ ] **Step 1: Create ClinicCalendarPage**

Date picker. Groups appointments by professional. Each professional block shows name + list of appointments with time, client, service, status badge. Use `useClinicCalendar(date)`.

Visual: cards per professional, timeline-style list of appointments inside each card.

- [ ] **Step 2: Create AusenciasPage**

Table: Profissional, Data, Horario (ou "Dia inteiro"), Motivo, Acoes (excluir). Filter by professional dropdown + date range. Create button opens FormModal with fields: professionalId select, date, startTime (optional), endTime (optional), reason. Use `useAbsences()`, `useCreateAbsence()`, `useDeleteAbsence()`.

- [ ] **Step 3: Add Ausencias tab to GestaoPage**

Add `{ to: '/gestao/ausencias', label: 'Ausencias' }` to tabs array.

- [ ] **Step 4: Run build**

- [ ] **Step 5: Commit**

```
feat(frontend): calendario clinica e gestao de ausencias
```

---

## Task 7: Professional Dashboard Pages

**Files:**
- Create: `frontend/src/pages/profissional/MeuCalendarioPage.tsx`
- Create: `frontend/src/pages/profissional/MinhasStatsPage.tsx`
- Create: `frontend/src/pages/profissional/MeusPacientesPage.tsx`

- [ ] **Step 1: Create MeuCalendarioPage**

DateRangePicker (default: current week, Mon-Sun). Show appointments + absences grouped by day (same as `CalendarResponse`). Each day shows a list of appointment cards with time, client, service, status. Absences shown as gray blocked bars. Use `useMyCalendar(dateFrom, dateTo)`.

Include filled slots summary at top using `useMyFilledSlots(selectedDate)`: "X/Y slots preenchidos (Z%)" progress bar.

- [ ] **Step 2: Create MinhasStatsPage**

DateRangePicker (default: last 30 days). 5 metric cards: Total, Concluidos, Cancelados, Faltas, Receita (R$). Use `useMyStats(from, to)`. Include a simple progress bar for completion rate.

- [ ] **Step 3: Create MeusPacientesPage**

SearchInput + paginated table: Nome, Email, Telefone, Ultimo Atendimento, Total Atendimentos. Use `useMyPatients(page, pageSize, search)`.

- [ ] **Step 4: Run build**

- [ ] **Step 5: Commit**

```
feat(frontend): dashboard do profissional (calendario, stats, pacientes)
```

---

## Task 8: Patient Portal Pages

**Files:**
- Create: `frontend/src/pages/paciente/RegisterPage.tsx`
- Create: `frontend/src/pages/paciente/BuscarProfissionalPage.tsx`
- Create: `frontend/src/pages/paciente/AgendarPage.tsx`
- Create: `frontend/src/pages/paciente/MeusAgendamentosPage.tsx`

- [ ] **Step 1: Create RegisterPage**

Public page (no auth). Similar to LoginPage layout (dark gradient). Form: nome, email, telefone, senha, confirmar senha. Requires `X-Tenant-Id` header (from URL param or input). On success, auto-login and redirect to `/meus-agendamentos`. Use `useRegisterPatient()`.

- [ ] **Step 2: Create BuscarProfissionalPage**

Search filters: nome (text), especialidade (text), servico (select from `useServices()`). Results as cards: name, specialty, services list. Click card → navigate to `/agendar?professionalId=X`.

Use `useSearchProfessionals()`.

- [ ] **Step 3: Create AgendarPage**

Multi-step flow:
1. Select professional (pre-filled if from search) + select service
2. Select date → show available slots (use `useAvailability()`)
3. Select slot → confirm → create appointment

Use `useCreateAppointment()`. On success, redirect to `/meus-agendamentos`.

- [ ] **Step 4: Create MeusAgendamentosPage**

Paginated list with status filter tabs (Todos, Agendados, Concluidos, Cancelados). Cards: date, time, professional name, service, status badge. Actions: Cancel, Reschedule (for future scheduled appointments only). Use `useMyAppointments()`.

- [ ] **Step 5: Run build**

- [ ] **Step 6: Commit**

```
feat(frontend): portal do paciente (registro, busca, agendamento, historico)
```

---

## Task 9: Router + Route Guards

**Files:**
- Modify: `frontend/src/App.tsx`

- [ ] **Step 1: Add all new routes**

```tsx
// Public
<Route path="/register" element={<RegisterPage />} />

// Admin routes (existing + new)
<Route element={<RoleGuard roles={['Admin']} />}>
  <Route path="relatorios/ranking" element={<RankingPage />} />
  <Route path="relatorios/conflitos" element={<ConflitosPage />} />
  <Route path="relatorios/exportar" element={<ExportarPage />} />
  <Route path="agenda/clinica" element={<ClinicCalendarPage />} />
  <Route path="gestao/ausencias" element={<AusenciasPage />} />
</Route>

// Professional routes
<Route element={<RoleGuard roles={['Professional']} />}>
  <Route path="meu-calendario" element={<MeuCalendarioPage />} />
  <Route path="minhas-stats" element={<MinhasStatsPage />} />
  <Route path="meus-pacientes" element={<MeusPacientesPage />} />
</Route>

// Client routes
<Route element={<RoleGuard roles={['Client']} />}>
  <Route path="buscar" element={<BuscarProfissionalPage />} />
  <Route path="agendar" element={<AgendarPage />} />
  <Route path="meus-agendamentos" element={<MeusAgendamentosPage />} />
</Route>
```

Admin also has access to Professional routes (use `roles={['Admin', 'Professional']}`).

- [ ] **Step 2: Run full build + lint**

```bash
cd frontend && npm run build && npm run lint
```

- [ ] **Step 3: Commit**

```
feat(frontend): rotas com guards por role (admin, profissional, paciente)
```

---

## Task 10: Final Integration Test + Build Verification

- [ ] **Step 1: Run frontend build**

```bash
cd frontend && npm run build
```

- [ ] **Step 2: Run frontend tests**

```bash
cd frontend && npm test
```

- [ ] **Step 3: Run backend build + tests**

```bash
dotnet build AgendeAqui.slnx && dotnet test AgendeAqui.slnx
```

- [ ] **Step 4: Final commit if needed**

```
chore: ajustes finais frontend clinic profiles
```

---

## Summary

| Task | Description | Files |
|------|-------------|-------|
| 1 | Auth role infrastructure | 3 |
| 2 | Role-aware navigation | 2 |
| 3 | API hooks (all endpoints) | 7 |
| 4 | Admin dashboard real data | 3 |
| 5 | Admin reports (ranking, conflicts, CSV) | 6 |
| 6 | Clinic calendar + absences | 4 |
| 7 | Professional dashboard | 3 |
| 8 | Patient portal | 4 |
| 9 | Router + guards | 1 |
| 10 | Final verification | 0 |

**Total: ~33 files (14 new + 19 modified)**
