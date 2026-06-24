using EventosVivosBackNet.Application.Commond.Interfaces.Repositories;
using EventosVivosBackNet.Application.Commond.Interfaces.Repositories.DbEventosVivos;
using EventosVivosBackNet.Application.Features.Events.Commands.CreateEvent;
using EventosVivosBackNet.Domain.Entities;
using FluentAssertions;
using Moq;

namespace EventosVivosBackNet.UnitTests.Features.Events.Commands
{
    public class CreateEventCommandHandlerTests
    {
        private readonly Mock<IUnitOfWorkDbEventosVivos> _unitOfWorkMock;
        private readonly Mock<IDbEventosVivosRepository> _repositoryMock;
        private readonly CreateEventCommandHandler _handler;

        public CreateEventCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWorkDbEventosVivos>();
            _repositoryMock = new Mock<IDbEventosVivosRepository>();
            _unitOfWorkMock.Setup(u => u.Repository).Returns(_repositoryMock.Object);
            _handler = new CreateEventCommandHandler(_unitOfWorkMock.Object);
        }

        private CreateEventCommand ValidCommand() => new()
        {
            Title = "Conferencia de prueba",
            Description = "Descripción válida para el evento de prueba técnica.",
            VenueId = 1,
            MaxCapacity = 100,
            StartAt = DateTime.UtcNow.AddDays(10),
            EndAt = DateTime.UtcNow.AddDays(10).AddHours(8),
            Price = 50m,
            EventType = "conferencia"
        };

        [Fact]
        public async Task Handle_ValidCommand_CreatesEventSuccessfully()
        {
            var command = ValidCommand();
            _repositoryMock.Setup(r => r.GetVenueByIdAsync(command.VenueId))
                .ReturnsAsync(new Venue { Id = 1, Name = "Test Venue", Capacity = 200, City = "Bogotá" });
            _repositoryMock.Setup(r => r.ExistsOverlappingEventAsync(It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), null))
                .ReturnsAsync(false);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.EsExitoso.Should().BeTrue();
            result.Resultado.Should().NotBeNull();
            result.Resultado!.Status.Should().Be("activo");
            _repositoryMock.Verify(r => r.AddEventAsync(It.IsAny<Event>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData("", "El título es obligatorio")]
        [InlineData("abc", "El título es obligatorio")]
        public async Task Handle_InvalidTitle_ReturnsError(string title, string expectedContains)
        {
            var command = ValidCommand();
            command.Title = title;

            var result = await _handler.Handle(command, CancellationToken.None);

            result.EsExitoso.Should().BeFalse();
            result.Mensaje.Should().Contain(expectedContains);
        }

        [Fact]
        public async Task Handle_InvalidDescription_ReturnsError()
        {
            var command = ValidCommand();
            command.Description = "corta";

            var result = await _handler.Handle(command, CancellationToken.None);

            result.EsExitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("descripción");
        }

        [Fact]
        public async Task Handle_InvalidEventType_ReturnsError()
        {
            var command = ValidCommand();
            command.EventType = "invalido";

            var result = await _handler.Handle(command, CancellationToken.None);

            result.EsExitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("tipo de evento");
        }

        [Fact]
        public async Task Handle_PastStartDate_ReturnsError()
        {
            var command = ValidCommand();
            command.StartAt = DateTime.UtcNow.AddHours(-1);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.EsExitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("futura");
        }

        [Fact]
        public async Task Handle_EndBeforeStart_ReturnsError()
        {
            var command = ValidCommand();
            command.EndAt = command.StartAt.AddHours(-1);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.EsExitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("posterior");
        }

        [Fact]
        public async Task Handle_NegativePrice_ReturnsError()
        {
            var command = ValidCommand();
            command.Price = -10;

            var result = await _handler.Handle(command, CancellationToken.None);

            result.EsExitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("precio");
        }

        [Fact]
        public async Task Handle_ZeroCapacity_ReturnsError()
        {
            var command = ValidCommand();
            command.MaxCapacity = 0;

            var result = await _handler.Handle(command, CancellationToken.None);

            result.EsExitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("capacidad");
        }

        [Fact]
        public async Task Handle_RN03_WeekendAfter22_ReturnsError()
        {
            var command = ValidCommand();
            // Find next Saturday
            var date = DateTime.UtcNow.Date;
            while (date.DayOfWeek != DayOfWeek.Saturday) date = date.AddDays(1);
            command.StartAt = date.AddDays(7).AddHours(23);
            command.EndAt = command.StartAt.AddHours(3);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.EsExitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("22:00");
        }

        [Fact]
        public async Task Handle_VenueNotFound_ReturnsError()
        {
            var command = ValidCommand();
            _repositoryMock.Setup(r => r.GetVenueByIdAsync(command.VenueId)).ReturnsAsync((Venue?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.EsExitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("venue");
        }

        [Fact]
        public async Task Handle_RN01_ExceedsVenueCapacity_ReturnsError()
        {
            var command = ValidCommand();
            command.MaxCapacity = 300;
            _repositoryMock.Setup(r => r.GetVenueByIdAsync(command.VenueId))
                .ReturnsAsync(new Venue { Id = 1, Name = "Test", Capacity = 200, City = "Bogotá" });

            var result = await _handler.Handle(command, CancellationToken.None);

            result.EsExitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("capacidad del venue");
        }

        [Fact]
        public async Task Handle_RN02_OverlappingEvent_ReturnsError()
        {
            var command = ValidCommand();
            _repositoryMock.Setup(r => r.GetVenueByIdAsync(command.VenueId))
                .ReturnsAsync(new Venue { Id = 1, Name = "Test", Capacity = 200, City = "Bogotá" });
            _repositoryMock.Setup(r => r.ExistsOverlappingEventAsync(It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), null))
                .ReturnsAsync(true);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.EsExitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("superpuestos");
        }
    }
}
