namespace apbd_OM_kolos.Models;

public enum BookingResult
{
    Success,
    BookingExists,
    GuestNotFound,
    EmployeeNotFound,
    AttractionNotFound,
    ValidationError,
    UnknownError
}