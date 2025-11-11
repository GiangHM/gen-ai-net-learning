namespace RestaurantBookingApi.Dto
{
    public class TableModel
    {
        public int Id { get; set; }
        public required int Seats { get; set; }
        public required string Description { get; set; }
    }
}
