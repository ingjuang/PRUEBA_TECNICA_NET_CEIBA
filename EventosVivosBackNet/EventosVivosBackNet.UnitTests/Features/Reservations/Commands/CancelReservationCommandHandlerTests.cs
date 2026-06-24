using EventosVivosBackNet.Application.Commond.Interfaces.Repositories;
using EventosVivosBackNet.Application.Commond.Interfaces.Repositories.DbEventosVivos;
using EventosVivosBackNet.Application.Features.Reservations.Commands.CancelReservation;
using EventosVivosBackNet.Domain.Entities;
using FluentAssertions;
using Moq;

namespace EventosVivosBackNet.UnitTests.Features.Reservations.Commands
{
    public class CancelReservationCommandHandlerTests
    {
        private readonly Mock<IUnitOfWorkDbEventosVivos> _unitOfWorkMock;
        private readonly Mock<IDbEventosVivosRepository> _repositoryMock;
        private readonly CancelReservationCommandHandler _handler;

        public CancelReservationCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWorkDbEventosVivos>();
            _repositoryMock = new Mock<IDbEventosVivosRepository>();
            _unitOfWorkMock.Setup(u => u.Repository).Returns(_repositoryMock.Object);
            _handler = new CancelReservationCommandHandler(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ConfirmedReservation_MoreThan48Hours_CancelsAndFrees()
        {
            var evento = new Event
            {
                Id = 1, StartAt = DateTime.UtcNow.AddDays(5), EndAt = DateTime.UtcNow.AddDays(5).AddHours(4)
            };
            var reservation = new Reservation
            {
                Id = 1, EventId = 1, Quantity = 3, BuyerName = "Test",
                BuyerEmail = "test@email.com", Status = "confirmada",
                ReservationCode = "EV-100001", Event = evento
            };
            _repositoryMock.Setup(r => r.GetReservationByIdAsync(1)).ReturnsAsync(reservation);

            var result = await _handler.Handle(new CancelReservationCommand { ReservationId = 1 }, CancellationToken.None);

            result.EsExitoso.Should().BeTrue();
            result.Resultado!.Status.Should().Be("cancelada");
            result.Resultado.CancelledAt.Should().NotBeNull();
            result.Mensaje.Should().Contain("liberadas");
            _repositoryMock.Verify(r => r.AddReservationLossAsync(It.IsAny<ReservationLoss>()), Times.Never);
        }

        [Fact]
        public async Task Handle_RN07_LessThan48Hours_CancelsWithPenalty()
        {
            var evento = new Event
            {
                Id = 1, StartAt = DateTime.UtcNow.AddHours(24), EndAt = DateTime.UtcNow.AddHours(28)
            };
            var reservation = new Reservation
            {
                Id = 1, EventId = 1, Quantity = 4, BuyerName = "Test",
                BuyerEmail = "test@email.com", Status = "confirmada",
                ReservationCode = "EV-100002", Event = evento
            };
            _repositoryMock.Setup(r => r.GetReservationByIdAsync(1)).ReturnsAsync(reservation);

            var result = await _handler.Handle(new CancelReservationCommand { ReservationId = 1 }, CancellationToken.None);

            result.EsExitoso.Should().BeTrue();
            result.Resultado!.Status.Should().Be("cancelada");
            result.Mensaje.Should().Contain("penalización");
            _repositoryMock.Verify(r => r.AddReservationLossAsync(It.Is<ReservationLoss>(
                l => l.Quantity == 4 && l.EventId == 1)), Times.Once);
        }

        [Fact]
        public async Task Handle_ReservationNotFound_ReturnsError()
        {
            _repositoryMock.Setup(r => r.GetReservationByIdAsync(99)).ReturnsAsync((Reservation?)null);

            var result = await _handler.Handle(new CancelReservationCommand { ReservationId = 99 }, CancellationToken.None);

            result.EsExitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("no existe");
        }

        [Fact]
        public async Task Handle_AlreadyCancelled_ReturnsError()
        {
            var reservation = new Reservation { Id = 1, Status = "cancelada" };
            _repositoryMock.Setup(r => r.GetReservationByIdAsync(1)).ReturnsAsync(reservation);

            var result = await _handler.Handle(new CancelReservationCommand { ReservationId = 1 }, CancellationToken.None);

            result.EsExitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("ya se encuentra cancelada");
        }

        [Fact]
        public async Task Handle_PendingPayment_ReturnsError()
        {
            var reservation = new Reservation { Id = 1, Status = "pendiente_pago" };
            _repositoryMock.Setup(r => r.GetReservationByIdAsync(1)).ReturnsAsync(reservation);

            var result = await _handler.Handle(new CancelReservationCommand { ReservationId = 1 }, CancellationToken.None);

            result.EsExitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("pendiente_pago");
        }
    }
}
