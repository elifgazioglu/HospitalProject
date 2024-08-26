using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using api.Data;
using HospitalProject.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace HospitalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDBContext context, IConfiguration configuration)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
            _configuration = configuration;
        }

        [HttpPost("login")]
        public ActionResult<string> Login([FromBody] LoginRequestModel model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest("Email and Password are required.");
            }

            var user = _context.Users.SingleOrDefault(u => u.Email == model.Email);

            if (user == null)
            {
                return Unauthorized("Invalid credentials");
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Invalid credentials");
            }

            var token = GenerateJwtToken(user);

            return Ok(new { Token = token });
        }

        private string GenerateJwtToken(User user)
        {
            var roles = _context.RoleUsers
                .Where(ru => ru.UserId == user.Id)
                .Select(ru => ru.Role.Name)
                .ToList();

            var claims = new List<Claim>
    {
            new Claim(_configuration["Jwt:NameIdentifier"], user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }

    public class LoginRequestModel
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
