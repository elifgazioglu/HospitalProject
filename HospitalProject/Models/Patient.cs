using System;
using System.Collections.Generic;

namespace HospitalProject.Models;

public partial class Patient
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string? BloodType { get; set; }

    public int? HeightCm { get; set; }

    public int? WeightCm { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual User? User { get; set; }
}
