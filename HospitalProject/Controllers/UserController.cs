using api.Data;
using AutoMapper;
using HospitalProject.Models;
using HospitalProject.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity; // PasswordHasher için
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace HospitalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IMapper _mapper;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly string _nameIdentifier;

        public UserController(ApplicationDBContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _passwordHasher = new PasswordHasher<User>();
            _configuration = configuration;
            _nameIdentifier = _configuration["Jwt:NameIdentifier"];
        }

        [HttpGet]
        [Authorize]
        public ActionResult<IEnumerable<User>> GetUsers()
        {
            return _context.Users.ToList();
        }

        [HttpGet("{id}")]
        [Authorize]
        public ActionResult<User> GetUser(int id)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == _nameIdentifier)?.Value;

            if (userIdClaim == null || !int.TryParse(userIdClaim, out var tokenUserId))
            {
                return Unauthorized("Invalid token.");
            }

            if (tokenUserId != id)
            {
                return Forbid("You can only access your own data.");
            }

            var user = _context.Users.Find(id);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(user);
        }

        [HttpPost]
        public ActionResult<User> CreateUser(UserRequestModel userRequestModel)
        {
            var existingUser = _context.Users
                .FirstOrDefault(u => u.Email == userRequestModel.Email);

            if (existingUser != null)
            {
                return Conflict("This mail already exists.");
            }

            var userEntity = _mapper.Map<User>(userRequestModel);
            userEntity.Password = _passwordHasher.HashPassword(userEntity, userRequestModel.Password);

            _context.Users.Add(userEntity);
            _context.SaveChanges();

            var userRole = new RoleUser
            {
                RoleId = (int)Roles.User,
                UserId = userEntity.Id
            };

            _context.RoleUsers.Add(userRole);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetUser), new { id = userEntity.Id }, new { id = userEntity.Id });
        }

        [HttpPut("{id}")]
        [Authorize]
        public ActionResult<User> UpdateUser(int id, UserUpdateRequestModel updateRequestModel)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == _nameIdentifier)?.Value;

            if (userIdClaim == null || !int.TryParse(userIdClaim, out var tokenUserId))
            {
                return Unauthorized("Invalid token.");
            }

            if (tokenUserId != id)
            {
                return Forbid("You can only update your own data.");
            }

            var user = _context.Users.Find(id);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (!string.IsNullOrEmpty(updateRequestModel.Ad))
            {
                user.Ad = updateRequestModel.Ad;
            }

            if (!string.IsNullOrEmpty(updateRequestModel.Soyad))
            {
                user.Soyad = updateRequestModel.Soyad;
            }

            if (!string.IsNullOrEmpty(updateRequestModel.Password))
            {
                user.Password = _passwordHasher.HashPassword(user, updateRequestModel.Password);
            }

            _context.Users.Update(user);
            _context.SaveChanges();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public ActionResult DeleteUser(int id)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == _nameIdentifier)?.Value;

            if (userIdClaim == null || !int.TryParse(userIdClaim, out var tokenUserId))
            {
                return Unauthorized("Invalid token.");
            }

            if (tokenUserId != id)
            {
                return Forbid("You can only delete your own data.");
            }

            var user = _context.Users.Find(id);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            _context.Users.Remove(user);
            _context.SaveChanges();

            return NoContent();
        }
    }

    public class UserRequestModel
    {
        public string Email { get; set; } = null!;
        public string Ad { get; set; } = null!;
        public string Soyad { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class UserUpdateRequestModel
    {
        public string? Ad { get; set; }
        public string? Soyad { get; set; }
        public string? Password { get; set; }
    }
}
