# AgendeAqui Dashboard + Sync Bidirecional — Design Document

**Data**: 2026-03-09
**Status**: Aprovado
**Autor**: Claude Code + José

---

## 1. Visão do Produto

O AgendeAqui é uma plataforma de agendamento com dois produtos:

1. **AgendeAqui API** (produto core, vendido standalone) — SaaS para sistemas que já têm frontend (chatbots, ERPs, apps). Consomem via API Key + Webhooks.
2. **AgendeAqui Dashboard** (produto adicional) — Frontend React para tenants que querem gestão visual: agenda, relatórios, LGPD, integrações.

O frontend é apenas mais um cliente da API — nenhum endpoint exclusivo.

```
┌─────────────────────────────────────────────────────────┐
│                    AgendeAqui                            │
│                                                          │
│  ┌─────────────────────────────────────────────────┐    │
│  │              AgendeAqui API                      │    │
│  │         (produto core, vendido solo)             │    │
│  └──────────┬──────────────┬───────────────┬────────┘    │
│             │              │               │             │
│     ┌───────┴──────┐ ┌────┴─────┐  ┌──────┴──────┐     │
│     │  Dashboard   │ │ Chatbot  │  │  Sistema    │     │
│     │  React       │ │ WhatsApp │  │  Parceiro   │     │
│     └──────────────┘ └──────────┘  └─────────────┘     │
│                                                          │
│  Todos são clientes iguais da mesma API.                │
└─────────────────────────────────────────────────────────┘
```

---

## 2. Stack do Frontend

| Tecnologia | Papel |
|-----------|-------|
| **Vite** | Build tool |
| **React 19** | UI framework |
| **TypeScript** | Tipagem estática |
| **TailwindCSS 4** | Estilo utilitário, mobile-first |
| **React Router** | Navegação SPA |
| **TanStack Query** | Cache, fetching, sincronização com API |
| **@microsoft/signalr** | Real-time (notificações, agenda ao vivo) |
| **Recharts** | Gráficos para relatórios |
| **date-fns** | Manipulação de datas |
| **openapi-typescript** | Geração de tipos TS a partir da OpenAPI spec |

**Deploy**: SPA estática (Vercel / Cloudflare / S3)

---

## 3. Estrutura do Monorepo

```
AgendeAqui/
├── src/                          ← backend .NET (existente)
│   ├── AgendeAqui.Domain/
│   ├── AgendeAqui.Application/
│   ├── AgendeAqui.Infrastructure/
│   └── AgendeAqui.Api/
├── frontend/                     ← novo projeto React
│   ├── public/
│   ├── src/
│   │   ├── api/                  ← tipos gerados via OpenAPI
│   │   ├── components/
│   │   │   ├── ui/               ← botões, inputs, modals, toasts
│   │   │   ├── agenda/           ← calendário dia/semana/mês
│   │   │   └── layout/           ← sidebar, bottom nav, header
│   │   ├── pages/
│   │   │   ├── home/
│   │   │   ├── agenda/
│   │   │   ├── gestao/
│   │   │   ├── relatorios/
│   │   │   ├── config/
│   │   │   └── login/
│   │   ├── hooks/                ← useAuth, useSignalR, useTenant
│   │   ├── lib/                  ← api client, signalr connection
│   │   ├── store/                ← estado global (auth, tenant)
│   │   └── App.tsx
│   ├── package.json
│   ├── tsconfig.json
│   ├── vite.config.ts
│   └── tailwind.config.ts
├── tests/
├── docker-compose.yml
└── CLAUDE.md
```

---

## 4. Design Visual

| Aspecto | Decisão |
|---------|---------|
| Estilo | Clean/minimalista + mobile-first |
| Mobile (< 768px) | Bottom navigation bar (5 ícones) |
| Desktop (≥ 768px) | Sidebar colapsável |
| Cores de status | Amarelo=Agendado, Verde=Confirmado, Azul=Em atendimento, Cinza=Concluído, Vermelho=Cancelado, Escuro=No-show |
| Interação agenda | Tap/click (sem drag-and-drop) |
| Criação | Modals no desktop, bottom sheets no mobile |
| Ações destrutivas | Dialog de confirmação com digitação |
| Real-time | Toast notifications + auto-refresh via SignalR |

---

## 5. Páginas e Navegação

### Layout Responsivo

- **Mobile**: Bottom navigation bar com 5 ícones (Home, Agenda, Gestão, Relatórios, Config)
- **Desktop**: Sidebar lateral colapsável + conteúdo principal

### Mapa de Rotas

| Página | Rota | API consumida |
|--------|------|--------------|
| Home (resumo do dia) | `/` | `GET /appointments` |
| Agenda (dia/semana/mês) | `/agenda` | `GET /appointments` + `GET /availability` + SignalR |
| Clientes | `/gestao/clientes` | `GET/POST/PUT /clients` |
| Cliente detalhe + LGPD | `/gestao/clientes/:id` | `GET /clients/:id` + `DELETE/GET /clients/:id/data` |
| Profissionais | `/gestao/profissionais` | `GET/POST/PUT /professionals` |
| Serviços | `/gestao/servicos` | `GET/POST/PUT /services` |
| Horários | `/gestao/horarios` | `GET/PUT /schedules` |
| Relatório atendimentos | `/relatorios/atendimentos` | `GET /reports/attendance` |
| Relatório faturamento | `/relatorios/faturamento` | `GET /reports/revenue` |
| Integrações | `/config/integracoes` | `GET/POST/DELETE /api-keys` + webhooks |
| Notificações | `/config/notificacoes` | `GET /notifications` |
| Conta | `/config/conta` | `GET/PUT /tenants/:id` |
| Login | `/login` | `POST /auth/login` |

