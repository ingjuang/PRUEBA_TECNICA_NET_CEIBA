using EventosVivosBackNet.Application.Commond.Interfaces.Repositories;
using EventosVivosBackNet.Application.Commond.Models.DTOs.DtoBase;
using EventosVivosBackNet.Application.Commond.Models.DTOs.Events;
using EventosVivosBackNet.Domain.Entities;
using MediatR;

namespace EventosVivosBackNet.Application.Features.Events.Commands.CreateEvent
{
    public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, DtoGenericResponse<DtoEventResponse>>
    {
        private readonly IUnitOfWorkDbEventosVivos _unitOfWork;

        public CreateEventCommandHandler(IUnitOfWorkDbEventosVivos unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DtoGenericResponse<DtoEventResponse>> Handle(CreateEventCommand request, CancellationToken cancellationToken)
        {
            var response = new DtoGenericResponse<DtoEventResponse>();

            // Validar título
            if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length < 5 || request.Title.Length > 100)
            {
                response.Mensaje = "El título es obligatorio y debe tener entre 5 y 100 caracteres.";
                return response;
            }

            // Validar descripción
            if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length < 10 || request.Description.Length > 500)
            {
                response.Mensaje = "La descripción es obligatoria y debe tener entre 10 y 500 caracteres.";
                return response;
            }

            // Validar tipo de evento
            var tiposValidos = new[] { "conferencia", "taller", "concierto" };
            if (!tiposValidos.Contains(request.EventType))
            {
                response.Mensaje = "El tipo de evento debe ser: conferencia, taller o concierto.";
                return response;
            }

            // Validar fechas
            if (request.StartAt <= DateTime.UtcNow)
            {
                response.Mensaje = "La fecha de inicio debe ser futura.";
                return response;
            }

            if (request.EndAt <= request.StartAt)
            {
                response.Mensaje = "La fecha de fin debe ser posterior a la fecha de inicio.";
                return response;
            }

            // Validar precio
            if (request.Price <= 0)
            {
                response.Mensaje = "El precio debe ser un decimal positivo.";
                return response;
            }

            // Validar capacidad
            if (request.MaxCapacity <= 0)
            {
                response.Mensaje = "La capacidad máxima debe ser un entero positivo.";
                return response;
            }

            // RN-03: Restricción de horario nocturno
            var dayOfWeek = request.StartAt.DayOfWeek;
            if ((dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday) && request.StartAt.Hour >= 22)
            {
                response.Mensaje = "Los eventos en fines de semana no pueden iniciar después de las 22:00.";
                return response;
            }

            // Validar venue
            var venue = await _unitOfWork.Repository.GetVenueByIdAsync(request.VenueId);
            if (venue == null)
            {
                response.Mensaje = "El venue especificado no existe.";
                return response;
            }

            // RN-01: Capacidad del venue
            if (request.MaxCapacity > venue.Capacity)
            {
                response.Mensaje = $"La capacidad máxima no puede exceder la capacidad del venue ({venue.Capacity}).";
                return response;
            }

            // RN-02: Superposición de venues
            var hasOverlap = await _unitOfWork.Repository.ExistsOverlappingEventAsync(request.VenueId, request.StartAt, request.EndAt);
            if (hasOverlap)
            {
                response.Mensaje = "Ya existe un evento activo en este venue con horarios superpuestos.";
                return response;
            }

            var newEvent = new Event
            {
                Title = request.Title,
                Description = request.Description,
                VenueId = request.VenueId,
                MaxCapacity = request.MaxCapacity,
                StartAt = request.StartAt,
                EndAt = request.EndAt,
                Price = request.Price,
                EventType = request.EventType,
                Status = "activo",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository.AddEventAsync(newEvent);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            response.EsExitoso = true;
            response.Mensaje = "Evento creado exitosamente.";
            response.Resultado = new DtoEventResponse
            {
                Id = newEvent.Id,
                Title = newEvent.Title,
                Description = newEvent.Description,
                VenueId = newEvent.VenueId,
                VenueName = venue.Name,
                MaxCapacity = newEvent.MaxCapacity,
                StartAt = newEvent.StartAt,
                EndAt = newEvent.EndAt,
                Price = newEvent.Price,
                EventType = newEvent.EventType,
                Status = newEvent.Status,
                CreatedAt = newEvent.CreatedAt
            };

            return response;
        }
    }
}
