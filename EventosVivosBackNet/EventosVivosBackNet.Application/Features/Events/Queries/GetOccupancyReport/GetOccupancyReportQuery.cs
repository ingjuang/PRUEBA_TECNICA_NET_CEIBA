using EventosVivosBackNet.Application.Commond.Models.DTOs.DtoBase;
using EventosVivosBackNet.Application.Commond.Models.DTOs.Events;
using MediatR;

namespace EventosVivosBackNet.Application.Features.Events.Queries.GetOccupancyReport
{
    public class GetOccupancyReportQuery : IRequest<DtoGenericResponse<DtoOccupancyReportResponse>>
    {
        public long EventId { get; set; }
    }
}
