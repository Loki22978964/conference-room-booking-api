using System;
using System.Collections.Generic;
using System.Text;
using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Domain.Entities;
using ConferenceBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;

namespace ConferenceBooking.Infrastructure.Repositories;

public class RoomRepository : RepositoryBase<Room>
{
    public RoomRepository(AppDbContext context) : base(context) { }
}