using api.Data;
using AutoMapper;
using HospitalProject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace HospitalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IMapper _mapper;

        public UserController(ApplicationDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<User>> GetUsers()
        {
            return _context.Users.ToList();
        }


        [HttpGet("{id}")]
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
        public ActionResult<User> CreateUser(UserRequestModel user)
        {

            var userEntityRequest = _mapper.Map<User>(user);

            _context.Users.Add(userEntityRequest);
            _context.SaveChanges();


            return CreatedAtAction(nameof(GetUser), new { id = userEntityRequest.Id }, user);
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