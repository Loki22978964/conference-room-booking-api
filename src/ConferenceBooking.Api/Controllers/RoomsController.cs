using ConferenceBooking.Application.Interfaces;

namespace ConferenceBooking.Api.Controllers;

public class RoomsController
{
    private readonly IUnitOfWork _unitOfWork;

    public RoomsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
}
