using api.Data;
using AutoMapper;
using HospitalProject.Models;
using HospitalProject.Models.Enums;
using HospitalProject.UserContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public PatientController(ApplicationDBContext context, IMapper mapper, IUserService userService)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
        }

        [HttpGet]
        [Authorize]
        public ActionResult<IEnumerable<Patient>> GetPatients()
        {
            return _context.Patients.ToList();
        }

        [HttpGet("{id}")]
        [Authorize]
        public ActionResult<Patient> GetPatient(int id)
        {
            var validation = new IntValidator();
            var validationResult = validation.Validate(id);

            if (validationResult == null)
            {
                return BadRequest(validationResult);
            }

            var patient = _context.Patients.Find(id);

            if (patient == null)
            {
                return NotFound();
            }

            return patient;
        }

        [HttpPost]
        [Authorize]
        public ActionResult<Patient> CreatePatient(PatientRequestModel patientRequestModel)
        {
            var userId = _userService.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return NotFound("User not found");
            }

            var userIdToInt = Convert.ToInt32(userId);


            var userRole = new RoleUser
            {
                RoleId = (int)Roles.Patient,
                UserId = userIdToInt
            };

            _context.RoleUsers.Add(userRole);
            _context.SaveChanges();

            var patientEntity = new Patient
            {
                UserId = userIdToInt,
                BirthDate = patientRequestModel.BirthDate,
                BloodType = patientRequestModel.BloodType,
                HeightCm = patientRequestModel.HeightCm,
                WeightCm = patientRequestModel.WeightCm
            };

            _context.Patients.Add(patientEntity);
            _context.SaveChanges();

            return CreatedAtAction("CreatePatient", new { id = patientEntity.Id });
        }


        [HttpDelete("{id}")]
        [Authorize]
        public ActionResult<Patient> DeletePatient(int id)
        {
            var validation = new IntValidator();
            var validationResult = validation.Validate(id);

            if (validationResult == null)
            {
                return BadRequest(validationResult);
            }

            var patient = _context.Patients.Find(id);

            if (patient == null)
            {
                return NotFound();
            }

            _context.Patients.Remove(patient);
            _context.SaveChanges();

            return NoContent();
        }
    }

    public class PatientRequestModel
    {
        public DateTime? BirthDate { get; set; }
        public string BloodType { get; set; } = null!;
        public int HeightCm { get; set; }
        public int WeightCm { get; set; }
    }
}