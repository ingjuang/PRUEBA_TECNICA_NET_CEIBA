using EventosVivosBackNet.Application.Commond.Interfaces.Repositories;
using EventosVivosBackNet.Application.Commond.Interfaces.Repositories.DbEventosVivos;
using EventosVivosBackNet.Application.Features.Events.Queries.GetEventsFiltered;
using EventosVivosBackNet.Domain.Entities;
using FluentAssertions;
using Moq;

namespace EventosVivosBackNet.UnitTests.Features.Events.Queries
{
    public class GetEventsFilteredQueryHandlerTests
    {
        private readonly Mock<IUnitOfWorkDbEventosVivos> _unitOfWorkMock;
        private readonly Mock<IDbEventosVivosRepository> _repositoryMock;
        private readonly GetEventsFilteredQueryHandler _handler;

        public GetEventsFilteredQueryHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWorkDbEventosVivos>();
            _repositoryMock = new Mock<IDbEventosVivosRepository>();
            _unitOfWorkMock.Setup(u => u.Repository).Returns(_repositoryMock.Object);
            _handler = new GetEventsFilteredQueryHandler(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsFilteredEvents()
        {
            var events = new List<Event>
            {
                new() { Id = 1, Title = "Test", Description = "Desc", VenueId = 1,
                    MaxCapacity = 100, StartAt = DateTime.UtcNow.AddDays(5),
                    EndAt = DateTime.UtcNow.AddDays(5).AddHours(4), Price = 50,
                    EventType = "conferencia", Status = "activo",
                    Venue = new Venue { Id = 1, Name = "Venue Test", Capacity = 200, City = "Bogotá" } }
            };
            _repositoryMock.Setup(r => r.GetEventsFilteredAsync(
                It.IsAny<string?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(),
                It.IsAny<long?>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .ReturnsAsync(events);

            var result = await _handler.Handle(new GetEventsFilteredQuery { EventType = "conferencia" }, CancellationToken.None);

            result.EsExitoso.Should().BeTrue();
            result.Resultado.Should().HaveCount(1);
            result.Resultado![0].VenueName.Should().Be("Venue Test");
        }

        [Fact]
        public async Task Handle_RN06_CompletedEventStatus()
        {
            var events = new List<Event>
            {
                new() { Id = 1, Title = "Past Event", Description = "Desc", VenueId = 1,
                    MaxCapacity = 100, StartAt = DateTime.UtcNow.AddDays(-2),
                    EndAt = DateTime.UtcNow.AddDays(-1), Price = 50,
                    EventType = "conferencia", Status = "activo",
                    Venue = new Venue { Id = 1, Name = "V", Capacity = 200, City = "B" } }
            };
            _repositoryMock.Setup(r => r.GetEventsFilteredAsync(
                It.IsAny<string?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(),
                It.IsAny<long?>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .ReturnsAsync(events);

            var result = await _handler.Handle(new GetEventsFilteredQuery(), CancellationToken.None);

            result.Resultado![0].Status.Should().Be("completado");
        }

        [Fact]
        public async Task Handle_EmptyList_ReturnsEmptyResult()
        {
            _repositoryMock.Setup(r => r.GetEventsFilteredAsync(
                It.IsAny<string?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(),
                It.IsAny<long?>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .ReturnsAsync(new List<Event>());

            var result = await _handler.Handle(new GetEventsFilteredQuery(), CancellationToken.None);

            result.EsExitoso.Should().BeTrue();
            result.Resultado.Should().BeEmpty();
        }
    }
}
