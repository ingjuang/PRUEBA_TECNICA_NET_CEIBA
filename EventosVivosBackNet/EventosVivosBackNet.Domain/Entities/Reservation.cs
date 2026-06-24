using System;
using System.Collections.Generic;

namespace EventosVivosBackNet.Domain.Entities;

public partial class Reservation
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

    public DateTime UpdatedAt { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual ReservationLoss? ReservationLoss { get; set; }
}
