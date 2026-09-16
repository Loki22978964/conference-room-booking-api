using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.Application.Services;

public static class PriceCalculator
{
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