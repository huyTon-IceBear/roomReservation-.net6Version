using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace roomReservation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReservationController : ControllerBase
    {
        private readonly DataContext _context;
        public ReservationController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Reservation>>> GetResevations(int userId)
        {
            var reservations = await _context.Reservations
                .Where(c => c.UserId == userId)
                .Include(c => c.Room)
                .ToListAsync();
            return reservations;
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Reservation>> GetReservationById(int id)
        {
            //Check reservation exist
            var reservation = await _context.Reservations.Include(i => i.Room).FirstOrDefaultAsync(i => i.Id == id);
            if (reservation == null) return NotFound("Reservation not found");
            return Ok(reservation);
        }

        [HttpPost()]
        public async Task<ActionResult<Reservation>> Create(CreateUpdateReservationDto request)
        {
            // Check room exist
            var room = await _context.Rooms.FindAsync(request.RoomId);
            if (room == null) return NotFound("Room not found");
            
            // Check if there is a reservation of that room on that date
            var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.BookingDate.Date.Equals(request.BookingDate.Date));
            if (reservation != null) return BadRequest("Room " + room.Number + " is busy on " + request.BookingDate.Date);

            //Create new reservation and add to the database
            var newReservation = new Reservation
            {
                BookingDate = request.BookingDate,
                Description = request.Description,
                UserId = request.UserId,
                Room = room,
            };

            _context.Reservations.Add(newReservation);
            await _context.SaveChangesAsync();

            return newReservation;
        }

        [HttpPut]
        public async Task<ActionResult<Reservation>> UpdateReservation(CreateUpdateReservationDto request)
        {
            // Check reservation exist
            var reservation = await _context.Reservations.FindAsync(request.Id);
            if (reservation == null) return NotFound("Reservation not found");
            
            // Check room exist
            var room = await _context.Rooms.Include(i => i.Reservations).FirstOrDefaultAsync(i => i.Id == request.RoomId);
            if (room == null) return NotFound("Request room not found");
            
            // Check if there is a reservation of that room on that date
            var dupReservation = await _context.Reservations.FirstOrDefaultAsync(r => r.BookingDate.Date.Equals(request.BookingDate.Date));
            if (dupReservation != null) return BadRequest("Room " + room.Number + " is busy on " + request.BookingDate.Date);

            //Update 
            reservation.BookingDate = request.BookingDate;
            if (request.Description != "") reservation.Description = request.Description;
            reservation.UserId = request.UserId;
            reservation.RoomId = request.RoomId;
            reservation.Room = room;

            await _context.SaveChangesAsync();

            return Ok(reservation);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<List<Reservation>>> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null) return BadRequest("Reservation not found");

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            return Ok(await _context.Reservations.Include(c => c.Room).ToListAsync());
        }
    }
}
