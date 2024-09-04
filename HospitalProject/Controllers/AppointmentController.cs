using api.Data;
using AutoMapper;
using HospitalProject.Models;
using HospitalProject.UserContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace HospitalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public AppointmentController(ApplicationDBContext context, IMapper mapper, IUserService userService)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
        }

        [HttpPost]
        [Authorize]
        public ActionResult<Appointment> CreateAppointment(AppointmentRequestModel appointmentRequestModel)
        {
            var userId = _userService.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound("User not found");
            }

            var userIdToInt = Convert.ToInt32(userId);
            var patientId = _context.Patients.FirstOrDefault(p => p.UserId == userIdToInt)?.Id;

            if (patientId == null)
            {
                return NotFound("Patient profile not found for the user.");
            }

            var slot = _context.Slots.FirstOrDefault(s => s.Id == appointmentRequestModel.SlotId && s.DoctorId == appointmentRequestModel.DoctorId && s.Status == 1);

            if (slot == null)
            {
                return NotFound("Seçilen doktorun uygun bir slotu bulunamadı veya slot dolu.");
            }

            var appointmentEntity = new Appointment
            {
                PatientId = patientId.Value,
                SlotId = appointmentRequestModel.SlotId
            };

            _context.Appointments.Add(appointmentEntity);
            slot.Status = 0;
            _context.SaveChanges();

            return CreatedAtAction("CreateAppointment", new { id = appointmentEntity.Id });
        }

        [HttpPut("{id}")]
        [Authorize]
        public ActionResult<Appointment> UpdateAppointment(int id, AppointmentUpdateModel updateModel)
        {
            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound("Appointment not found.");
            }

            var oldSlot = _context.Slots.FirstOrDefault(s => s.Id == appointment.SlotId);
            if (oldSlot != null)
            {
                oldSlot.Status = 1;
            }

            var newSlot = _context.Slots.FirstOrDefault(s => s.Id == updateModel.SlotId && s.DoctorId == updateModel.DoctorId && s.Status == 1);

            if (newSlot == null)
            {
                return NotFound("Seçilen doktorun uygun bir slotu bulunamadı veya slot dolu.");
            }

            appointment.SlotId = updateModel.SlotId;
            newSlot.Status = 0;

            _context.Appointments.Update(appointment);
            _context.SaveChanges();

            return NoContent();
        }

        [HttpGet("GetAvailableSlots")]
        [Authorize]
        public ActionResult<IEnumerable<Slot>> GetAvailableSlots(int doctorId)
        {
            var availableSlots = _context.Slots
                .Where(s => s.DoctorId == doctorId && s.Status == 1)
                .ToList();

            if (!availableSlots.Any())
            {
                return NotFound("Seçilen doktora ait uygun slot bulunamadı.");
            }

            return Ok(availableSlots);
        }
    }

    public class AppointmentRequestModel
    {
        public int SlotId { get; set; }
        public int DoctorId { get; set; }
    }

    public class AppointmentUpdateModel
    {
        public int SlotId { get; set; }
        public int DoctorId { get; set; }
    }
}