using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace roomReservation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly DataContext _context;
        public RoomController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Room>>> GetFreeRoomsOnDate(DateTime BookingDate)
        {
            var rooms = await _context.Rooms
                .Where(r => !(r!.Reservations!.Any(c => c.BookingDate.Date.Equals(BookingDate.Date))))
                .ToListAsync();
            return Ok(rooms);
        }

        [HttpGet("range"), Authorize]
        public async Task<ActionResult<List<Room>>> GetFreeRoomsOnRange(DateTime StartDate, DateTime EndDate)
        {
            //Check date input is valid
            if (StartDate.Date >= EndDate.Date)
                return NotFound("End date must after the start date");

            //Get all the busy roomid from filter the reservation & start date + end date
            var busyRoomIds = await _context.Reservations
                .Where(r => r.BookingDate.Date >= StartDate.Date && r.BookingDate.Date <= EndDate.Date)
                .Select(r => r.RoomId)
                .Distinct()
                .ToListAsync();
            
            //Get rooms with status
            var rooms = await _context.Rooms
                .Include(c => c.Reservations)
                .Select(r => new {
                    r.Id,
                    r.Reservations,
                    r.Number,
                    r.Floor,
                    status = busyRoomIds.Contains(r.Id) ? "busy" : "free",
                }).ToListAsync();
            
            return Ok(rooms);
        }

    }
}
