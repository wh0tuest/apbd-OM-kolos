using apbd_OM_kolos.Models;
using apbd_OM_kolos.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace apbd_OM_kolos.Services;

public class DbService : IDbService
{
    private readonly string _connectionString;

    public DbService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("Default")!;
    }

    public async Task<BookingDto?> GetBookingByIdAsync(int bookingId)
    {
        const string query = @"
            SELECT b.Date,
                   g.FirstName, g.LastName, g.DateOfBirth,
                   e.FirstName, e.LastName, e.EmployeeNumber,
                   a.Name, a.Price, ba.Amount
            FROM Booking b
            JOIN Guest g ON b.IdGuest = g.IdGuest
            JOIN Employee e ON b.EmployeeNumber = e.EmployeeNumber
            LEFT JOIN Booking_Attraction ba ON b.IdBooking = ba.IdBooking
            LEFT JOIN Attraction a ON ba.IdAttraction = a.IdAttraction
            WHERE b.IdBooking = @IdBooking";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@IdBooking", bookingId);

        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();

        BookingDto? result = null;

        while (await reader.ReadAsync())
        {
            if (result == null)
            {
                result = new BookingDto
                {
                    Date = reader.GetDateTime(0),
                    Guest = new GuestDto
                    {
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        DateOfBirth = reader.GetDateTime(3)
                    },
                    Employee = new EmployeeDto
                    {
                        FirstName = reader.GetString(4),
                        LastName = reader.GetString(5),
                        EmployeeNumber = reader.GetString(6)
                    },
                    Attractions = new List<AttractionDto>()
                };
            }

            if (!reader.IsDBNull(7))
            {
                result.Attractions.Add(new AttractionDto
                {
                    Name = reader.GetString(7),
                    Price = reader.GetDecimal(8),
                    Amount = reader.GetInt32(9)
                });
            }
        }

        return result;
    }

    public async Task<BookingResult> AddBookingAsync(AddBookingRequestDto request)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        using var tran = conn.BeginTransaction();

        try
        {
            var checkCmd = new SqlCommand("SELECT COUNT(1) FROM Booking WHERE IdBooking = @Id", conn, tran);
            checkCmd.Parameters.AddWithValue("@Id", request.BookingId);
            var exists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0;
            if (exists)
                return BookingResult.BookingExists;

            var guestCmd = new SqlCommand("SELECT COUNT(1) FROM Guest WHERE IdGuest = @Id", conn, tran);
            guestCmd.Parameters.AddWithValue("@Id", request.GuestId);
            var guestExists = Convert.ToInt32(await guestCmd.ExecuteScalarAsync()) > 0;
            if (!guestExists)
                return BookingResult.GuestNotFound;

            var empCmd = new SqlCommand("SELECT COUNT(1) FROM Employee WHERE EmployeeNumber = @Num", conn, tran);
            empCmd.Parameters.AddWithValue("@Num", request.EmployeeNumber);
            var empExists = Convert.ToInt32(await empCmd.ExecuteScalarAsync()) > 0;
            if (!empExists)
                return BookingResult.EmployeeNotFound;

            var insertBooking = new SqlCommand(@"
                INSERT INTO Booking (IdBooking, Date, IdGuest, EmployeeNumber)
                VALUES (@Id, GETDATE(), @GuestId, @EmpNum);", conn, tran);
            insertBooking.Parameters.AddWithValue("@Id", request.BookingId);
            insertBooking.Parameters.AddWithValue("@GuestId", request.GuestId);
            insertBooking.Parameters.AddWithValue("@EmpNum", request.EmployeeNumber);
            await insertBooking.ExecuteNonQueryAsync();

            foreach (var attraction in request.Attractions)
            {
                var attrIdCmd = new SqlCommand("SELECT IdAttraction FROM Attraction WHERE Name = @Name", conn, tran);
                attrIdCmd.Parameters.AddWithValue("@Name", attraction.Name);
                var attrIdObj = await attrIdCmd.ExecuteScalarAsync();

                if (attrIdObj is null)
                    return BookingResult.AttractionNotFound;

                var attrId = Convert.ToInt32(attrIdObj);

                var insertBA = new SqlCommand(@"
                    INSERT INTO Booking_Attraction (IdBooking, IdAttraction, Amount)
                    VALUES (@Bid, @Aid, @Amount);", conn, tran);
                insertBA.Parameters.AddWithValue("@Bid", request.BookingId);
                insertBA.Parameters.AddWithValue("@Aid", attrId);
                insertBA.Parameters.AddWithValue("@Amount", attraction.Amount);
                await insertBA.ExecuteNonQueryAsync();
            }

            await tran.CommitAsync();
            return BookingResult.Success;
        }
        catch
        {
            await tran.RollbackAsync();
            return BookingResult.UnknownError;
        }
    }
}