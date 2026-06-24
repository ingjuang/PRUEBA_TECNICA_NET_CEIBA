using EventosVivosBackNet.Application.Commond.Interfaces.Repositories;
using EventosVivosBackNet.Application.Commond.Models.DTOs.DtoBase;
using EventosVivosBackNet.Application.Commond.Models.DTOs.Events;
using MediatR;

namespace EventosVivosBackNet.Application.Features.Events.Queries.GetEventsFiltered
{
    public class GetEventsFilteredQueryHandler : IRequestHandler<GetEventsFilteredQuery, DtoGenericResponse<List<DtoEventResponse>>>
    {
        private readonly IUnitOfWorkDbEventosVivos _unitOfWork;

        public GetEventsFilteredQueryHandler(IUnitOfWorkDbEventosVivos unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DtoGenericResponse<List<DtoEventResponse>>> Handle(GetEventsFilteredQuery request, CancellationToken cancellationToken)
        {
            var events = await _unitOfWork.Repository.GetEventsFilteredAsync(
                request.EventType,
                request.StartDateFrom,
                request.StartDateTo,
                request.VenueId,
                request.Status,
                request.TitleSearch);

            var result = events.Select(e => new DtoEventResponse
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                VenueId = e.VenueId,
                VenueName = e.Venue?.Name ?? string.Empty,
                MaxCapacity = e.MaxCapacity,
                StartAt = e.StartAt,
                EndAt = e.EndAt,
                Price = e.Price,
                EventType = e.EventType,
                Status = DateTime.UtcNow > e.EndAt && e.Status == "activo" ? "completado" : e.Status,
                CreatedAt = e.CreatedAt
            }).ToList();

            return new DtoGenericResponse<List<DtoEventResponse>>
            {
                EsExitoso = true,
                Mensaje = $"Se encontraron {result.Count} evento(s).",
                Resultado = result
            };
        }
    }
}
