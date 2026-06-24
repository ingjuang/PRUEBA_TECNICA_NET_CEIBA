using EventosVivosBackNet.Application.Commond.Models.DTOs.DtoBase;
using EventosVivosBackNet.Application.Commond.Models.DTOs.Events;
using MediatR;

namespace EventosVivosBackNet.Application.Features.Events.Queries.GetEventsFiltered
{
    public class GetEventsFilteredQuery : IRequest<DtoGenericResponse<List<DtoEventResponse>>>
    {
        public string? EventType { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public long? VenueId { get; set; }
        public string? Status { get; set; }
        public string? TitleSearch { get; set; }
    }
}
