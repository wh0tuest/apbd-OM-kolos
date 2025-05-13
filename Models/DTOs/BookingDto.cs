namespace apbd_OM_kolos.Models.DTOs;

public class BookingDto
{
    public DateTime Date { get; set; }
    public GuestDto Guest { get; set; } = null!;
    public EmployeeDto Employee { get; set; } = null!;
    public List<AttractionDto> Attractions { get; set; } = new();
}
