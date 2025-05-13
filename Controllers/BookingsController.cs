using apbd_OM_kolos.Models;
using apbd_OM_kolos.Models.DTOs;
using apbd_OM_kolos.Services;
using Microsoft.AspNetCore.Mvc;

namespace apbd_OM_kolos.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingsController : ControllerBase
{
    private readonly IDbService _dbService;

    public BookingsController(IDbService dbService)
    {
        _dbService = dbService;
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBooking(int id)
    {
        var booking = await _dbService.GetBookingByIdAsync(id);

        if (booking is null)
            return NotFound($"Booking with id {id} not found.");

        return Ok(booking);
    }
    
    [HttpPost]
    public async Task<IActionResult> AddBooking(AddBookingRequestDto request)
    {
        var result = await _dbService.AddBookingAsync(request);

        return result switch
        {
            BookingResult.Success => CreatedAtAction(nameof(GetBooking), new { id = request.BookingId }, null),
            BookingResult.BookingExists => Conflict("Booking with this ID already exists."),
            BookingResult.GuestNotFound => NotFound("Guest with this ID does not exist."),
            BookingResult.EmployeeNotFound => NotFound("Employee with this number does not exist."),
            BookingResult.AttractionNotFound => NotFound("One or more attractions do not exist."),
            BookingResult.ValidationError => BadRequest("Invalid input data."),
            _ => StatusCode(500, "Unexpected error.")
        };
    }
}