using System;
using System.Collections.Generic;

namespace HospitalProject.Models;

public partial class Appointment
{
    public int Id { get; set; }

    public int? PatientId { get; set; }

    public int? SlotId { get; set; }

    public virtual Patient? Patient { get; set; }

    public virtual Slot? Slot { get; set; }
}
