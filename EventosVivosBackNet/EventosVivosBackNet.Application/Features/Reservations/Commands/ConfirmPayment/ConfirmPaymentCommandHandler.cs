using EventosVivosBackNet.Application.Commond.Interfaces.Repositories;
using EventosVivosBackNet.Application.Commond.Models.DTOs.DtoBase;
using EventosVivosBackNet.Application.Commond.Models.DTOs.Reservations;
using MediatR;

namespace EventosVivosBackNet.Application.Features.Reservations.Commands.ConfirmPayment
{
    public class ConfirmPaymentCommandHandler : IRequestHandler<ConfirmPaymentCommand, DtoGenericResponse<DtoReservationResponse>>
    {
        private readonly IUnitOfWorkDbEventosVivos _unitOfWork;

        public ConfirmPaymentCommandHandler(IUnitOfWorkDbEventosVivos unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DtoGenericResponse<DtoReservationResponse>> Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
        {
            var response = new DtoGenericResponse<DtoReservationResponse>();

            var reservation = await _unitOfWork.Repository.GetReservationByIdAsync(request.ReservationId);
            if (reservation == null)
            {
                response.Mensaje = "La reserva especificada no existe.";
                return response;
            }

            if (reservation.Status == "confirmada")
            {
                response.Mensaje = "La reserva ya se encuentra confirmada.";
                return response;
            }

            if (reservation.Status == "cancelada")
            {
                response.Mensaje = "No se puede confirmar una reserva cancelada.";
                return response;
            }

            if (reservation.Status != "pendiente_pago")
            {
                response.Mensaje = "Solo se pueden confirmar reservas con estado pendiente_pago.";
                return response;
            }

            // Generar código único EV-{6 dígitos}
            string reservationCode;
            var random = new Random();
            do
            {
                reservationCode = $"EV-{random.Next(100000, 999999)}";
            } while (await _unitOfWork.Repository.ExistsReservationCodeAsync(reservationCode));

            reservation.Status = "confirmada";
            reservation.ReservationCode = reservationCode;
            reservation.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository.UpdateReservation(reservation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            response.EsExitoso = true;
            response.Mensaje = "Pago confirmado exitosamente.";
            response.Resultado = new DtoReservationResponse
            {
                Id = reservation.Id,
                EventId = reservation.EventId,
                Quantity = reservation.Quantity,
                BuyerName = reservation.BuyerName,
                BuyerEmail = reservation.BuyerEmail,
                Status = reservation.Status,
                ReservationCode = reservation.ReservationCode,
                CreatedAt = reservation.CreatedAt
            };

            return response;
        }
    }
}
