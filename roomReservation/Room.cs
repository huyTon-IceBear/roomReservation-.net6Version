using System.Text.Json.Serialization;

namespace roomReservation
{
    public class Room
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public int Floor { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<Reservation>? Reservations { get; set; }
    }
}
