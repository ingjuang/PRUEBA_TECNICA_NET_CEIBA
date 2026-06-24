using EventosVivosBackNet.Application.Commond.Interfaces.Repositories;
using EventosVivosBackNet.Application.Commond.Interfaces.Repositories.DbEventosVivos;
using EventosVivosBackNet.Application.Features.Events.Queries.GetOccupancyReport;
using EventosVivosBackNet.Domain.Entities;
using FluentAssertions;
using Moq;

namespace EventosVivosBackNet.UnitTests.Features.Events.Queries
{
    public class GetOccupancyReportQueryHandlerTests
    {
        private readonly Mock<IUnitOfWorkDbEventosVivos> _unitOfWorkMock;
        private readonly Mock<IDbEventosVivosRepository> _repositoryMock;
        private readonly GetOccupancyReportQueryHandler _handler;

        public GetOccupancyReportQueryHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWorkDbEventosVivos>();
            _repositoryMock = new Mock<IDbEventosVivosRepository>();
            _unitOfWorkMock.Setup(u => u.Repository).Returns(_repositoryMock.Object);
            _handler = new GetOccupancyReportQueryHandler(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ValidEvent_ReturnsOccupancyReport()
        {
            var evento = new Event
            {
                Id = 1, Title = "Test", MaxCapacity = 100, Price = 50m,
                Status = "activo", StartAt = DateTime.UtcNow.AddDays(5),
                EndAt = DateTime.UtcNow.AddDays(5).AddHours(4)
            };
            _repositoryMock.Setup(r => r.GetEventByIdAsync(1)).ReturnsAsync(evento);
            _repositoryMock.Setup(r => r.GetConfirmedReservationsQuantityByEventAsync(1)).ReturnsAsync(30);
            _repositoryMock.Setup(r => r.GetLostQuantityByEventAsync(1)).ReturnsAsync(5);

            var result = await _handler.Handle(new GetOccupancyReportQuery { EventId = 1 }, CancellationToken.None);

            result.EsExitoso.Should().BeTrue();
            result.Resultado!.TotalSold.Should().Be(30);
            result.Resultado.TotalAvailable.Should().Be(65);
            result.Resultado.OccupancyPercentage.Should().Be(35m);
            result.Resultado.TotalRevenue.Should().Be(1500m);
        }

        [Fact]
        public async Task Handle_EventNotFound_ReturnsError()
        {
            _repositoryMock.Setup(r => r.GetEventByIdAsync(99)).ReturnsAsync((Event?)null);

            var result = await _handler.Handle(new GetOccupancyReportQuery { EventId = 99 }, CancellationToken.None);

            result.EsExitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("no existe");
        }

        [Fact]
        public async Task Handle_RN06_PastEvent_UpdatesToCompleted()
        {
            var evento = new Event
            {
                Id = 1, Title = "Past", MaxCapacity = 100, Price = 50m,
                Status = "activo", StartAt = DateTime.UtcNow.AddDays(-2),
                EndAt = DateTime.UtcNow.AddDays(-1)
            };
            _repositoryMock.Setup(r => r.GetEventByIdAsync(1)).ReturnsAsync(evento);
            _repositoryMock.Setup(r => r.GetConfirmedReservationsQuantityByEventAsync(1)).ReturnsAsync(50);
            _repositoryMock.Setup(r => r.GetLostQuantityByEventAsync(1)).ReturnsAsync(0);

            var result = await _handler.Handle(new GetOccupancyReportQuery { EventId = 1 }, CancellationToken.None);

            result.EsExitoso.Should().BeTrue();
            result.Resultado!.Status.Should().Be("completado");
            _repositoryMock.Verify(r => r.UpdateEvent(It.Is<Event>(e => e.Status == "completado")), Times.Once);
        }
    }
}
