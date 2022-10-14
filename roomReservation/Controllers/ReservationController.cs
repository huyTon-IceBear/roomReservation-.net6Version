using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using roomReservation.Service.UserService;

namespace roomReservation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReservationController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IUserService _userService;
        public ReservationController(DataContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Reservation>>> GetResevations()
        {
            //Get user data from JWT token
            var user = await _userService.GetUser();

            //Get all reservations of user
            var reservations = await _context.Reservations
                .Where(c => c.UserId == user!.Value!.Id)
                .Select(r => new
                {
                    r.Id,
                    r.RoomId,
                    r.BookingDate,
                    r.Description,
                })
                .ToListAsync();
            return Ok(reservations);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Reservation>> GetReservationById(int id)
        {
            //Get user data from JWT token
            var user = await _userService.GetUser();

            //Check reservation exist
            var reservation = await _context.Reservations
                .Include(i => i.Room)
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == user.Value!.Id);

            if (reservation == null) return NotFound("Reservation not found");
            return Ok(reservation);
        }

        [HttpPost()]
        public async Task<ActionResult<Reservation>> Create(CreateUpdateReservationDto request)
        {
            //Get user data from JWT token
            var user = await _userService.GetUser();

            // Check room exist
            var room = await _context.Rooms
                .Where(r => r.Id == request.RoomId)
                .Include(r => r.Reservations)
                .FirstOrDefaultAsync();
            if (room == null) return BadRequest("There is no room with id " + request.RoomId);

            // Check if there is a reservation of that room on that date
            var dup = room.Reservations!
                .Exists(r => r.BookingDate.Date.Equals(request.BookingDate.Date));
            if (dup) return BadRequest("Room " + room.Number + " is busy on " + request.BookingDate.Date);

            //Create new reservation and add to the database
            var newReservation = new Reservation
            {
                BookingDate = request.BookingDate,
                Description = request.Description,
                UserId = user.Value!.Id,
                RoomId = room.Id,
                Room = room,
            };

            _context.Reservations.Add(newReservation);
            await _context.SaveChangesAsync();

            var result = new
            {
                newReservation.Id,
                newReservation.BookingDate,
                newReservation.Description,
                newReservation.RoomId,
            };
            return Ok(result);
        }

        [HttpPut]
        public async Task<ActionResult<Reservation>> UpdateReservation(CreateUpdateReservationDto request)
        {
            //Get user data from JWT token
            var user = await _userService.GetUser();

            // Check reservation exist
            var reservation = await _context.Reservations.FindAsync(request.Id);
            if (reservation == null) return BadRequest("Reservation not found");

            // Check room exist
            var room = await _context.Rooms
                .Include(i => i.Reservations)
                .FirstOrDefaultAsync(i => i.Id == request.RoomId);
            if (room == null) return BadRequest("Request room not found");

            // Check if there is a reservation of that room on that date
            var dup = room.Reservations!
                .Exists(r => r.BookingDate.Date.Equals(request.BookingDate.Date));
            if (dup) return BadRequest("Room " + room.Number + " is busy on " + request.BookingDate.Date);

            //Update 
            reservation.BookingDate = request.BookingDate;
            if (request.Description != "") reservation.Description = request.Description;
            reservation.UserId = user.Value!.Id;
            reservation.RoomId = request.RoomId;
            reservation.Room = room;

            await _context.SaveChangesAsync();

            return Ok(reservation);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<List<Reservation>>> DeleteReservation(int id)
        {
            //Get user data from JWT token
            var user = await _userService.GetUser();

            //Check reservation exist
            var reservation = await _context.Reservations.FirstOrDefaultAsync(i => i.Id == id && i.UserId == user.Value!.Id);

            if (reservation == null) return BadRequest("Reservation not found");

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            return Ok(await _context.Reservations.Include(c => c.Room).ToListAsync());
        }
    }
}
