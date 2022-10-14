using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace roomReservation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DataContext _context;

        public UserController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> Get(int id)
        {
            var user = await _context.Users.Include(c => c.Reservations).FirstOrDefaultAsync(c => c.Id == id);
            if (user == null) return BadRequest("User not found");
            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<User>> AddUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        [HttpPut]
        public async Task<ActionResult<List<User>>> UpdateUser(User request)
        {
            var user = await _context.Users.FindAsync(request.Id);
            if (user == null) return BadRequest("User not found");

            if (request.Name != "") user.Name = request.Name;
            if (request.FirstName != "") user.FirstName = request.FirstName;
            if (request.LastName != "") user.LastName = request.LastName;

            await _context.SaveChangesAsync();
            return Ok(await _context.Users.ToListAsync());
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<User>> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return BadRequest("User not found");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }
    }
}
