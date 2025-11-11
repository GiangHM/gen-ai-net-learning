namespace RestaurantBookingApi.Dto
{
    public class ReserveTableRequest
    {
        public int TableId { get; set; }
        public DateTime ReservationTime { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public int NumberOfPeople { get; set; }
    }
}
