using ConferenceBooking.Application.DTOs;
using ConferenceBooking.Application.Exceptions;
using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Application.Specifications;
using ConferenceBooking.Domain.Entities;
using FluentAssertions;
using Moq;
using BookingService = ConferenceBooking.Application.Services.BookingService;

namespace ConferenceBooking.UnitTests.Application.Services;

public class BookingServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
    private readonly Mock<IBookingRepository> _bookingRepositoryMock = new();

    public BookingServiceTests()
    {
        _unitOfWorkMock.Setup(u => u.Rooms).Returns(_roomRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Bookings).Returns(_bookingRepositoryMock.Object);
    }

    private BookingService CreateSut() => new(_unitOfWorkMock.Object);

    private static CreateBookingRequest CreateRequest(Guid roomId, List<Guid>? serviceIds = null) => new()
    {
        RoomId = roomId,
        StartDateTimeUtc = DateTime.UtcNow.AddDays(1), // завжди в майбутньому відносно моменту запуску тесту
        DurationHours = 2,
        ServiceIds = serviceIds ?? new List<Guid>()
    };

    [Fact]
    public async Task BookRoomAsync_WhenRoomNotFound_ReturnsRoomNotFound_DoesNotAddBooking()
    {
        _roomRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<RoomByIdWithServicesSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Room?)null);

        var sut = CreateSut();

        var result = await sut.BookRoomAsync(CreateRequest(Guid.NewGuid()));

        result.Should().BeOfType<BookingResult.RoomNotFound>();
        _bookingRepositoryMock.Verify(b => b.Add(It.IsAny<Booking>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BookRoomAsync_WhenRoomFoundAndNoOverlap_ReturnsSuccessWithCalculatedPrice()
    {
        var room = new Room("Hall A", 50, 1000m); // rate that keeps math simple

        _roomRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<RoomByIdWithServicesSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        var sut = CreateSut();

        var result = await sut.BookRoomAsync(CreateRequest(room.Id));

        result.Should().BeOfType<BookingResult.Success>();
        var success = (BookingResult.Success)result;
        success.TotalPrice.Should().BeGreaterThan(0);

        _bookingRepositoryMock.Verify(b => b.Add(It.Is<Booking>(bk => bk.RoomId == room.Id)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BookRoomAsync_WhenSaveChangesThrowsOverlappingBookingException_ReturnsOverlapping()
    {
        var room = new Room("Hall C", 50, 1000m);

        _roomRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<RoomByIdWithServicesSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OverlappingBookingException());

        var sut = CreateSut();

        var result = await sut.BookRoomAsync(CreateRequest(room.Id));

        result.Should().BeOfType<BookingResult.Overlapping>();
    }
}