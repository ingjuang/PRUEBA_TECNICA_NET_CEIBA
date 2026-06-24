namespace EventosVivosBackNet.Application.Commond.Models.DTOs.Venues
{
    public class DtoVenueResponse
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public int Capacity { get; set; }
        public string City { get; set; } = null!;
    }
}
