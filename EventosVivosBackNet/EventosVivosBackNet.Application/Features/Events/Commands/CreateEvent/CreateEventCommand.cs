using EventosVivosBackNet.Application.Commond.Models.DTOs.DtoBase;
using EventosVivosBackNet.Application.Commond.Models.DTOs.Events;
using MediatR;

namespace EventosVivosBackNet.Application.Features.Events.Commands.CreateEvent
{
    public class CreateEventCommand : IRequest<DtoGenericResponse<DtoEventResponse>>
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public long VenueId { get; set; }
        public int MaxCapacity { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public decimal Price { get; set; }
        public string EventType { get; set; } = null!;
    }
}
