using System;
using System.Collections.Generic;

namespace HospitalProject.Models;

public partial class User
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string Ad { get; set; } = null!;

    public string Soyad { get; set; } = null!;

    public string Password { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();

    public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
