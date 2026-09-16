using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.Application.DTOs;

public class SearchRoomsRequest
{
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int Capacity { get; set; }
}