using EventosVivosBackNet.Application.Commond.Interfaces.Repositories;
using EventosVivosBackNet.Application.Commond.Models.DTOs.DtoBase;
using EventosVivosBackNet.Application.Commond.Models.DTOs.Reservations;
using EventosVivosBackNet.Domain.Entities;
using MediatR;

namespace EventosVivosBackNet.Application.Features.Reservations.Commands.CancelReservation
{
    public class CancelReservationCommandHandler : IRequestHandler<CancelReservationCommand, DtoGenericResponse<DtoReservationResponse>>
    {
        private readonly IUnitOfWorkDbEventosVivos _unitOfWork;

        public CancelReservationCommandHandler(IUnitOfWorkDbEventosVivos unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DtoGenericResponse<DtoReservationResponse>> Handle(CancelReservationCommand request, CancellationToken cancellationToken)
        {
            var response = new DtoGenericResponse<DtoReservationResponse>();

            var reservation = await _unitOfWork.Repository.GetReservationByIdAsync(request.ReservationId);
            if (reservation == null)
            {
                response.Mensaje = "La reserva especificada no existe.";
                return response;
            }

            if (reservation.Status == "cancelada")
            {
                response.Mensaje = "La reserva ya se encuentra cancelada.";
                return response;
            }

            if (reservation.Status == "pendiente_pago")
            {
                response.Mensaje = "No se puede cancelar una reserva con estado pendiente_pago.";
                return response;
            }

            if (reservation.Status != "confirmada")
            {
                response.Mensaje = "Solo se pueden cancelar reservas con estado confirmada.";
                return response;
            }

            reservation.Status = "cancelada";
            reservation.CancelledAt = DateTime.UtcNow;
            reservation.UpdatedAt = DateTime.UtcNow;

            // RN-07: Si faltan menos de 48 horas para el evento, registrar como pérdida
            var horasParaEvento = (reservation.Event.StartAt - DateTime.UtcNow).TotalHours;
            if (horasParaEvento < 48)
            {
                var loss = new ReservationLoss
                {
                    ReservationId = reservation.Id,
                    EventId = reservation.EventId,
                    Quantity = reservation.Quantity,
                    Reason = "Cancelación con menos de 48 horas",
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Repository.AddReservationLossAsync(loss);
            }

            _unitOfWork.Repository.UpdateReservation(reservation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            response.EsExitoso = true;
            response.Mensaje = horasParaEvento < 48
                ? "Reserva cancelada con penalización. Las entradas no serán liberadas para venta."
                : "Reserva cancelada exitosamente. Las entradas han sido liberadas.";
            response.Resultado = new DtoReservationResponse
            {
                Id = reservation.Id,
                EventId = reservation.EventId,
                Quantity = reservation.Quantity,
                BuyerName = reservation.BuyerName,
                BuyerEmail = reservation.BuyerEmail,
                Status = reservation.Status,
                ReservationCode = reservation.ReservationCode,
                CancelledAt = reservation.CancelledAt,
                CreatedAt = reservation.CreatedAt
            };

            return response;
        }
    }
}
