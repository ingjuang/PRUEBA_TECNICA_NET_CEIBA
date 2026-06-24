using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EventosVivosBackNet.Application.Commond.Models.DTOs.DtoBase;
using EventosVivosBackNet.Application.Commond.Models.DTOs.Events;
using EventosVivosBackNet.Application.Commond.Models.DTOs.Reservations;
using EventosVivosBackNet.Application.Features.Events.Commands.CreateEvent;
using EventosVivosBackNet.Application.Features.Reservations.Commands.CreateReservation;
using EventosVivosBackNet.IntegrationTests.Fixtures;
using FluentAssertions;

namespace EventosVivosBackNet.IntegrationTests.Controllers
{
    public class ReservationsControllerTests : IClassFixture<EventosVivosWebAppFactory>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public ReservationsControllerTests(EventosVivosWebAppFactory factory)
        {
            _client = factory.CreateClient();
        }

        private static int _dayOffset = 50;

        private async Task<long> CreateActiveEventAsync(decimal price = 50m, int maxCapacity = 100)
        {
            var offset = Interlocked.Increment(ref _dayOffset);
            var command = new CreateEventCommand
            {
                Title = $"Evento Test {Guid.NewGuid():N}"[..30],
                Description = "Descripción válida para un evento de prueba de integración.",
                VenueId = 3,
                MaxCapacity = maxCapacity,
                StartAt = DateTime.UtcNow.AddDays(offset),
                EndAt = DateTime.UtcNow.AddDays(offset).AddHours(8),
                Price = price,
                EventType = "concierto"
            };
            var response = await _client.PostAsJsonAsync("/api/events", command);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadFromJsonAsync<DtoGenericResponse<DtoEventResponse>>(_jsonOptions);
            return body!.Resultado!.Id;
        }

        [Fact]
        public async Task FullFlow_CreateReservation_Confirm_Cancel()
        {
            var eventId = await CreateActiveEventAsync();

            // RF-03: Create reservation
            var createRes = await _client.PostAsJsonAsync("/api/reservations", new CreateReservationCommand
            {
                EventId = eventId, Quantity = 3,
                BuyerName = "Integration Test", BuyerEmail = "test@integration.com"
            });
            createRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await createRes.Content.ReadFromJsonAsync<DtoGenericResponse<DtoReservationResponse>>(_jsonOptions);
            created!.Resultado!.Status.Should().Be("pendiente_pago");
            var reservationId = created.Resultado.Id;

            // RF-04: Confirm payment
            var confirmRes = await _client.PatchAsync($"/api/reservations/{reservationId}/confirm", null);
            confirmRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var confirmed = await confirmRes.Content.ReadFromJsonAsync<DtoGenericResponse<DtoReservationResponse>>(_jsonOptions);
            confirmed!.Resultado!.Status.Should().Be("confirmada");
            confirmed.Resultado.ReservationCode.Should().StartWith("EV-");

            // RF-04: Double confirm should fail
            var doubleConfirm = await _client.PatchAsync($"/api/reservations/{reservationId}/confirm", null);
            doubleConfirm.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // RF-05: Cancel reservation
            var cancelRes = await _client.PatchAsync($"/api/reservations/{reservationId}/cancel", null);
            cancelRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var cancelled = await cancelRes.Content.ReadFromJsonAsync<DtoGenericResponse<DtoReservationResponse>>(_jsonOptions);
            cancelled!.Resultado!.Status.Should().Be("cancelada");
            cancelled.Resultado.CancelledAt.Should().NotBeNull();

            // RF-05: Double cancel should fail
            var doubleCancel = await _client.PatchAsync($"/api/reservations/{reservationId}/cancel", null);
            doubleCancel.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // RF-06: Occupancy report
            var reportRes = await _client.GetAsync($"/api/events/{eventId}/occupancy");
            reportRes.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CreateReservation_InvalidEmail_Returns400()
        {
            var eventId = await CreateActiveEventAsync();

            var response = await _client.PostAsJsonAsync("/api/reservations", new CreateReservationCommand
            {
                EventId = eventId, Quantity = 1,
                BuyerName = "Test", BuyerEmail = "not-an-email"
            });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateReservation_ZeroQuantity_Returns400()
        {
            var eventId = await CreateActiveEventAsync();

            var response = await _client.PostAsJsonAsync("/api/reservations", new CreateReservationCommand
            {
                EventId = eventId, Quantity = 0,
                BuyerName = "Test", BuyerEmail = "test@email.com"
            });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateReservation_RN05_PriceOver100_MaxTen_Returns400()
        {
            var eventId = await CreateActiveEventAsync(price: 150m, maxCapacity: 200);

            var response = await _client.PostAsJsonAsync("/api/reservations", new CreateReservationCommand
            {
                EventId = eventId, Quantity = 15,
                BuyerName = "Test", BuyerEmail = "test@email.com"
            });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var body = await response.Content.ReadFromJsonAsync<DtoGenericResponse<DtoReservationResponse>>(_jsonOptions);
            body!.Mensaje.Should().Contain("máximo 10 entradas");
        }

        [Fact]
        public async Task CreateReservation_ExceedsCapacity_Returns400()
        {
            var eventId = await CreateActiveEventAsync(maxCapacity: 5);

            var response = await _client.PostAsJsonAsync("/api/reservations", new CreateReservationCommand
            {
                EventId = eventId, Quantity = 10,
                BuyerName = "Test", BuyerEmail = "test@email.com"
            });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var body = await response.Content.ReadFromJsonAsync<DtoGenericResponse<DtoReservationResponse>>(_jsonOptions);
            body!.Mensaje.Should().Contain("Disponibles");
        }

        [Fact]
        public async Task CancelReservation_PendingPayment_Returns400()
        {
            var eventId = await CreateActiveEventAsync();
            var createRes = await _client.PostAsJsonAsync("/api/reservations", new CreateReservationCommand
            {
                EventId = eventId, Quantity = 2,
                BuyerName = "Test", BuyerEmail = "test@email.com"
            });
            var created = await createRes.Content.ReadFromJsonAsync<DtoGenericResponse<DtoReservationResponse>>(_jsonOptions);

            var cancelRes = await _client.PatchAsync($"/api/reservations/{created!.Resultado!.Id}/cancel", null);

            cancelRes.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var body = await cancelRes.Content.ReadFromJsonAsync<DtoGenericResponse<DtoReservationResponse>>(_jsonOptions);
            body!.Mensaje.Should().Contain("pendiente_pago");
        }
    }
}
