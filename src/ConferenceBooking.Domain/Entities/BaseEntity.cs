using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
}