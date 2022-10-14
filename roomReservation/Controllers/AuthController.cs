using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using roomReservation.Service.UserService;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace roomReservation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        public AuthController(DataContext context, IConfiguration configuration, IUserService userService)
        {
            _context = context;
            _configuration = configuration;
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserDto request)
        {
            //Check if any user have the same email
            var findDup = await _context.Users.FirstOrDefaultAsync(r => r.Email == request.Email);

            //If there is a dupplicate result, return error
            if (findDup != null) return BadRequest("You can not use this email to create account");

            //Create password hash 
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            //Create new user with password Hash and password Salt
            User user = new User
            {
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
            };

            //Add user to database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserDto request)
        {
            //Find user by request email
            var user = await _context.Users.FirstOrDefaultAsync(r => r.Email == request.Email);
            
            //If user not found, return error
            if (user == null) return BadRequest("User does not exist");
            
            //Verify user password
            if(!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("Wrong password");
            }

            //Create and return token
            string token = CreateToken(user);
            return Ok(token);
        }

        private string CreateToken(User user)
        {
            //Create claim
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email)
            };

            //Create security key
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                           _configuration.GetSection("AppSettings:Token").Value));

            //Signing credentials
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            //Create jwt token
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            //Create password salt and password hash
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt) 
        {
            using(var hmac = new HMACSHA512(passwordSalt))
            {
                //Create a hash from given passwordSalt and password
                var computeHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                //Compare hash to check password
                return computeHash.SequenceEqual(passwordHash);
            }
        }
    }
}
