using System.Text.Json.Serialization;

namespace roomReservation
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public List<Reservation>? Reservations { get; set; }
    }
}
