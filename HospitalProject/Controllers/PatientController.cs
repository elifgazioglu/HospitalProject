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

        // Tüm hastaları getirme (sadece admin yetkilendirmesi gerektirir)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult<IEnumerable<Patient>> GetPatients()
        {
            return _context.Patients.ToList();
        }

        // Hastanın sadece kendi bilgilerini görüntüleyebilmesi
        [HttpGet("me")]
        [Authorize]
        public ActionResult<Patient> GetMyInfo()
        {
            var userId = _userService.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return NotFound("User not found");
            }

            var patient = _context.Patients.SingleOrDefault(p => p.UserId == Convert.ToInt32(userId));

            if (patient == null)
            {
                return NotFound("Patient not found");
            }

            return Ok(patient);
        }

        // ID'ye göre belirli bir hastayı getirme (Admin yetkisi gerektirir)
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public ActionResult<Patient> GetPatient(int id)
        {
            var patient = _context.Patients.Find(id);

            if (patient == null)
            {
                return NotFound();
            }

            return Ok(patient);
        }

        // Yeni hasta oluşturma
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

            return CreatedAtAction("GetPatient", new { id = patientEntity.Id }, patientEntity);
        }

        // Hastanın kendi fiziksel özelliklerini güncellemesi
        [HttpPut("me/updatePhysicalAttributes")]
        [Authorize]
        public ActionResult<Patient> UpdateMyPhysicalAttributes([FromBody] UpdatePhysicalAttributesRequestModel model)
        {
            var userId = _userService.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return NotFound("User not found");
            }

            var patient = _context.Patients.SingleOrDefault(p => p.UserId == Convert.ToInt32(userId));

            if (patient == null)
            {
                return NotFound("Patient not found");
            }

            // Boy ve kilo değerlerini güncelleme
            patient.HeightCm = model.HeightCm;
            patient.WeightCm = model.WeightCm;

            _context.Patients.Update(patient);
            _context.SaveChanges();

            return Ok(patient);
        }

        // Adminin belirli bir hastanın fiziksel özelliklerini güncellemesi
        [HttpPut("{id}/updatePhysicalAttributes")]
        [Authorize(Roles = "Admin")]
        public ActionResult<Patient> UpdatePhysicalAttributes(int id, [FromBody] UpdatePhysicalAttributesRequestModel model)
        {
            var patient = _context.Patients.Find(id);

            if (patient == null)
            {
                return NotFound();
            }

            // Boy ve kilo değerlerini güncelleme
            patient.HeightCm = model.HeightCm;
            patient.WeightCm = model.WeightCm;

            _context.Patients.Update(patient);
            _context.SaveChanges();

            return Ok(patient);
        }

        // Hastanın kendi hesabını silmesi
        [HttpDelete("me")]
        [Authorize]
        public ActionResult DeleteMyAccount()
        {
            var userId = _userService.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return NotFound("User not found");
            }

            var patient = _context.Patients.SingleOrDefault(p => p.UserId == Convert.ToInt32(userId));

            if (patient == null)
            {
                return NotFound("Patient not found");
            }

            _context.Patients.Remove(patient);
            _context.SaveChanges();

            return NoContent();
        }

        // Adminin belirli bir hastayı silmesi
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public ActionResult DeletePatient(int id)
        {
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

    public class UpdatePhysicalAttributesRequestModel
    {
        public int HeightCm { get; set; }
        public int WeightCm { get; set; }
    }
}


