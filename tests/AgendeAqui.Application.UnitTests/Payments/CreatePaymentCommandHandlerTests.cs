using AgendeAqui.Application.Payments.CreatePayment;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments;
using AgendeAqui.Domain.Payments;
using AgendeAqui.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Payments;

public sealed class CreatePaymentCommandHandlerTests
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly CreatePaymentCommandHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public CreatePaymentCommandHandlerTests()
    {
        _paymentRepository = Substitute.For<IPaymentRepository>();
        _appointmentRepository = Substitute.For<IAppointmentRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _tenantProvider.GetTenantId().Returns(_tenantId);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        _handler = new CreatePaymentCommandHandler(
            _paymentRepository,
            _appointmentRepository,
            _unitOfWork,
            _tenantProvider);
    }

    private Appointment CreateAppointment()
    {
        var timeSlot = TimeSlot.Create(new TimeOnly(9, 0), new TimeOnly(10, 0)).Value;
        return Appointment.Create(
            _tenantId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            timeSlot).Value;
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreatePaymentAndReturnId()
    {
        var appointment = CreateAppointment();
        _appointmentRepository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);

        var command = new CreatePaymentCommand(appointment.Id, 150.00m, "Pix", "Test payment");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _paymentRepository.Received(1).AddAsync(Arg.Any<Payment>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AppointmentNotFound_ShouldReturnError()
    {
        _appointmentRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Appointment?)null);

        var command = new CreatePaymentCommand(Guid.NewGuid(), 100m, "Cash", null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PaymentErrors.AppointmentNotFound);
    }

    [Fact]
    public async Task Handle_CancelledAppointment_ShouldReturnNotEligible()
    {
        var appointment = CreateAppointment();
        appointment.Cancel("No longer needed");
        _appointmentRepository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);

        var command = new CreatePaymentCommand(appointment.Id, 100m, "Card", null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PaymentErrors.AppointmentNotEligible);
    }
}
