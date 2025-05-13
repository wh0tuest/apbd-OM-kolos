using apbd_OM_kolos.Models;
using apbd_OM_kolos.Models.DTOs;

namespace apbd_OM_kolos.Services;

public interface IDbService
{
    Task<BookingDto?> GetBookingByIdAsync(int bookingId);
    Task<BookingResult> AddBookingAsync(AddBookingRequestDto request);
}