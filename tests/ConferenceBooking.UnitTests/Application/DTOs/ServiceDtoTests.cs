using ConferenceBooking.Application.DTOs;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.UnitTests.Application.DTOs;

public class ServiceDtoTests
{
    [Fact]
    public void ServiceDto_SetProperties_StoresValuesCorrectly()
    {
        var dto = new ServiceDto
        {
            Id = Guid.NewGuid(),
            Name = "Wi-Fi",
            Price = 300m
        };

        dto.Id.Should().NotBeEmpty();
        dto.Name.Should().Be("Wi-Fi");
        dto.Price.Should().Be(300m);
    }
}