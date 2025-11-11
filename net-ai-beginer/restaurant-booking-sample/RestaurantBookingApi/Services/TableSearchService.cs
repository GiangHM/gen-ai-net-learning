using Microsoft.EntityFrameworkCore;
using RestaurantBookingApi.Data;
using RestaurantBookingApi.Dto;
using System.Text.Json;

namespace RestaurantBookingApi.Services
{
    public interface ITableSearchService
    {
        Task<IEnumerable<TableModel>> SearchAvailableTablesAsync(DateTime reservationTime, int capacity);
    }
    public class TableSearchService (BookingDataContext _context) : ITableSearchService
    {
        public async Task<IEnumerable<TableModel>> SearchAvailableTablesAsync(DateTime reservationTime, int capacity)
        {
            // Find tables that are not booked at the given reservation time
            var bookedTableIds = await _context.Bookings
                .Where(b => b.ReservationTime == reservationTime)
                .Select(b => b.TableEntitiesId)
                .ToListAsync();

            var availableTables = await _context.Tables
                .Where(t => !bookedTableIds.Contains(t.Id) && t.Capacity >= capacity)
                .Select(t => new TableModel
                {
                    Id = t.Id,
                    Seats = t.Capacity,
                    Description = t.Description
                })
                .ToListAsync();

            return availableTables;
        }
    }
}
