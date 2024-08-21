using HospitalProject.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using api.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace HospitalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IMapper _mapper;  // _mapper alanı tanımlandı

        public DoctorController(ApplicationDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;  // constructor'da _mapper initialize edildi
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
        public ActionResult<Doctor> CreateDoctor(DoctorRequestModel doctor)
        {
            var doctorEntityRequest = _mapper.Map<Doctor>(doctor);

            _context.Doctors.Add(doctorEntityRequest);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetDoctorById), new { id = doctorEntityRequest.Id }, doctorEntityRequest);
        }
    }
    public class DoctorRequestModel
    {
        public int User_Id { get; set; }

        public string Salary { get; set; } = null!;

        public string Title { get; set; } = null!;
        

    }
}