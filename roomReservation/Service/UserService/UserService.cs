using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace roomReservation.Service.UserService
{
    public class UserService :IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DataContext _context;

        public UserService(DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<ActionResult<User>> GetUser()
        {
            var result = string.Empty;
            if(_httpContextAccessor.HttpContext != null)
            {
                result = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Email);
            }

            var user = await _context.Users
                .Where(c => c.Email == result)
                .Include(c => c.Reservations)
                .FirstOrDefaultAsync();

            if (user == null) return BadRequest("User does not exist");
            return user;
        }

        private ActionResult<User> BadRequest(string v)
        {
            throw new NotImplementedException();
        }
    }
}
