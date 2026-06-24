using EventosVivosBackNet.Application.Commond.Interfaces.Repositories;
using EventosVivosBackNet.Application.Commond.Models.DTOs.DtoBase;
using EventosVivosBackNet.Application.Commond.Models.DTOs.Venues;
using MediatR;

namespace EventosVivosBackNet.Application.Features.Venues.Queries.GetAllVenues
{
    public class GetAllVenuesQueryHandler : IRequestHandler<GetAllVenuesQuery, DtoGenericResponse<List<DtoVenueResponse>>>
    {
        private readonly IUnitOfWorkDbEventosVivos _unitOfWork;

        public GetAllVenuesQueryHandler(IUnitOfWorkDbEventosVivos unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DtoGenericResponse<List<DtoVenueResponse>>> Handle(GetAllVenuesQuery request, CancellationToken cancellationToken)
        {
            var venues = await _unitOfWork.Repository.GetAllVenuesAsync();

            var result = venues.Select(v => new DtoVenueResponse
            {
                Id = v.Id,
                Name = v.Name,
                Capacity = v.Capacity,
                City = v.City
            }).ToList();

            return new DtoGenericResponse<List<DtoVenueResponse>>
            {
                EsExitoso = true,
                Mensaje = $"Se encontraron {result.Count} venue(s).",
                Resultado = result
            };
        }
    }
}
