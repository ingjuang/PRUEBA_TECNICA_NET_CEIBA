using EventosVivosBackNet.Application.Commond.Models.DTOs.DtoBase;
using EventosVivosBackNet.Application.Commond.Models.DTOs.Reservations;
using MediatR;

namespace EventosVivosBackNet.Application.Features.Reservations.Commands.CreateReservation
{
    public class CreateReservationCommand : IRequest<DtoGenericResponse<DtoReservationResponse>>
    {
        public long EventId { get; set; }
        public int Quantity { get; set; }
        public string BuyerName { get; set; } = null!;
        public string BuyerEmail { get; set; } = null!;
    }
}
