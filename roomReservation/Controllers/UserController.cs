using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using roomReservation.Service.UserService;

namespace roomReservation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class UserController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IUserService _userService;

        public UserController(DataContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<User>> GetPersonalInfor()
        {
            var userEmail = _userService.GetMyName();
            var user = await _context.Users.Include(c => c.Reservations).FirstOrDefaultAsync(c => c.Email == userEmail);
            if (user == null) return BadRequest("User not found");
            return Ok(user);
        }


        [HttpPut]
        public async Task<ActionResult<List<User>>> UpdateUser(User request)
        {
            var user = await _context.Users.FindAsync(request.Id);
            if (user == null) return BadRequest("User not found");

            if (request.Email != "") user.Email = request.Email;
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
