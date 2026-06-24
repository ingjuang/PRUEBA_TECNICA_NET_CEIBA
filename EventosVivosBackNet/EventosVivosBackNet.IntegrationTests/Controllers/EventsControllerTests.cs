using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EventosVivosBackNet.Application.Commond.Models.DTOs.DtoBase;
using EventosVivosBackNet.Application.Commond.Models.DTOs.Events;
using EventosVivosBackNet.Application.Features.Events.Commands.CreateEvent;
using EventosVivosBackNet.IntegrationTests.Fixtures;
using FluentAssertions;

namespace EventosVivosBackNet.IntegrationTests.Controllers
{
    public class EventsControllerTests : IClassFixture<EventosVivosWebAppFactory>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public EventsControllerTests(EventosVivosWebAppFactory factory)
        {
            _client = factory.CreateClient();
        }

        private CreateEventCommand ValidEvent() => new()
        {
            Title = "Conferencia Integration Test",
            Description = "Descripción completa para la prueba de integración del evento.",
            VenueId = 1,
            MaxCapacity = 100,
            StartAt = DateTime.UtcNow.AddDays(30),
            EndAt = DateTime.UtcNow.AddDays(30).AddHours(8),
            Price = 50m,
            EventType = "conferencia"
        };

        [Fact]
        public async Task CreateEvent_ValidData_Returns201()
        {
            var command = ValidEvent();

            var response = await _client.PostAsJsonAsync("/api/events", command);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var body = await response.Content.ReadFromJsonAsync<DtoGenericResponse<DtoEventResponse>>(_jsonOptions);
            body!.EsExitoso.Should().BeTrue();
            body.Resultado!.Status.Should().Be("activo");
            body.Resultado.VenueName.Should().Be("Auditorio Central");
        }

        [Fact]
        public async Task CreateEvent_InvalidVenue_Returns400()
        {
            var command = ValidEvent();
            command.VenueId = 999;

            var response = await _client.PostAsJsonAsync("/api/events", command);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateEvent_RN01_ExceedsCapacity_Returns400()
        {
            var command = ValidEvent();
            command.MaxCapacity = 300;

            var response = await _client.PostAsJsonAsync("/api/events", command);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var body = await response.Content.ReadFromJsonAsync<DtoGenericResponse<DtoEventResponse>>(_jsonOptions);
            body!.Mensaje.Should().Contain("capacidad del venue");
        }

        [Fact]
        public async Task CreateEvent_RN03_WeekendAfter22_Returns400()
        {
            var command = ValidEvent();
            var date = DateTime.UtcNow.Date;
            while (date.DayOfWeek != DayOfWeek.Saturday) date = date.AddDays(1);
            command.StartAt = date.AddDays(14).AddHours(23);
            command.EndAt = command.StartAt.AddHours(3);

            var response = await _client.PostAsJsonAsync("/api/events", command);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var body = await response.Content.ReadFromJsonAsync<DtoGenericResponse<DtoEventResponse>>(_jsonOptions);
            body!.Mensaje.Should().Contain("22:00");
        }

        [Fact]
        public async Task GetEvents_ReturnsOkWithList()
        {
            var response = await _client.GetAsync("/api/events");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadFromJsonAsync<DtoGenericResponse<List<DtoEventResponse>>>(_jsonOptions);
            body!.EsExitoso.Should().BeTrue();
        }

        [Fact]
        public async Task GetEvents_FilterByType_ReturnsFiltered()
        {
            // Create an event first
            await _client.PostAsJsonAsync("/api/events", new CreateEventCommand
            {
                Title = "Taller para filtro test",
                Description = "Descripción del taller para probar filtros por tipo de evento.",
                VenueId = 2, MaxCapacity = 20,
                StartAt = DateTime.UtcNow.AddDays(40),
                EndAt = DateTime.UtcNow.AddDays(40).AddHours(4),
                Price = 30m, EventType = "taller"
            });

            var response = await _client.GetAsync("/api/events?eventType=taller");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadFromJsonAsync<DtoGenericResponse<List<DtoEventResponse>>>(_jsonOptions);
            body!.Resultado!.Should().OnlyContain(e => e.EventType == "taller");
        }

        [Fact]
        public async Task GetOccupancyReport_EventNotFound_Returns404()
        {
            var response = await _client.GetAsync("/api/events/9999/occupancy");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
