using ConferenceBooking.Application.Services;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.UnitTests.Application.Services;

public class PriceCalculatorTests
{
    [Theory]
    [InlineData(6, 0.9)]
    [InlineData(8, 0.9)]
    [InlineData(9, 1.0)]
    [InlineData(11, 1.0)]
    [InlineData(12, 1.15)]
    [InlineData(13, 1.15)]
    [InlineData(14, 1.0)]
    [InlineData(17, 1.0)]
    [InlineData(18, 0.8)]
    [InlineData(22, 0.8)]
    [InlineData(23, 1.0)]
    [InlineData(2, 1.0)]
    public void GetMultiplierForHour_ReturnsExpectedMultiplier(int hour, double expected)
    {
        var result = PriceCalculator.GetMultiplierForHour(hour);

        result.Should().Be((decimal)expected);
    }

    [Fact]
    public void CalculateRoomCost_SingleHourInStandardWindow_ReturnsBaseRate()
    {
        // 10:00–11:00 Kyiv time -> multiplier 1.0
        var startUtc = new DateTime(2026, 6, 15, 7, 0, 0, DateTimeKind.Utc); // 10:00 Kyiv (UTC+3 summer)
        var endUtc = startUtc.AddHours(1);

        var cost = PriceCalculator.CalculateRoomCost(startUtc, endUtc, baseHourlyRate: 1000m);

        cost.Should().Be(1000m);
    }

    [Fact]
    public void CalculateRoomCost_DiscountWindow_AppliesDiscountMultiplier()
    {
        // 07:00–08:00 Kyiv time -> multiplier 0.9
        var startUtc = new DateTime(2026, 6, 15, 4, 0, 0, DateTimeKind.Utc);
        var endUtc = startUtc.AddHours(1);

        var cost = PriceCalculator.CalculateRoomCost(startUtc, endUtc, baseHourlyRate: 1000m);

        cost.Should().Be(900m);
    }

    [Fact]
    public void CalculateRoomCost_PeakWindow_AppliesSurchargeMultiplier()
    {
        // 12:00–13:00 Kyiv time -> multiplier 1.15
        var startUtc = new DateTime(2026, 6, 15, 9, 0, 0, DateTimeKind.Utc);
        var endUtc = startUtc.AddHours(1);

        var cost = PriceCalculator.CalculateRoomCost(startUtc, endUtc, baseHourlyRate: 1000m);

        cost.Should().Be(1150m);
    }

    [Fact]
    public void CalculateRoomCost_SpanningMultipleRateWindows_SumsEachSegmentProportionally()
    {
        // 08:30–09:30 Kyiv: 30 min at 0.9 (discount) + 30 min at 1.0 (standard)
        var startUtc = new DateTime(2026, 6, 15, 5, 30, 0, DateTimeKind.Utc);
        var endUtc = startUtc.AddHours(1);

        var cost = PriceCalculator.CalculateRoomCost(startUtc, endUtc, baseHourlyRate: 1000m);

        // 0.5h * 1000 * 0.9 + 0.5h * 1000 * 1.0 = 450 + 500 = 950
        cost.Should().Be(950m);
    }

    [Fact]
    public void CalculateRoomCost_ZeroDuration_ReturnsZero()
    {
        var start = new DateTime(2026, 6, 15, 7, 0, 0, DateTimeKind.Utc);

        var cost = PriceCalculator.CalculateRoomCost(start, start, baseHourlyRate: 1000m);

        cost.Should().Be(0m);
    }
}
