using System.Text.RegularExpressions;
using EventosVivosBackNet.Application.Commond.Interfaces.Repositories;
using EventosVivosBackNet.Application.Commond.Models.DTOs.DtoBase;
using EventosVivosBackNet.Application.Commond.Models.DTOs.Reservations;
using EventosVivosBackNet.Domain.Entities;
using MediatR;

namespace EventosVivosBackNet.Application.Features.Reservations.Commands.CreateReservation
{
    public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, DtoGenericResponse<DtoReservationResponse>>
    {
        private readonly IUnitOfWorkDbEventosVivos _unitOfWork;

        public CreateReservationCommandHandler(IUnitOfWorkDbEventosVivos unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DtoGenericResponse<DtoReservationResponse>> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
        {
            var response = new DtoGenericResponse<DtoReservationResponse>();

            // Validar cantidad
            if (request.Quantity < 1)
            {
                response.Mensaje = "La cantidad debe ser 1 o más.";
                return response;
            }

            // Validar email
            if (string.IsNullOrWhiteSpace(request.BuyerEmail) || !Regex.IsMatch(request.BuyerEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                response.Mensaje = "El email no tiene un formato válido.";
                return response;
            }

            // Validar nombre
            if (string.IsNullOrWhiteSpace(request.BuyerName))
            {
                response.Mensaje = "El nombre del comprador es obligatorio.";
                return response;
            }

            // Obtener evento
            var evento = await _unitOfWork.Repository.GetEventByIdAsync(request.EventId);
            if (evento == null)
            {
                response.Mensaje = "El evento especificado no existe.";
                return response;
            }

            if (evento.Status != "activo")
            {
                response.Mensaje = "Solo se pueden reservar entradas para eventos activos.";
                return response;
            }

            // RN-06: Marcar completado si ya pasó
            if (DateTime.UtcNow > evento.EndAt)
            {
                evento.Status = "completado";
                evento.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository.UpdateEvent(evento);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                response.Mensaje = "El evento ya ha finalizado.";
                return response;
            }

            // RN-04: No permitir reservas si el evento inicia en menos de 1 hora
            var horasParaInicio = (evento.StartAt - DateTime.UtcNow).TotalHours;
            if (horasParaInicio < 1)
            {
                response.Mensaje = "No se permiten reservas para eventos que inicien en menos de 1 hora.";
                return response;
            }

            // RF-03: Si faltan menos de 24 horas, máximo 5 entradas (prioridad sobre RN-05)
            if (horasParaInicio < 24 && request.Quantity > 5)
            {
                response.Mensaje = "Para eventos con menos de 24 horas para iniciar, solo se permiten máximo 5 entradas por transacción.";
                return response;
            }

            // RN-05: Eventos con precio > $100 limitan a máximo 10 entradas
            if (horasParaInicio >= 24 && evento.Price > 100 && request.Quantity > 10)
            {
                response.Mensaje = "Eventos con precio mayor a $100 limitan a máximo 10 entradas por transacción.";
                return response;
            }

            // Validar disponibilidad
            var reservedQuantity = await _unitOfWork.Repository.GetConfirmedReservationsQuantityByEventAsync(request.EventId);
            var lostQuantity = await _unitOfWork.Repository.GetLostQuantityByEventAsync(request.EventId);
            var available = evento.MaxCapacity - reservedQuantity - lostQuantity;

            if (request.Quantity > available)
            {
                response.Mensaje = $"No hay suficientes entradas disponibles. Disponibles: {available}.";
                return response;
            }

            var reservation = new Reservation
            {
                EventId = request.EventId,
                Quantity = request.Quantity,
                BuyerName = request.BuyerName,
                BuyerEmail = request.BuyerEmail,
                Status = "pendiente_pago",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository.AddReservationAsync(reservation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            response.EsExitoso = true;
            response.Mensaje = "Reserva creada exitosamente con estado pendiente_pago.";
            response.Resultado = new DtoReservationResponse
            {
                Id = reservation.Id,
                EventId = reservation.EventId,
                Quantity = reservation.Quantity,
                BuyerName = reservation.BuyerName,
                BuyerEmail = reservation.BuyerEmail,
                Status = reservation.Status,
                CreatedAt = reservation.CreatedAt
            };

            return response;
        }
    }
}
