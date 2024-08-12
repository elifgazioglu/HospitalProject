using System;
using System.Collections.Generic;

namespace HospitalProject.Models;

public partial class Slot
{
    public int Id { get; set; }

    public int? DoctorId { get; set; }

    public DateTime? SlotDate { get; set; }

    public byte? Status { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual Doctor? Doctor { get; set; }
}
