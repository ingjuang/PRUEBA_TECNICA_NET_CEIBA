namespace EventosVivosBackNet.Application.Commond.Models.DTOs.Events
{
    public class DtoEventResponse
    {
        public long Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public long VenueId { get; set; }
        public string VenueName { get; set; } = null!;
        public int MaxCapacity { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public decimal Price { get; set; }
        public string EventType { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
