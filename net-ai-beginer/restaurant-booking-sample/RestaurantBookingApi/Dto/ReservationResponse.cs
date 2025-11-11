namespace RestaurantBookingApi.Dto
{
    public class ReservationResponse
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public DateTime ReservationTime { get; set; }
        public int NumberOfPeople { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
    }
}
