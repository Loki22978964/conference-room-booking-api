using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.Application.Services;

/// <summary>
/// Calculates the dynamic cost of booking a room based on the time of day,
/// applying different rate multipliers for discount, peak, and standard hours.
/// </summary>
public static class PriceCalculator
{
    /// <summary>
    /// Calculates the total cost of a room booking by splitting the requested
    /// time range into hourly segments (aligned to local wall-clock hours) and
    /// applying the appropriate rate multiplier to each segment.
    /// </summary>
    /// <param name="startUtc">The booking's start time, in UTC.</param>
    /// <param name="endUtc">The booking's end time, in UTC.</param>
    /// <param name="baseHourlyRate">The room's base hourly rate before multipliers.</param>
    /// <returns>The total calculated cost for the room over the given time range.</returns>
    /// <remarks>
    /// Time-of-day multipliers are applied based on Kyiv local time
    /// (<c>FLE Standard Time</c>), not UTC. A booking spanning multiple rate
    /// windows (e.g. crossing from the morning discount into the standard rate)
    /// is billed proportionally for the fraction of the hour spent in each window.
    /// </remarks>
    public static decimal CalculateRoomCost(DateTime startUtc, DateTime endUtc, decimal baseHourlyRate)
    {
        var kyivZone = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
        var startLocal = TimeZoneInfo.ConvertTimeFromUtc(startUtc, kyivZone);
        var endLocal = TimeZoneInfo.ConvertTimeFromUtc(endUtc, kyivZone);

        decimal totalCost = 0;

        var current = startLocal;

        while (current < endLocal)
        {
            var nextHour = new DateTime(current.Year, current.Month, current.Day, current.Hour, 0, 0, current.Kind).AddHours(1);
            var stepEnd = nextHour < endLocal ? nextHour : endLocal;

            var hoursInStep = (decimal)(stepEnd - current).TotalHours;
            var multiplier = GetMultiplierForHour(current.Hour);

            totalCost += baseHourlyRate * multiplier * hoursInStep;

            current = stepEnd;
        }

        return totalCost;
    }

    /// <summary>
    /// Returns the rate multiplier that applies for a given local hour of day.
    /// </summary>
    /// <param name="hour">The local hour, in 24-hour format (0–23).</param>
    /// <returns>
    /// <c>0.9</c> for the morning discount (06:00–09:00), <c>1.15</c> for the
    /// midday peak surcharge (12:00–14:00), <c>0.8</c> for the evening discount
    /// (18:00–23:00), and <c>1.0</c> for all standard and nighttime hours.
    /// </returns>
    public static decimal GetMultiplierForHour(int hour)
    {
        return hour switch
        {
            >= 6 and < 9 => 0.9m,    // 10% discount from 06:00 to 09:00
            >= 12 and < 14 => 1.15m, // Пікові 12:00 - 14:00  націнка 15%
            >= 18 and < 23 => 0.8m,  // 20% discount in the evening (18:00–23:00)
            >= 9 and < 12 => 1.0m,   // Standard 09:00 – 12:00
            >= 14 and < 18 => 1.0m,  // Standard 14:00 - 18:00
            _ => 1.0m                // Default nighttime setting (basic)
        };
    }
}