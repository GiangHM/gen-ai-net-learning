using Microsoft.EntityFrameworkCore;
using RestaurantBookingApi.Data;
using RestaurantBookingApi.Dto;

namespace RestaurantBookingApi.Services
{
    public interface IReservationService
    {
        Task<string> ReserveTableAsync(ReserveTableRequest request);
        Task<List<ReservationResponse>> GetAllReservationsAsync();
    }
    public class ReservationService(BookingDataContext _context) : IReservationService
    {
        public async Task<string> ReserveTableAsync(ReserveTableRequest request)
        {
            // Check if the table exists
            var table = await _context.Tables.FirstOrDefaultAsync(t => t.Id == request.TableId);
            if (table == null)
            {
                return "Table not found. Please provide a valid table ID.";
            }

            // Check if the table is already booked at the given reservation time
            var isBooked = await _context.Bookings
                .AnyAsync(b => b.TableEntitiesId == request.TableId && b.ReservationTime == request.ReservationTime);

            if (isBooked)
            {
                return "This table is already booked at the selected time.";
            }

            // Create a new booking
            var booking = new Booking
            {
                TableEntitiesId = request.TableId,
                ReservationTime = request.ReservationTime,
                CustomerName = request.CustomerName,
                CustomerPhone = request.CustomerPhone,
                NumberOfPeople = request.NumberOfPeople
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return $"Table {request.TableId} reserved successfully for {request.ReservationTime}.";
        }

        public async Task<List<ReservationResponse>> GetAllReservationsAsync()
        {
            return await _context.Bookings
                .Include(b => b.Table)
                .Select(b => new ReservationResponse
                {
                    Id = b.Id,
                    TableNumber = b.Table != null ? b.Table.Number : 0,
                    ReservationTime = b.ReservationTime,
                    NumberOfPeople = b.NumberOfPeople,
                    CustomerName = b.CustomerName,
                    CustomerPhone = b.CustomerPhone
                })
                .ToListAsync();
        }
    }
}
