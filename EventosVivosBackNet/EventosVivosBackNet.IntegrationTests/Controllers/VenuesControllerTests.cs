using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EventosVivosBackNet.Application.Commond.Models.DTOs.DtoBase;
using EventosVivosBackNet.Application.Commond.Models.DTOs.Venues;
using EventosVivosBackNet.IntegrationTests.Fixtures;
using FluentAssertions;

namespace EventosVivosBackNet.IntegrationTests.Controllers
{
    public class VenuesControllerTests : IClassFixture<EventosVivosWebAppFactory>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public VenuesControllerTests(EventosVivosWebAppFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAllVenues_ReturnsSeededVenues()
        {
            var response = await _client.GetAsync("/api/venues");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadFromJsonAsync<DtoGenericResponse<List<DtoVenueResponse>>>(_jsonOptions);
            body!.EsExitoso.Should().BeTrue();
            body.Resultado.Should().HaveCount(3);
            body.Resultado.Should().Contain(v => v.Name == "Auditorio Central");
            body.Resultado.Should().Contain(v => v.Name == "Sala Norte");
            body.Resultado.Should().Contain(v => v.Name == "Arena Sur");
        }
    }
}
