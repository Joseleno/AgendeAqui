import { http, HttpResponse } from 'msw'

export const handlers = [
  http.post('/api/v1/auth/login', async ({ request }) => {
    const body = (await request.json()) as { email: string; password: string }

    if (body.email === 'admin@test.com' && body.password === 'password123') {
      return HttpResponse.json({
        accessToken: 'fake-jwt-token',
        refreshToken: 'fake-refresh-token',
        expiresInMinutes: 60,
      })
    }

    return HttpResponse.json(
      { title: 'User.InvalidCredentials', detail: 'Invalid email or password.' },
      { status: 401 },
    )
  }),

  http.get('/api/v1/appointments', ({ request }) => {
    const url = new URL(request.url)
    const dateFrom = url.searchParams.get('dateFrom')

    if (dateFrom) {
      return HttpResponse.json({
        items: [
          {
            id: '11111111-1111-1111-1111-111111111111',
            professionalId: 'p1',
            professionalName: 'Dr. Silva',
            serviceId: 's1',
            serviceName: 'Consulta',
            clientId: 'c1',
            clientName: 'Maria Santos',
            date: dateFrom,
            startTime: '09:00:00',
            endTime: '09:30:00',
            status: 'Scheduled',
            notes: null,
            createdAt: '2026-01-01T00:00:00Z',
          },
          {
            id: '22222222-2222-2222-2222-222222222222',
            professionalId: 'p1',
            professionalName: 'Dr. Silva',
            serviceId: 's1',
            serviceName: 'Consulta',
            clientId: 'c2',
            clientName: 'Joao Lima',
            date: dateFrom,
            startTime: '10:00:00',
            endTime: '10:30:00',
            status: 'Confirmed',
            notes: null,
            createdAt: '2026-01-01T00:00:00Z',
          },
        ],
        page: 1,
        pageSize: 50,
        totalCount: 2,
        totalPages: 1,
        hasNextPage: false,
        hasPreviousPage: false,
      })
    }

    return HttpResponse.json({ items: [], page: 1, pageSize: 10, totalCount: 0, totalPages: 0, hasNextPage: false, hasPreviousPage: false })
  }),

  http.get('/api/v1/reports/dashboard', () => {
    return HttpResponse.json({
      appointmentsToday: 12,
      appointmentsThisWeek: 47,
      activeProfessionals: 5,
      registeredClients: 238,
      cancelledToday: 2,
      noShowToday: 1,
    })
  }),

  http.get('/api/v1/reports/appointments-by-status', () => {
    return HttpResponse.json({ items: [{ status: 'Scheduled', count: 8 }, { status: 'Completed', count: 4 }] })
  }),

  http.get('/api/v1/reports/appointments-timeline', () => {
    return HttpResponse.json({ points: [] })
  }),

  http.get('/api/v1/reports/busiest-hours', () => {
    return HttpResponse.json({ slots: [] })
  }),

  http.get('/api/v1/reports/patient-growth', () => {
    return HttpResponse.json({ points: [] })
  }),

  http.get('/api/v1/clients', () => {
    return HttpResponse.json({
      items: [
        { id: 'c1', name: 'Ana Costa', email: 'ana@test.com', phone: '11999990001', createdAt: '2026-01-01T00:00:00Z' },
        { id: 'c2', name: 'Bruno Lima', email: 'bruno@test.com', phone: '11999990002', createdAt: '2026-01-01T00:00:00Z' },
        { id: 'c3', name: 'Carla Souza', email: 'carla@test.com', phone: '11999990003', createdAt: '2026-01-01T00:00:00Z' },
      ],
      page: 1,
      pageSize: 10,
      totalCount: 3,
      totalPages: 1,
    })
  }),

  http.post('/api/v1/clients', () => {
    return HttpResponse.json({ id: 'c-new' }, { status: 201 })
  }),

  http.put('/api/v1/clients/:id', () => {
    return new HttpResponse(null, { status: 204 })
  }),
]
