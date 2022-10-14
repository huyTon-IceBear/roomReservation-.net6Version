﻿using System.Text.Json.Serialization;

namespace roomReservation
{
    public class Reservation
    {
        public int Id { get; set; }
        public DateTime BookingDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public int UserId { get; set; }
        public User? User { get; set; }
        public int RoomId { get; set; }
        public Room? Room { get; set; }
    }
}
