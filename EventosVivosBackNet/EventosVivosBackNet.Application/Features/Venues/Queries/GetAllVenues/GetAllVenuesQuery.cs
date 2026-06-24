using EventosVivosBackNet.Application.Commond.Models.DTOs.DtoBase;
using EventosVivosBackNet.Application.Commond.Models.DTOs.Venues;
using MediatR;

namespace EventosVivosBackNet.Application.Features.Venues.Queries.GetAllVenues
{
    public class GetAllVenuesQuery : IRequest<DtoGenericResponse<List<DtoVenueResponse>>>
    {
    }
}
