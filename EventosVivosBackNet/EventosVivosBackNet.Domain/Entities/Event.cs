using System;
using System.Collections.Generic;

namespace EventosVivosBackNet.Domain.Entities;

public partial class Event
{
    public long Id { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public long VenueId { get; set; }

    public int MaxCapacity { get; set; }

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public decimal Price { get; set; }

    public string EventType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<ReservationLoss> ReservationLosses { get; set; } = new List<ReservationLoss>();

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    public virtual Venue Venue { get; set; } = null!;
}
