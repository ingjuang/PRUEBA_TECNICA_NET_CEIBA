namespace EventosVivosBackNet.Application.Commond.Models.DTOs.Events
{
    public class DtoOccupancyReportResponse
    {
        public long EventId { get; set; }
        public string Title { get; set; } = null!;
        public int TotalSold { get; set; }
        public int TotalAvailable { get; set; }
        public decimal OccupancyPercentage { get; set; }
        public decimal TotalRevenue { get; set; }
        public string Status { get; set; } = null!;
    }
}
