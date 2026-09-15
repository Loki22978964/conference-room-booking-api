using ConferenceBooking.Application.DTOs;
using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Application.Specifications;
using ConferenceBooking.Domain.Entities;
using FluentAssertions;
using Moq;
using RoomService = ConferenceBooking.Application.Services.RoomService;

namespace ConferenceBooking.UnitTests.Application.Services;

public class RoomServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IRoomRepository> _roomRepositoryMock = new();

    public RoomServiceTests()
    {
        _unitOfWorkMock.Setup(u => u.Rooms).Returns(_roomRepositoryMock.Object);
    }

    private RoomService CreateSut() => new(_unitOfWorkMock.Object);

    [Fact]
    public async Task CreateRoomAsync_AddsRoomAndSavesChanges_ReturnsNewRoomId()
    {
        var request = new CreateOrUpdateRoomRequest
        {
            Name = "Hall A",
            Capacity = 50,
            BaseHourlyRate = 1000m
        };

        var sut = CreateSut();

        var roomId = await sut.CreateRoomAsync(request);

        roomId.Should().NotBeEmpty();
        _roomRepositoryMock.Verify(r => r.Add(It.Is<Room>(room =>
            room.Name == "Hall A" &&
            room.Capacity == 50 &&
            room.BaseHourlyRate == 1000m)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetRoomAsync_WhenRoomExists_ReturnsMappedDto()
    {
        var room = new Room("Hall B", 100, 2000m);

        _roomRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<RoomByIdWithServicesSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        var sut = CreateSut();

        var dto = await sut.GetRoomAsync(room.Id);

        dto.Should().NotBeNull();
        dto!.Id.Should().Be(room.Id);
        dto.Name.Should().Be("Hall B");
        dto.Capacity.Should().Be(100);
        dto.Services.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRoomAsync_WhenRoomDoesNotExist_ReturnsNull()
    {
        _roomRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<RoomByIdWithServicesSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Room?)null);

        var sut = CreateSut();

        var dto = await sut.GetRoomAsync(Guid.NewGuid());

        dto.Should().BeNull();
    }

    [Fact]
    public async Task UpdateRoomAsync_WhenRoomExists_UpdatesDetailsAndSaves_ReturnsTrue()
    {
        var room = new Room("Old Name", 10, 500m);
        _roomRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<RoomByIdWithServicesSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        var request = new CreateOrUpdateRoomRequest
        {
            Name = "New Name",
            Capacity = 20,
            BaseHourlyRate = 1500m
        };

        var sut = CreateSut();

        var result = await sut.UpdateRoomAsync(room.Id, request);

        result.Should().BeTrue();
        room.Name.Should().Be("New Name");
        room.Capacity.Should().Be(20);
        room.BaseHourlyRate.Should().Be(1500m);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateRoomAsync_WhenRoomDoesNotExist_ReturnsFalse_DoesNotSave()
    {
        _roomRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<RoomByIdWithServicesSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Room?)null);

        var sut = CreateSut();

        var result = await sut.UpdateRoomAsync(Guid.NewGuid(), new CreateOrUpdateRoomRequest
        {
            Name = "X",
            Capacity = 1,
            BaseHourlyRate = 1m
        });

        result.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteRoomAsync_WhenRoomExists_MarksAsDeletedAndSaves_ReturnsTrue()
    {
        var room = new Room("Hall C", 30, 800m);
        _roomRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<RoomByIdSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        var sut = CreateSut();

        var result = await sut.DeleteRoomAsync(room.Id);

        result.Should().BeTrue();
        room.IsActive.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteRoomAsync_WhenRoomDoesNotExist_ReturnsFalse()
    {
        _roomRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<RoomByIdSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Room?)null);

        var sut = CreateSut();

        var result = await sut.DeleteRoomAsync(Guid.NewGuid());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAvailableRoomsAsync_ReturnsMappedDtos_ForRoomsReturnedByRepository()
    {
        var room1 = new Room("Hall D", 40, 900m);
        var room2 = new Room("Hall E", 60, 1200m);

        _roomRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<AvailableRoomsSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Room> { room1, room2 });

        var request = new SearchRoomsRequest
        {
            Date = new DateTime(2026, 6, 15),
            StartTime = TimeSpan.FromHours(10),
            EndTime = TimeSpan.FromHours(12),
            Capacity = 30
        };

        var sut = CreateSut();

        var result = await sut.GetAvailableRoomsAsync(request);

        result.Should().HaveCount(2);
        result.Select(r => r.Id).Should().Contain(new[] { room1.Id, room2.Id });
    }
}