---

## 6. Componente de Agenda

### 3 Visualizações

- **Vista Dia** (padrão mobile): Timeline vertical com cards de agendamento por hora
- **Vista Semana** (padrão desktop): Grade 7 colunas com agendamentos posicionados
- **Vista Mês** (overview): Calendário com dots indicando quantidade de agendamentos

### Interações

- Criar: botão [+ Novo] → modal com seleção de cliente, serviço, profissional, data e horário (via GET /availability)
- Ver detalhes: tap no card → bottom sheet (mobile) ou side panel (desktop)
- Cancelar/Remarcar: dentro dos detalhes
- Filtrar por profissional: dropdown no topo
- Navegar: swipe (mobile) ou setas (desktop)

### Real-time

- SignalR atualiza calendário automaticamente quando outro usuário/chatbot/parceiro cria ou altera agendamento
- Toast notification no canto superior

---

## 7. Relatórios

### Home — Resumo do Dia
- 3 cards de métricas: total do dia, confirmados, faturamento previsto
- Lista de próximos agendamentos
- Alertas (sem confirmar, cancelamentos recentes)
- API: `GET /appointments?dateFrom=hoje&dateTo=hoje` (contagem no frontend)

### Atendimentos (`/relatorios/atendimentos`)
- Filtro por período (date range picker)
- Cards: total, completados, taxa de conclusão
- Barra horizontal: proporção completados/cancelados/no-show
- Tabela por profissional (da API: `breakdown[]`)
- API: `GET /reports/attendance?from=X&to=Y`

### Faturamento (`/relatorios/faturamento`)
- Filtro por período
- Card destaque: receita total
- Bar chart horizontal por serviço
- Tabela detalhada (da API: `byService[]`)
- API: `GET /reports/revenue?from=X&to=Y`

---

## 8. Gestão

Todas as telas de CRUD seguem o mesmo padrão: lista com busca + paginação, botão [+ Novo], tap para detalhe.

### Clientes
- Lista: nome, telefone, quantidade de agendamentos
- Detalhe: dados + histórico de agendamentos + ações LGPD (exportar/anonimizar)
- Anonimizar: dialog de confirmação com digitação do nome (ação irreversível)

### Profissionais
- Lista: nome, status (ativo/inativo), email
- Criar/Editar: nome, email, telefone

### Serviços
- Lista: nome, duração, preço, status
- Criar/Editar: nome, duração (minutos), preço (R$)

### Horários
- Vista por profissional: grade semanal (seg-dom)
- Mostra início/fim, pausas, ativo/folga
- Edição inline

---

## 9. Configurações

### Integrações
- API Keys: lista com nome, prefixo, status, expiração. Ações: criar, revogar
- Webhooks: lista com URL, eventos inscritos, status, última entrega. Ações: criar, editar, remover

### Notificações
- Lista cronológica de WhatsApp enviados: horário, status (enviado/falhou), destinatário, template

### Conta
- Dados do tenant: nome, slug, plano, data de criação
- Ações: editar dados, alterar plano

---

## 10. Adições à API

### Bloqueantes (necessárias para o dashboard)

| Adição | Tipo |
|--------|------|
| `POST /api/v1/auth/login` + refresh token | Endpoint novo |
| `POST/GET/DELETE /api/v1/api-keys` | Endpoints novos |
| Filtro `?clientId=` no ListAppointments | Extensão de query |
| Filtro `?dateFrom=&dateTo=` no ListNotifications | Extensão de query |

### Sync Bidirecional

| Adição | Tipo |
|--------|------|
| `externalId` no Appointment | Campo + migration + filtro `?externalId=` |
| `X-Idempotency-Key` middleware | Middleware novo (HybridCache, TTL 24h) |
| Webhook anti-eco (sourceApiKeyId) | Extensão do webhook handler |
| 409 Conflict mapping | Extensão do ExceptionHandling |

---

## 11. Sync Bidirecional — Detalhamento

### Fluxo

- **Parceiro → AgendeAqui**: API Key + REST + X-Idempotency-Key + externalId
- **AgendeAqui → Parceiro**: Webhooks HMAC (com anti-eco para não notificar quem criou)
- **Conflitos**: 409 Conflict + notification no dashboard

### ExternalId

Campo opcional no Appointment (string?, max 256). O parceiro vincula seu ID local ao agendamento do AgendeAqui. Buscável via `GET /appointments?externalId=P-42`.

### Idempotência

Middleware intercepta `X-Idempotency-Key` header. Usa HybridCache (já existe) com TTL 24h. Mesma key retorna mesma response sem reprocessar.

### Anti-eco

Domain events carregam `sourceApiKeyId`. WebhookDispatcher não envia webhook para webhooks registrados pelo mesmo API Key owner.

### Conflitos

Máquina de estados já rejeita transições inválidas. ExceptionHandling mapeia para 409 Conflict com ProblemDetails. Dashboard exibe notification de conflito com ações (manter/cancelar).

---

## 12. Fora do Escopo

- Drag-and-drop na agenda
- Multi-idioma (português apenas)
- Tema escuro
- PWA/App nativo
- Chat integrado no dashboard

---

## 13. Autenticação do Dashboard

```
Login (email+senha) → POST /auth/login → JWT + Refresh Token
  │
  ├→ JWT no header Authorization: Bearer <token>
  ├→ Refresh token em httpOnly cookie
  └→ Tenant resolvido via claim tenant_id do JWT
```

---

**Aprovado em**: 2026-03-09
