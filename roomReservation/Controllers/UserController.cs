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
            //Get user data from JWT token
            var user = await _userService.GetUser();

            //Modified data of user
            var result = new
            {
                user.Value!.Email,
                user.Value!.FirstName,
                user.Value!.LastName,
                reservation = user.Value.Reservations!.Select((r) =>
                {
                    return new
                    {
                        r.Id,
                        r.BookingDate,
                        r.Description,
                        r.RoomId,
                    };
                })
            };
            return Ok(result);
        }


        [HttpPut]
        public async Task<ActionResult<User>> UpdateUser(UpdateUserDto request)
        {
            //Get user data from JWT token
            var user = await _userService.GetUser();

            //Update user informaation base on request input
            if (request.Email != "string") user.Value!.Email = request.Email;
            if (request.FirstName != "string") user.Value!.FirstName = request.FirstName;
            if (request.LastName != "string") user.Value!.LastName = request.LastName;

            await _context.SaveChangesAsync();

            var result = new
            {
                user.Value!.Email,
                user.Value!.FirstName,
                user.Value!.LastName,
            };

            return Ok(result);
        }

        [HttpDelete]
        public async Task<ActionResult<string>> DeleteUser()
        {
            //Get user data from JWT token
            var user = await _userService.GetUser();

            //Remove user from the database
            _context.Users.Remove(user.Value!);

            return Ok("User " + user.Value!.Email + " is removed");
        }
    }
}
