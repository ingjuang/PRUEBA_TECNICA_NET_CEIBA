using EventosVivosBackNet.Application.Commond.Interfaces.Repositories;
using EventosVivosBackNet.Application.Commond.Interfaces.Repositories.DbEventosVivos;
using EventosVivosBackNet.Application.Features.Reservations.Commands.CreateReservation;
using EventosVivosBackNet.Domain.Entities;
using FluentAssertions;
using Moq;

namespace EventosVivosBackNet.UnitTests.Features.Reservations.Commands
{
    public class CreateReservationCommandHandlerTests
    {
        private readonly Mock<IUnitOfWorkDbEventosVivos> _unitOfWorkMock;
        private readonly Mock<IDbEventosVivosRepository> _repositoryMock;
        private readonly CreateReservationCommandHandler _handler;

        public CreateReservationCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWorkDbEventosVivos>();
            _repositoryMock = new Mock<IDbEventosVivosRepository>();
            _unitOfWorkMock.Setup(u => u.Repository).Returns(_repositoryMock.Object);
            _handler = new CreateReservationCommandHandler(_unitOfWorkMock.Object);
        }

        private Event ActiveEvent(decimal price = 50m, int capacity = 200) => new()
        {
            Id = 1, Title = "Test", Description = "Test", VenueId = 1,
            MaxCapacity = capacity, StartAt = DateTime.UtcNow.AddDays(10),
            EndAt = DateTime.UtcNow.AddDays(10).AddHours(8),
            Price = price, EventType = "conferencia", Status = "activo"
        };

        private CreateReservationCommand ValidCommand() => new()
        {
            EventId = 1, Quantity = 3,
            BuyerName = "Carlos López", BuyerEmail = "carlos@email.com"
        };

        [Fact]
        public async Task Handle_ValidCommand_CreatesReservation()
        {
            var command = ValidCommand();
            _repositoryMock.Setup(r => r.GetEventByIdAsync(1)).ReturnsAsync(ActiveEvent());
            _repositoryMock.Setup(r => r.GetConfirmedReservationsQuantityByEventAsync(1)).ReturnsAsync(0);
            _repositoryMock.Setup(r => r.GetLostQuantityByEventAsync(1)).ReturnsAsync(0);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.EsExitoso.Should().BeTrue();
            result.Resultado!.Status.Should().Be("pendiente_pago");
            _repositoryMock.Verify(r => r.AddReservationAsync(It.IsAny<Reservation>()), Times.Once);
        }

        [Fact]
        public async Task Handle_QuantityZero_ReturnsError()
        {
            var command = ValidCommand();
            command.Quantity = 0;

            var result = await _handler.Handle(command, CancellationToken.None);

            result.EsExitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("cantidad");
        }

        [Fact]
        public async Task Handle_InvalidEmail_ReturnsError()
        {
            var command = ValidCommand();
            command.BuyerEmail = "invalid-email";

            var result = await _handler.Handle(command, CancellationToken.None);

            result.EsExitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("email");
        }

        [Fact]
        public async Task Handle_EmptyBuyerName_ReturnsError()
        {
            var command = ValidCommand();
            command.BuyerName = "";

            var result = await _handler.Handle(command, CancellationToken.None);

            result.EsExitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("nombre");
        }

        [Fact]
        public async Task Handle_EventNotFound_ReturnsError()
        {
            var command = ValidCommand();
            _repositoryMock.Setup(r => r.GetEventByIdAsync(1)).ReturnsAsync((Event?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.EsExitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("no existe");
        }

        [Fact]
        public async Task Handle_CancelledEvent_ReturnsError()
        {
            var command = ValidCommand();
            var evento = ActiveEvent();
            evento.Status = "cancelado";
            _repositoryMock.Setup(r => r.GetEventByIdAsync(1)).ReturnsAsync(evento);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.EsExitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("activos");
        }

        [Fact]
        public async Task Handle_RN04_LessThan1Hour_ReturnsError()
        {
            var command = ValidCommand();
            var evento = ActiveEvent();
            evento.StartAt = DateTime.UtcNow.AddMinutes(30);
            evento.EndAt = evento.StartAt.AddHours(2);
            _repositoryMock.Setup(r => r.GetEventByIdAsync(1)).ReturnsAsync(evento);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.EsExitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("menos de 1 hora");
        }

        [Fact]
        public async Task Handle_RF03_LessThan24Hours_MaxFiveEntries()
        {
            var command = ValidCommand();
            command.Quantity = 8;
            var evento = ActiveEvent();
            evento.StartAt = DateTime.UtcNow.AddHours(12);
            evento.EndAt = evento.StartAt.AddHours(4);
            _repositoryMock.Setup(r => r.GetEventByIdAsync(1)).ReturnsAsync(evento);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.EsExitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("máximo 5 entradas");
        }

        [Fact]
        public async Task Handle_RN05_PriceOver100_MaxTenEntries()
        {
            var command = ValidCommand();
            command.Quantity = 15;
            var evento = ActiveEvent(price: 150m);
            _repositoryMock.Setup(r => r.GetEventByIdAsync(1)).ReturnsAsync(evento);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.EsExitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("máximo 10 entradas");
        }

        [Fact]
        public async Task Handle_NotEnoughAvailability_ReturnsError()
        {
            var command = ValidCommand();
            command.Quantity = 5;
            var evento = ActiveEvent(capacity: 10);
            _repositoryMock.Setup(r => r.GetEventByIdAsync(1)).ReturnsAsync(evento);
            _repositoryMock.Setup(r => r.GetConfirmedReservationsQuantityByEventAsync(1)).ReturnsAsync(8);
            _repositoryMock.Setup(r => r.GetLostQuantityByEventAsync(1)).ReturnsAsync(0);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.EsExitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("Disponibles: 2");
        }

        [Fact]
        public async Task Handle_RN06_EventAlreadyEnded_MarksCompleted()
        {
            var command = ValidCommand();
            var evento = ActiveEvent();
            evento.StartAt = DateTime.UtcNow.AddDays(-2);
            evento.EndAt = DateTime.UtcNow.AddDays(-1);
            _repositoryMock.Setup(r => r.GetEventByIdAsync(1)).ReturnsAsync(evento);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.EsExitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("finalizado");
            _repositoryMock.Verify(r => r.UpdateEvent(It.Is<Event>(e => e.Status == "completado")), Times.Once);
        }
    }
}
