namespace roomReservation
{
    public class CreateUpdateReservationDto
    {
        public int? Id { get; set; }
        public DateTime BookingDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public int RoomId { get; set; } = 1;
    }
}
