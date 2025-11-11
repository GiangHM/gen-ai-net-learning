using Microsoft.AspNetCore.Mvc;
using RestaurantBookingApi.Dto;
using RestaurantBookingApi.Services;

namespace RestaurantBookingApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RestaurantBookingController : ControllerBase
    {

        private readonly ILogger<RestaurantBookingController> _logger;
        private readonly ITableSearchService _tableSearchService;
        private readonly IReservationService _reservationService;
        public RestaurantBookingController(ILogger<RestaurantBookingController> logger
            , ITableSearchService tableService
            , IReservationService reservationService)
        {
            _logger = logger;
            _tableSearchService = tableService;
            _reservationService = reservationService;
        }

        [HttpGet("AvailableTable")]
        public async Task<IEnumerable<TableModel>> Get(DateTime? reservationTime, int capacity)
        {
            _logger.LogInformation("Searching for available tables at {ReservationTime} for {Capacity} people.", reservationTime, capacity);
            if(reservationTime == null)
                reservationTime = DateTime.Now;
            return await _tableSearchService.SearchAvailableTablesAsync(reservationTime ?? DateTime.Now, capacity);
        }

        [HttpPost("ReserveTable")]
        public async Task<IActionResult> ReserveTable([FromBody] ReserveTableRequest reservationRequest)
        {
            if (reservationRequest == null)
            {
                return BadRequest("Reservation request is required.");
            }

            try
            {
                var reservationResult = await _reservationService.ReserveTableAsync(reservationRequest);
                return Ok(reservationResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reserving table.");
                return StatusCode(500, "An error occurred while reserving the table.");
            }
        }

        [HttpGet("GetReservedTable")]
        public async Task<IActionResult> GetReservedTable() {
            try
            {
                var reservations = await _reservationService.GetAllReservationsAsync();
                return Ok(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservations.");
                return StatusCode(500, "An error occurred while retrieving reservations.");
            }
        }

    }
}
