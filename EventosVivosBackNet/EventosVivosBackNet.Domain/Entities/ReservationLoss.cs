using System;
using System.Collections.Generic;

namespace EventosVivosBackNet.Domain.Entities;

public partial class ReservationLoss
{
    public long Id { get; set; }

    public long ReservationId { get; set; }

    public long EventId { get; set; }

    public int Quantity { get; set; }

    public string Reason { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual Reservation Reservation { get; set; } = null!;
}
