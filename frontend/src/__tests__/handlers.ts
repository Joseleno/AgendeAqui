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
]
