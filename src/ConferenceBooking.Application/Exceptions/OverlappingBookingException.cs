using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.Application.Exceptions;

/// <summary>
/// Thrown when a booking cannot be saved because it overlaps with an existing
/// booking for the same room. Raised by the Infrastructure layer when the
/// database's exclusion constraint (enforced at the PostgreSQL level via the
/// <c>btree_gist</c> extension) rejects the insert, and translated here into a
/// domain-friendly exception so the Application layer never depends on
/// EF Core or Npgsql-specific error types.
/// </summary>
public class OverlappingBookingException : Exception
{
    /// <summary>
    /// Initializes a new instance with a default, user-facing message.
    /// </summary>
    public OverlappingBookingException()
        : base("The room is already booked for the selected time slot.") { }
}
