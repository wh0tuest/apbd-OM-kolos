namespace apbd_OM_kolos.Models.DTOs;

public class AddBookingRequestDto
{
    public int BookingId { get; set; }
    public int GuestId { get; set; }
    public string EmployeeNumber { get; set; } = null!;
    public List<AttractionAmountDto> Attractions { get; set; } = new();
}
