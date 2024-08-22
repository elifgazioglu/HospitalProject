using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using api.Data;
using HospitalProject.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Tüm metodlar için kimlik doðrulama gerektirir
    public class DoctorController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IMapper _mapper;

        public DoctorController(ApplicationDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Doctor>> GetDoctors()
        {
            var doctors = _context.Doctors.ToList();

            if (!doctors.Any())
            {
                return NotFound("No doctors found.");
            }

            return Ok(doctors);
        }

        [HttpGet("{id}")]
        public ActionResult<Doctor> GetDoctorById(int id)
        {
            var validation = new IntValidator();
            var validationResult = validation.Validate(id);

            if (validationResult == null)
            {
                return BadRequest(validationResult);
            }

            var doctor = _context.Doctors.FirstOrDefault(d => d.Id == id);

            if (doctor == null)
            {
                return NotFound();
            }

            return Ok(doctor);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")] //sadece adminler bu kismi yapabilecek
        public ActionResult<Doctor> CreateDoctor([FromBody] DoctorRequestModel doctorRequestModel)
        {
            var doctorEntity = _mapper.Map<Doctor>(doctorRequestModel);

            _context.Doctors.Add(doctorEntity);
            _context.SaveChanges();

            var results = new
            {
                id = doctorEntity.Id,
            };

            return Created($"CreateDoctor/{doctorEntity.Id}", results);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")] //sadece adminler bu kismi yapabilecek
        public ActionResult<Doctor> DeleteDoctor(int id)
        {
            var validation = new IntValidator();
            var validationResult = validation.Validate(id);

            if (validationResult == null)
            {
                return BadRequest(validationResult);
            }

            var doctor = _context.Doctors.Find(id);

            if (doctor == null)
            {
                return NotFound();
            }

            _context.Doctors.Remove(doctor);
            _context.SaveChanges();

            return NoContent();
        }
    }

    public class DoctorRequestModel
    {
        public int Salary { get; set; }
        public string Title { get; set; } = null!;
    }
}
