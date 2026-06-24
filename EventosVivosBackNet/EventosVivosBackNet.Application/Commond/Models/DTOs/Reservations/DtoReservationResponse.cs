namespace EventosVivosBackNet.Application.Commond.Models.DTOs.Reservations
{
    public class DtoReservationResponse
    {
        public long Id { get; set; }
        public long EventId { get; set; }
        public int Quantity { get; set; }
        public string BuyerName { get; set; } = null!;
        public string BuyerEmail { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? ReservationCode { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
