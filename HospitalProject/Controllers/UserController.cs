using api.Data;
using AutoMapper;
using HospitalProject.Models;
using HospitalProject.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity; // PasswordHasher için
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public UserController(ApplicationDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _passwordHasher = new PasswordHasher<User>();
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
            var validation = new IntValidator();
            var validationResult = validation.Validate(id);

            if (validationResult == null)
            {
                return BadRequest(validationResult);
            }

            var user = _context.Users.Find(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        [HttpPost]
        public ActionResult<User> CreateUser(UserRequestModel userRequestModel)
        {
            var existingUser = _context.Users
            .FirstOrDefault(u => u.Email == userRequestModel.Email);

            if (existingUser != null)
            {
                // E-posta adresi zaten kullanılıyor
                return Conflict("This mail is already exist");
            }
            var userEntity = _mapper.Map<User>(userRequestModel);

            // Şifreyi hashle
            var hashedPassword = _passwordHasher.HashPassword(userEntity, userRequestModel.Password);
            userEntity.Password = hashedPassword;

            _context.Users.Add(userEntity);
            _context.SaveChanges();


            var userRole = new RoleUser
            {
                RoleId = (int)Roles.User,
                UserId = userEntity.Id
            };

            _context.RoleUsers.Add(userRole);
            _context.SaveChanges();

            var results = new
            {
                id = userEntity.Id,
            };

            return Created($"CreateUser/{userEntity.Id}", results);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public ActionResult<User> DeleteUser(int id)
        {
            var validation = new IntValidator();
            var validationResult = validation.Validate(id);

            if (validationResult == null)
            {
                return BadRequest(validationResult);
            }

            var user = _context.Users.Find(id);

            if (user == null)
            {
                return NotFound();
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
}
