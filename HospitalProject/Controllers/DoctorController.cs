using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using api.Data;
using HospitalProject.Models;
using Microsoft.EntityFrameworkCore;
using HospitalProject.Models.Enums;

namespace HospitalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DoctorController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IMapper _mapper;

        public DoctorController(ApplicationDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Tüm doktorları getirme
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

        // ID'ye göre doktor getirme
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

        // Doktor rolü ve departman atama
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public ActionResult<Doctor> AssignDoctorRoleAndDepartment(int userId, int departmentId, [FromBody] DoctorRequestModel doctorRequestModel)
        {
            var user = _context.Users.Find(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var department = _context.Department.Find(departmentId); // Fixed typo from `Department` to `Departments`
            if (department == null)
            {
                return NotFound("Department not found.");
            }

            // Mevcut doktoru kontrol et
            var existingDoctor = _context.Doctors.FirstOrDefault(d => d.UserId == userId && d.DepartmentId == departmentId);
            if (existingDoctor != null)
            {
                return BadRequest("Doctor already exists in this department.");
            }

            var doctorEntity = _mapper.Map<Doctor>(doctorRequestModel);
            doctorEntity.UserId = userId;
            doctorEntity.DepartmentId = departmentId;

            _context.Doctors.Add(doctorEntity);
            _context.SaveChanges();

            var userRole = new RoleUser
            {
                RoleId = (int)Roles.Doctor,
                UserId = userId
            };

            _context.RoleUsers.Add(userRole);
            _context.SaveChanges();

            var results = new
            {
                id = doctorEntity.Id,
                userId = userId,
                departmentId = departmentId
            };

            return Created($"api/Doctor/{doctorEntity.Id}", results); // Adjusted URL for Created response
        }

        // Adminin doktor özelliklerini güncellemesi
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public ActionResult<Doctor> UpdateDoctor(int id, [FromBody] DoctorRequestModel doctorRequestModel)
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
                return NotFound("Doctor not found.");
            }

            // Doktor özelliklerini güncelleme
            doctor.Salary = doctorRequestModel.Salary;
            doctor.Title = doctorRequestModel.Title;

            _context.Doctors.Update(doctor);
            _context.SaveChanges();

            return Ok(doctor);
        }

        // Adminin doktoru silmesi
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
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

