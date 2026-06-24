using EventosVivosBackNet.Application.Commond.Models.DTOs.DtoBase;
using EventosVivosBackNet.Application.Commond.Models.DTOs.Reservations;
using MediatR;

namespace EventosVivosBackNet.Application.Features.Reservations.Commands.ConfirmPayment
{
    public class ConfirmPaymentCommand : IRequest<DtoGenericResponse<DtoReservationResponse>>
    {
        public long ReservationId { get; set; }
    }
}
