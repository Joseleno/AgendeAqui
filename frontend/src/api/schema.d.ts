/**
 * Auto-generated OpenAPI types.
 *
 * Regenerate with: npm run generate-api
 * Requires the API server running at https://localhost:5001
 */

export interface paths {
  "/api/v1/auth/tenant/{slug}": {
    get: {
      parameters: { path: { slug: string } };
      responses: {
        200: { content: { "application/json": components["schemas"]["TenantInfo"] } };
        404: never;
      };
    };
  };
  "/api/v1/auth/login": {
    post: {
      requestBody: { content: { "application/json": components["schemas"]["LoginRequest"] } };
      responses: {
        200: { content: { "application/json": components["schemas"]["LoginResponse"] } };
        401: never;
      };
    };
  };
  "/api/v1/auth/refresh-token": {
    post: {
      requestBody: { content: { "application/json": components["schemas"]["RefreshTokenRequest"] } };
      responses: {
        200: { content: { "application/json": components["schemas"]["LoginResponse"] } };
        401: never;
      };
    };
  };
  "/api/v1/auth/register": {
    post: {
      requestBody: { content: { "application/json": components["schemas"]["RegisterRequest"] } };
      responses: {
        201: { content: { "application/json": components["schemas"]["LoginResponse"] } };
        409: never;
        400: never;
      };
    };
  };
  "/api/v1/appointments": {
    get: {
      parameters: { query: { page?: number; pageSize?: number; professionalId?: string; clientId?: string; dateFrom?: string; dateTo?: string; status?: string } };
      responses: { 200: { content: { "application/json": components["schemas"]["PagedResponse_Appointment"] } } };
    };
    post: {
      requestBody: { content: { "application/json": components["schemas"]["CreateAppointmentRequest"] } };
      responses: { 201: { content: { "application/json": { id: string } } }; 400: never; 409: never };
    };
  };
  "/api/v1/appointments/{id}": {
    get: {
      parameters: { path: { id: string } };
      responses: { 200: { content: { "application/json": components["schemas"]["Appointment"] } }; 404: never };
    };
    put: {
      parameters: { path: { id: string } };
      requestBody: { content: { "application/json": components["schemas"]["UpdateAppointmentRequest"] } };
      responses: { 204: never; 404: never };
    };
    delete: {
      parameters: { path: { id: string } };
      responses: { 204: never; 404: never };
    };
  };
  "/api/v1/appointments/{id}/status": {
    put: {
      parameters: { path: { id: string } };
      requestBody: { content: { "application/json": { status: string } } };
      responses: { 204: never; 400: never; 404: never };
    };
  };
  "/api/v1/appointments/calendar": {
    get: {
      parameters: { query: { dateFrom: string; dateTo: string } };
      responses: { 200: { content: { "application/json": components["schemas"]["CalendarResponse"] } } };
    };
  };
  "/api/v1/appointments/filled-slots": {
    get: {
      parameters: { query: { date: string } };
      responses: { 200: { content: { "application/json": components["schemas"]["FilledSlotsResponse"] } } };
    };
  };
  "/api/v1/availability": {
    get: {
      parameters: { query: { professionalId: string; date: string; serviceId?: string } };
      responses: { 200: { content: { "application/json": components["schemas"]["AvailabilityResponse"] } } };
    };
  };
  "/api/v1/clients": {
    get: {
      parameters: { query: { page?: number; pageSize?: number; search?: string } };
      responses: { 200: { content: { "application/json": components["schemas"]["PagedResponse_Client"] } } };
    };
    post: {
      requestBody: { content: { "application/json": components["schemas"]["CreateClientRequest"] } };
      responses: { 201: { content: { "application/json": { id: string } } }; 409: never };
    };
  };
  "/api/v1/clients/{id}": {
    get: {
      parameters: { path: { id: string } };
      responses: { 200: { content: { "application/json": components["schemas"]["Client"] } }; 404: never };
    };
    put: {
      parameters: { path: { id: string } };
      requestBody: { content: { "application/json": components["schemas"]["UpdateClientRequest"] } };
      responses: { 204: never; 404: never };
    };
    delete: {
      parameters: { path: { id: string } };
      responses: { 204: never; 404: never };
    };
  };
  "/api/v1/professionals": {
    get: {
      parameters: { query: { page?: number; pageSize?: number } };
      responses: { 200: { content: { "application/json": components["schemas"]["PagedResponse_Professional"] } } };
    };
    post: {
      requestBody: { content: { "application/json": components["schemas"]["CreateProfessionalRequest"] } };
      responses: { 201: { content: { "application/json": { id: string } } }; 409: never };
    };
  };
  "/api/v1/professionals/{id}": {
    put: {
      parameters: { path: { id: string } };
      requestBody: { content: { "application/json": components["schemas"]["UpdateProfessionalRequest"] } };
      responses: { 204: never; 404: never };
    };
    delete: {
      parameters: { path: { id: string } };
      responses: { 204: never; 404: never };
    };
  };
  "/api/v1/professionals/search": {
    get: {
      parameters: { query: { page?: number; pageSize?: number; name?: string; specialty?: string; serviceId?: string } };
      responses: { 200: { content: { "application/json": components["schemas"]["PagedResponse_ProfessionalSummary"] } } };
    };
  };
  "/api/v1/professionals/available": {
    get: {
      parameters: { query: { serviceId: string; date: string } };
      responses: { 200: { content: { "application/json": components["schemas"]["ProfessionalAvailability"][] } } };
    };
  };
  "/api/v1/professionals/my-patients": {
    get: {
      parameters: { query: { page?: number; pageSize?: number; search?: string } };
      responses: { 200: { content: { "application/json": components["schemas"]["PagedResponse_PatientSummary"] } } };
    };
  };
  "/api/v1/services": {
    get: {
      parameters: { query: { page?: number; pageSize?: number } };
      responses: { 200: { content: { "application/json": components["schemas"]["PagedResponse_Service"] } } };
    };
    post: {
      requestBody: { content: { "application/json": components["schemas"]["CreateServiceRequest"] } };
      responses: { 201: { content: { "application/json": { id: string } } } };
    };
  };
  "/api/v1/services/{id}": {
    put: {
      parameters: { path: { id: string } };
      requestBody: { content: { "application/json": components["schemas"]["UpdateServiceRequest"] } };
      responses: { 204: never; 404: never };
    };
    delete: {
      parameters: { path: { id: string } };
      responses: { 204: never; 404: never };
    };
  };
  "/api/v1/schedules": {
    get: {
      parameters: { query: { page?: number; pageSize?: number; professionalId?: string } };
      responses: { 200: { content: { "application/json": components["schemas"]["PagedResponse_Schedule"] } } };
    };
    post: {
      requestBody: { content: { "application/json": components["schemas"]["CreateScheduleRequest"] } };
      responses: { 201: { content: { "application/json": { id: string } } } };
    };
  };
  "/api/v1/schedules/{id}": {
    put: {
      parameters: { path: { id: string } };
      requestBody: { content: { "application/json": components["schemas"]["UpdateScheduleRequest"] } };
      responses: { 204: never; 404: never };
    };
    delete: {
      parameters: { path: { id: string } };
      responses: { 204: never; 404: never };
    };
  };
  "/api/v1/absences": {
    get: {
      parameters: { query: { professionalId?: string; from?: string; to?: string } };
      responses: { 200: { content: { "application/json": components["schemas"]["Absence"][] } } };
    };
    post: {
      requestBody: { content: { "application/json": components["schemas"]["CreateAbsenceRequest"] } };
      responses: { 201: { content: { "application/json": { id: string } } } };
    };
  };
  "/api/v1/absences/{id}": {
    delete: {
      parameters: { path: { id: string } };
      responses: { 204: never; 404: never };
    };
  };
  "/api/v1/clinical-notes": {
    get: {
      parameters: { query: { clientId: string; page?: number; pageSize?: number } };
      responses: { 200: { content: { "application/json": components["schemas"]["PagedResponse_ClinicalNote"] } } };
    };
    post: {
      requestBody: { content: { "application/json": components["schemas"]["CreateClinicalNoteRequest"] } };
      responses: { 201: { content: { "application/json": { id: string } } } };
    };
  };
  "/api/v1/clinical-notes/{id}": {
    get: {
      parameters: { path: { id: string } };
      responses: { 200: { content: { "application/json": components["schemas"]["ClinicalNote"] } }; 404: never };
    };
    put: {
      parameters: { path: { id: string } };
      requestBody: { content: { "application/json": components["schemas"]["UpdateClinicalNoteRequest"] } };
      responses: { 204: never; 404: never };
    };
    delete: {
      parameters: { path: { id: string } };
      responses: { 204: never; 404: never };
    };
  };
  "/api/v1/payments": {
    get: {
      parameters: { query: { page?: number; pageSize?: number; appointmentId?: string; from?: string; to?: string } };
      responses: { 200: { content: { "application/json": components["schemas"]["PagedResponse_Payment"] } } };
    };
    post: {
      requestBody: { content: { "application/json": components["schemas"]["CreatePaymentRequest"] } };
      responses: { 201: { content: { "application/json": { id: string } } } };
    };
  };
  "/api/v1/payments/{id}": {
    delete: {
      parameters: { path: { id: string } };
      responses: { 204: never; 404: never };
    };
  };
  "/api/v1/payments/summary": {
    get: {
      parameters: { query: { from: string; to: string } };
      responses: { 200: { content: { "application/json": components["schemas"]["PaymentSummary"] } } };
    };
  };
  "/api/v1/notifications": {
    get: {
      parameters: { query: { dateFrom: string; dateTo: string } };
      responses: { 200: { content: { "application/json": components["schemas"]["Notification"][] } } };
    };
  };
  "/api/v1/notifications/in-app": {
    get: {
      parameters: { query: { page?: number; pageSize?: number } };
      responses: { 200: { content: { "application/json": components["schemas"]["PagedResponse_InAppNotification"] } } };
    };
  };
  "/api/v1/notifications/in-app/unread-count": {
    get: {
      responses: { 200: { content: { "application/json": number } } };
    };
  };
  "/api/v1/notifications/in-app/{id}/read": {
    put: {
      parameters: { path: { id: string } };
      responses: { 204: never };
    };
  };
  "/api/v1/notifications/in-app/read-all": {
    put: {
      responses: { 204: never };
    };
  };
  "/api/v1/reports/attendance": {
    get: {
      parameters: { query: { from: string; to: string; professionalId?: string } };
      responses: { 200: { content: { "application/json": components["schemas"]["AttendanceReport"] } } };
    };
  };
  "/api/v1/reports/revenue": {
    get: {
      parameters: { query: { from: string; to: string } };
      responses: { 200: { content: { "application/json": components["schemas"]["RevenueReport"] } } };
    };
  };
  "/api/v1/reports/appointments-by-status": {
    get: {
      parameters: { query: { from: string; to: string } };
      responses: { 200: { content: { "application/json": components["schemas"]["AppointmentsByStatusResponse"] } } };
    };
  };
  "/api/v1/reports/appointments-timeline": {
    get: {
      parameters: { query: { from: string; to: string; groupBy?: string } };
      responses: { 200: { content: { "application/json": components["schemas"]["AppointmentsTimelineResponse"] } } };
    };
  };
  "/api/v1/reports/busiest-hours": {
    get: {
      parameters: { query: { from: string; to: string } };
      responses: { 200: { content: { "application/json": components["schemas"]["BusiestHoursResponse"] } } };
    };
  };
  "/api/v1/reports/patient-growth": {
    get: {
      parameters: { query: { months?: number } };
      responses: { 200: { content: { "application/json": components["schemas"]["PatientGrowthResponse"] } } };
    };
  };
  "/api/v1/reports/revenue-timeline": {
    get: {
      parameters: { query: { months?: number } };
      responses: { 200: { content: { "application/json": components["schemas"]["RevenueTimelineResponse"] } } };
    };
  };
  "/api/v1/reports/my-stats": {
    get: {
      parameters: { query: { from: string; to: string } };
      responses: { 200: { content: { "application/json": components["schemas"]["ProfessionalStatsResponse"] } } };
    };
  };
  "/api/v1/api-keys": {
    get: {
      responses: { 200: { content: { "application/json": components["schemas"]["ApiKey"][] } } };
    };
    post: {
      requestBody: { content: { "application/json": components["schemas"]["CreateApiKeyRequest"] } };
      responses: { 201: { content: { "application/json": components["schemas"]["CreateApiKeyResponse"] } } };
    };
  };
  "/api/v1/api-keys/{id}": {
    delete: {
      parameters: { path: { id: string } };
      responses: { 204: never; 404: never };
    };
  };
  "/api/v1/webhooks": {
    get: {
      responses: { 200: { content: { "application/json": components["schemas"]["Webhook"][] } } };
    };
    post: {
      requestBody: { content: { "application/json": components["schemas"]["CreateWebhookRequest"] } };
      responses: { 201: { content: { "application/json": { id: string } } } };
    };
  };
  "/api/v1/webhooks/{id}": {
    put: {
      parameters: { path: { id: string } };
      requestBody: { content: { "application/json": components["schemas"]["UpdateWebhookRequest"] } };
      responses: { 204: never; 404: never };
    };
    delete: {
      parameters: { path: { id: string } };
      responses: { 204: never; 404: never };
    };
  };
}

