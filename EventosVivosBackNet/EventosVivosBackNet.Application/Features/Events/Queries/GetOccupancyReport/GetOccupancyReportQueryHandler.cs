using EventosVivosBackNet.Application.Commond.Interfaces.Repositories;
using EventosVivosBackNet.Application.Commond.Models.DTOs.DtoBase;
using EventosVivosBackNet.Application.Commond.Models.DTOs.Events;
using MediatR;

namespace EventosVivosBackNet.Application.Features.Events.Queries.GetOccupancyReport
{
    public class GetOccupancyReportQueryHandler : IRequestHandler<GetOccupancyReportQuery, DtoGenericResponse<DtoOccupancyReportResponse>>
    {
        private readonly IUnitOfWorkDbEventosVivos _unitOfWork;

        public GetOccupancyReportQueryHandler(IUnitOfWorkDbEventosVivos unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DtoGenericResponse<DtoOccupancyReportResponse>> Handle(GetOccupancyReportQuery request, CancellationToken cancellationToken)
        {
            var response = new DtoGenericResponse<DtoOccupancyReportResponse>();

            var evento = await _unitOfWork.Repository.GetEventByIdAsync(request.EventId);
            if (evento == null)
            {
                response.Mensaje = "El evento especificado no existe.";
                return response;
            }

            // RN-06: Actualizar estado si ya finalizó
            var status = evento.Status;
            if (DateTime.UtcNow > evento.EndAt && status == "activo")
            {
                status = "completado";
                evento.Status = status;
                evento.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository.UpdateEvent(evento);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            var confirmedQuantity = await _unitOfWork.Repository.GetConfirmedReservationsQuantityByEventAsync(request.EventId);
            var lostQuantity = await _unitOfWork.Repository.GetLostQuantityByEventAsync(request.EventId);

            var totalSold = confirmedQuantity;
            var totalAvailable = evento.MaxCapacity - confirmedQuantity - lostQuantity;
            if (totalAvailable < 0) totalAvailable = 0;

            var occupancyPercentage = evento.MaxCapacity > 0
                ? Math.Round((decimal)(confirmedQuantity + lostQuantity) / evento.MaxCapacity * 100, 2)
                : 0;

            var totalRevenue = evento.Price * confirmedQuantity;

            response.EsExitoso = true;
            response.Mensaje = "Reporte de ocupación generado exitosamente.";
            response.Resultado = new DtoOccupancyReportResponse
            {
                EventId = evento.Id,
                Title = evento.Title,
                TotalSold = totalSold,
                TotalAvailable = totalAvailable,
                OccupancyPercentage = occupancyPercentage,
                TotalRevenue = totalRevenue,
                Status = status
            };

            return response;
        }
    }
}
