using EventosVivosBackNet.Application.Commond.Interfaces.Repositories;
using EventosVivosBackNet.Application.Commond.Interfaces.Repositories.DbEventosVivos;
using EventosVivosBackNet.Application.Features.Venues.Queries.GetAllVenues;
using EventosVivosBackNet.Domain.Entities;
using FluentAssertions;
using Moq;

namespace EventosVivosBackNet.UnitTests.Features.Venues.Queries
{
    public class GetAllVenuesQueryHandlerTests
    {
        private readonly Mock<IUnitOfWorkDbEventosVivos> _unitOfWorkMock;
        private readonly Mock<IDbEventosVivosRepository> _repositoryMock;
        private readonly GetAllVenuesQueryHandler _handler;

        public GetAllVenuesQueryHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWorkDbEventosVivos>();
            _repositoryMock = new Mock<IDbEventosVivosRepository>();
            _unitOfWorkMock.Setup(u => u.Repository).Returns(_repositoryMock.Object);
            _handler = new GetAllVenuesQueryHandler(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsAllVenues()
        {
            var venues = new List<Venue>
            {
                new() { Id = 1, Name = "Auditorio Central", Capacity = 200, City = "Bogotá" },
                new() { Id = 2, Name = "Sala Norte", Capacity = 50, City = "Bogotá" },
                new() { Id = 3, Name = "Arena Sur", Capacity = 500, City = "Medellín" }
            };
            _repositoryMock.Setup(r => r.GetAllVenuesAsync()).ReturnsAsync(venues);

            var result = await _handler.Handle(new GetAllVenuesQuery(), CancellationToken.None);

            result.EsExitoso.Should().BeTrue();
            result.Resultado.Should().HaveCount(3);
            result.Mensaje.Should().Contain("3");
        }

        [Fact]
        public async Task Handle_EmptyVenues_ReturnsEmptyList()
        {
            _repositoryMock.Setup(r => r.GetAllVenuesAsync()).ReturnsAsync(new List<Venue>());

            var result = await _handler.Handle(new GetAllVenuesQuery(), CancellationToken.None);

            result.EsExitoso.Should().BeTrue();
            result.Resultado.Should().BeEmpty();
        }
    }
}
