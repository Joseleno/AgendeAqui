using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Professionals.GetProfessional;

public sealed record GetProfessionalQuery(Guid ProfessionalId) : IQuery<ProfessionalResponse>;