export interface components {
  schemas: {
    // Auth
    TenantInfo: { id: string; name: string };
    LoginRequest: { email: string; password: string };
    RefreshTokenRequest: { refreshToken: string };
    RegisterRequest: { name: string; email: string; phone: string; password: string };
    LoginResponse: { accessToken: string; refreshToken: string; expiresInMinutes: number };

    // Appointments
    Appointment: {
      id: string;
      professionalId: string;
      professionalName: string;
      serviceId: string;
      serviceName: string;
      clientId: string;
      clientName: string;
      date: string;
      startTime: string;
      endTime: string;
      status: string;
      notes: string | null;
      isTeleconsultation: boolean;
      meetingUrl: string | null;
      createdAt: string;
    };
    CreateAppointmentRequest: {
      professionalId: string;
      serviceId: string;
      clientId: string;
      date: string;
      startTime: string;
      notes?: string;
    };
    UpdateAppointmentRequest: {
      professionalId: string;
      serviceId: string;
      clientId: string;
      date: string;
      startTime: string;
      notes?: string;
    };
    AvailableSlot: { start: string; end: string };
    AvailabilityResponse: { date: string; professionalId: string; slots: components["schemas"]["AvailableSlot"][] };
    CalendarAppointment: { id: string; clientName: string; serviceName: string; startTime: string; endTime: string; status: string };
    CalendarAbsence: { id: string; startTime: string | null; endTime: string | null; reason: string | null; isFullDay: boolean };
    CalendarDay: { date: string; appointments: components["schemas"]["CalendarAppointment"][]; absences: components["schemas"]["CalendarAbsence"][] };
    CalendarResponse: { days: components["schemas"]["CalendarDay"][] };
    FilledSlotsResponse: {
      date: string;
      totalSlots: number;
      filledSlots: number;
      availableSlots: number;
      slots: { startTime: string; endTime: string; isFilled: boolean }[];
    };

    // Clients
    Client: { id: string; name: string; email: string; phone: string; createdAt: string };
    CreateClientRequest: { name: string; email: string; phone: string };
    UpdateClientRequest: { name: string; email: string; phone: string };

    // Professionals
    Professional: { id: string; name: string; email: string; phone: string; isActive: boolean; createdAt: string };
    CreateProfessionalRequest: { name: string; email: string; phone: string };
    UpdateProfessionalRequest: { name: string; email: string; phone: string };
    ProfessionalSummary: { id: string; name: string; specialty: string | null; services: string[] };
    ProfessionalAvailability: { professionalId: string; professionalName: string; specialty: string | null; availableSlots: number };
    PatientSummary: { clientId: string; name: string; email: string; phone: string; lastAppointmentDate: string | null; totalAppointments: number };
    ProfessionalStatsResponse: { total: number; completed: number; cancelled: number; noShow: number; revenue: number };

    // Services
    Service: { id: string; name: string; durationMinutes: number; price: number; isActive: boolean; createdAt: string };
    CreateServiceRequest: { name: string; durationMinutes: number; price: number };
    UpdateServiceRequest: { name: string; durationMinutes: number; price: number };

    // Schedules
    Schedule: { id: string; professionalId: string; professionalName: string; dayOfWeek: number; startTime: string; endTime: string; slotDurationMinutes: number; isActive: boolean; createdAt: string };
    CreateScheduleRequest: { professionalId: string; dayOfWeek: number; startTime: string; endTime: string; slotDurationMinutes: number };
    UpdateScheduleRequest: { dayOfWeek: number; startTime: string; endTime: string; slotDurationMinutes: number; isActive: boolean };

    // Absences
    Absence: { id: string; professionalId: string; date: string; startTime: string | null; endTime: string | null; reason: string | null; isFullDay: boolean };
    CreateAbsenceRequest: { professionalId: string; date: string; startTime?: string; endTime?: string; reason?: string };

    // Clinical Notes
    ClinicalNote: { id: string; professionalId: string; professionalName: string; clientId: string; appointmentId: string | null; title: string; content: string; isPrivate: boolean; createdAt: string; updatedAt: string };
    CreateClinicalNoteRequest: { clientId: string; appointmentId?: string; title: string; content: string; isPrivate: boolean };
    UpdateClinicalNoteRequest: { title: string; content: string; isPrivate: boolean };

    // Payments
    Payment: { id: string; appointmentId: string; clientName: string; serviceName: string; amount: number; method: string; status: string; notes: string | null; createdAt: string };
    CreatePaymentRequest: { appointmentId: string; amount: number; method: string; notes?: string };
    MethodSummary: { method: string; total: number; count: number };
    PaymentSummary: { totalReceived: number; totalPending: number; totalRefunded: number; paymentCount: number; byMethod: components["schemas"]["MethodSummary"][] };

    // Notifications
    Notification: { id: string; appointmentId: string; channel: string; recipient: string; templateName: string; status: string; sentAt: string | null; errorMessage: string | null; createdAt: string };
    InAppNotification: { id: string; title: string; message: string; type: string; referenceId: string | null; isRead: boolean; createdAt: string };

    // Reports
    ProfessionalAttendance: { professionalId: string; professionalName: string; total: number; completed: number; cancelled: number; noShow: number };
    AttendanceReport: { totalAppointments: number; totalCompleted: number; totalCancelled: number; totalNoShow: number; breakdown: components["schemas"]["ProfessionalAttendance"][] };
    ServiceRevenue: { serviceId: string; serviceName: string; unitPrice: number; appointmentCount: number; totalRevenue: number };
    RevenueReport: { totalRevenue: number; byService: components["schemas"]["ServiceRevenue"][] };
    StatusCount: { status: string; count: number };
    AppointmentsByStatusResponse: { items: components["schemas"]["StatusCount"][] };
    TimelinePoint: { period: string; scheduled: number; completed: number; cancelled: number; noShow: number };
    AppointmentsTimelineResponse: { points: components["schemas"]["TimelinePoint"][] };
    HourSlot: { dayOfWeek: number; hour: number; count: number };
    BusiestHoursResponse: { slots: components["schemas"]["HourSlot"][] };
    GrowthPoint: { month: string; newClients: number; totalClients: number };
    PatientGrowthResponse: { points: components["schemas"]["GrowthPoint"][] };
    RevenuePoint: { month: string; revenue: number; appointmentCount: number };
    RevenueTimelineResponse: { points: components["schemas"]["RevenuePoint"][] };

    // Patient Portal
    MyAppointment: { id: string; professionalName: string; serviceName: string; date: string; startTime: string; endTime: string; status: string; notes: string | null };

    // Dashboard
    DashboardOverview: { appointmentsToday: number; appointmentsThisWeek: number; activeProfessionals: number; registeredClients: number; cancelledToday: number; noShowToday: number };
    ProfessionalRanking: { professionalId: string; name: string; specialty: string | null; total: number; completed: number; cancelled: number; noShow: number; completionRate: number; noShowRate: number };
    ScheduleConflict: { professionalId: string; professionalName: string; appointmentId1: string; appointmentId2: string; startTime1: string; endTime1: string; startTime2: string; endTime2: string; client1: string; client2: string };
    ClinicAppointment: { id: string; clientName: string; serviceName: string; startTime: string; endTime: string; status: string };
    ProfessionalDay: { professionalId: string; professionalName: string; appointments: components["schemas"]["ClinicAppointment"][] };
    ClinicCalendar: { date: string; professionals: components["schemas"]["ProfessionalDay"][] };

    // API Keys
    ApiKey: { id: string; name: string; keyPrefix: string; isActive: boolean; expiresAt: string | null; createdAt: string };
    CreateApiKeyRequest: { name: string; expiresAt?: string };
    CreateApiKeyResponse: { id: string; rawKey: string; name: string; expiresAt: string | null };

    // Webhooks
    Webhook: { id: string; url: string; events: string[]; isActive: boolean; createdAt: string };
    CreateWebhookRequest: { url: string; secret: string; events: string[] };
    UpdateWebhookRequest: { url: string; events: string[]; isActive: boolean };

    // Paged responses
    PagedResponse_Appointment: { items: components["schemas"]["Appointment"][]; page: number; pageSize: number; totalCount: number; totalPages: number; hasNextPage: boolean; hasPreviousPage: boolean };
    PagedResponse_Client: { items: components["schemas"]["Client"][]; page: number; pageSize: number; totalCount: number; totalPages: number };
    PagedResponse_Professional: { items: components["schemas"]["Professional"][]; page: number; pageSize: number; totalCount: number; totalPages: number };
    PagedResponse_ProfessionalSummary: { items: components["schemas"]["ProfessionalSummary"][]; page: number; pageSize: number; totalCount: number; totalPages: number };
    PagedResponse_PatientSummary: { items: components["schemas"]["PatientSummary"][]; page: number; pageSize: number; totalCount: number; totalPages: number };
    PagedResponse_Service: { items: components["schemas"]["Service"][]; page: number; pageSize: number; totalCount: number; totalPages: number };
    PagedResponse_Schedule: { items: components["schemas"]["Schedule"][]; page: number; pageSize: number; totalCount: number; totalPages: number };
    PagedResponse_Payment: { items: components["schemas"]["Payment"][]; page: number; pageSize: number; totalCount: number; totalPages: number };
    PagedResponse_ClinicalNote: { items: components["schemas"]["ClinicalNote"][]; page: number; pageSize: number; totalCount: number; totalPages: number };
    PagedResponse_InAppNotification: { items: components["schemas"]["InAppNotification"][]; page: number; pageSize: number; totalCount: number; totalPages: number };
  };
}
