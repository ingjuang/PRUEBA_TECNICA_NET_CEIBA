using System;
using System.Collections.Generic;

namespace EventosVivosBackNet.Domain.Entities;

public partial class Venue
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public int Capacity { get; set; }

    public string City { get; set; } = null!;

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}
