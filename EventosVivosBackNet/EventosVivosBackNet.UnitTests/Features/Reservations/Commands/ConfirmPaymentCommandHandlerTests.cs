using EventosVivosBackNet.Application.Commond.Interfaces.Repositories;
using EventosVivosBackNet.Application.Commond.Interfaces.Repositories.DbEventosVivos;
using EventosVivosBackNet.Application.Features.Reservations.Commands.ConfirmPayment;
using EventosVivosBackNet.Domain.Entities;
using FluentAssertions;
using Moq;

namespace EventosVivosBackNet.UnitTests.Features.Reservations.Commands
{
    public class ConfirmPaymentCommandHandlerTests
    {
        private readonly Mock<IUnitOfWorkDbEventosVivos> _unitOfWorkMock;
        private readonly Mock<IDbEventosVivosRepository> _repositoryMock;
        private readonly ConfirmPaymentCommandHandler _handler;

        public ConfirmPaymentCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWorkDbEventosVivos>();
            _repositoryMock = new Mock<IDbEventosVivosRepository>();
            _unitOfWorkMock.Setup(u => u.Repository).Returns(_repositoryMock.Object);
            _handler = new ConfirmPaymentCommandHandler(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ValidPendingReservation_ConfirmsAndGeneratesCode()
        {
            var reservation = new Reservation
            {
                Id = 1, EventId = 1, Quantity = 3, BuyerName = "Test",
                BuyerEmail = "test@email.com", Status = "pendiente_pago"
            };
            _repositoryMock.Setup(r => r.GetReservationByIdAsync(1)).ReturnsAsync(reservation);
            _repositoryMock.Setup(r => r.ExistsReservationCodeAsync(It.IsAny<string>())).ReturnsAsync(false);

            var result = await _handler.Handle(new ConfirmPaymentCommand { ReservationId = 1 }, CancellationToken.None);

            result.EsExitoso.Should().BeTrue();
            result.Resultado!.Status.Should().Be("confirmada");
            result.Resultado.ReservationCode.Should().StartWith("EV-");
            result.Resultado.ReservationCode.Should().HaveLength(9);
            _repositoryMock.Verify(r => r.UpdateReservation(It.IsAny<Reservation>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ReservationNotFound_ReturnsError()
        {
            _repositoryMock.Setup(r => r.GetReservationByIdAsync(99)).ReturnsAsync((Reservation?)null);

            var result = await _handler.Handle(new ConfirmPaymentCommand { ReservationId = 99 }, CancellationToken.None);

            result.EsExitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("no existe");
        }

        [Fact]
        public async Task Handle_AlreadyConfirmed_ReturnsError()
        {
            var reservation = new Reservation { Id = 1, Status = "confirmada" };
            _repositoryMock.Setup(r => r.GetReservationByIdAsync(1)).ReturnsAsync(reservation);

            var result = await _handler.Handle(new ConfirmPaymentCommand { ReservationId = 1 }, CancellationToken.None);

            result.EsExitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("ya se encuentra confirmada");
        }

        [Fact]
        public async Task Handle_CancelledReservation_ReturnsError()
        {
            var reservation = new Reservation { Id = 1, Status = "cancelada" };
            _repositoryMock.Setup(r => r.GetReservationByIdAsync(1)).ReturnsAsync(reservation);

            var result = await _handler.Handle(new ConfirmPaymentCommand { ReservationId = 1 }, CancellationToken.None);

            result.EsExitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("cancelada");
        }
    }
}
