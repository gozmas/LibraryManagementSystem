
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using LibraryManagementSystem.API.Data;
using LibraryManagementSystem.DTOs;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;


namespace LibraryManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

       public AuthController(
    AppDbContext context,
    IConfiguration configuration)
{
    _context = context;
    _configuration = configuration;
}

        [HttpPost("register")]
        public IActionResult Register(RegisterDto dto)
        {
            if (_context.Users.Any(u => u.Email == dto.Email))
            {
                return BadRequest("Email already exists.");
            }

           var user = new User
{
    Username = dto.Username,
    Email = dto.Email,
    Role = dto.Role
};

user.PasswordHash = new PasswordHasher<User>().HashPassword(user, dto.Password);

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(new
            {
                message = "User registered successfully."
            });
        }
        [HttpPost("login")]
public IActionResult Login(LoginDto dto)
{
    var user = _context.Users
        .FirstOrDefault(u => u.Email == dto.Email);

    if (user == null)
    {
        return Unauthorized("Invalid email or password.");
    }

    var passwordHasher = new PasswordHasher<User>();

    var result = passwordHasher.VerifyHashedPassword(
        user,
        user.PasswordHash,
        dto.Password);

    if (result == PasswordVerificationResult.Failed)
    {
        return Unauthorized("Invalid email or password.");
    }

    var claims = new[]
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Name, user.Username),
    new Claim(ClaimTypes.Email, user.Email),
    new Claim(ClaimTypes.Role, user.Role)
};

var key = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

var creds = new SigningCredentials(
    key,
    SecurityAlgorithms.HmacSha256);

var token = new JwtSecurityToken(
    issuer: _configuration["Jwt:Issuer"],
    audience: _configuration["Jwt:Audience"],
    claims: claims,
    expires: DateTime.Now.AddHours(2),
    signingCredentials: creds);

var jwt = new JwtSecurityTokenHandler()
    .WriteToken(token);

return Ok(new
{
    token = jwt,
    username = user.Username,
    email = user.Email,
    role = user.Role
});
}
    }
}