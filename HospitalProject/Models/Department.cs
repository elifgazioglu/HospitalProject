using System;
using System.Collections.Generic;

namespace HospitalProject.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = null!;

        // Bir departmanın birden fazla doktoru olabilir.
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}
