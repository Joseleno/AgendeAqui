using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Professionals.GetProfessionalServices;

public sealed record GetProfessionalServicesQuery(Guid ProfessionalId) : IQuery<List<Guid>>;
