using Microsoft.AspNetCore.RateLimiting;
using LibraryManagementSystem.API.Responses;
using LibraryManagementSystem.API.Models;
using LibraryManagementSystem.API.Data;
using LibraryManagementSystem.API.Dtos;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new ApiResponse<object>(false, "Email already exists.", null));

            var user = new User
            {
                Username = dto.Username,
                Email    = dto.Email,
                Role     = "Member"
            };
            user.PasswordHash = new PasswordHasher<User>().HashPassword(user, dto.Password);

            var member = new Member
            {
                FirstName = dto.FirstName,
                LastName  = dto.LastName,
                Email     = dto.Email,
                User      = user
            };

            _context.Users.Add(user);
            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>(true, "User registered successfully.", new
            {
                username  = user.Username,
                email     = user.Email,
                firstName = dto.FirstName,
                lastName  = dto.LastName,
                role      = user.Role
            }));
        }
[EnableRateLimiting("login")]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)

        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return Unauthorized(new ApiResponse<object>(false, "Invalid email or password.", null));

            var passwordHasher = new PasswordHasher<User>();
            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized(new ApiResponse<object>(false, "Invalid email or password.", null));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new ApiResponse<object>(true, "Login successful.", new
            {
                token    = jwt,
                username = user.Username,
                email    = user.Email,
                role     = user.Role
            }));
        }
    }
}