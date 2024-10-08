﻿using System;
using System.Collections.Generic;

namespace HospitalProject.Models
{
    public partial class Doctor
    {
        public int Id { get; set; }

        public int? UserId { get; set; }

        public int? Salary { get; set; }

        public string? Title { get; set; }

        public int? DepartmentId { get; set; } // Foreign Key

        public virtual Department? Department { get; set; } // Navigation Property

        public virtual ICollection<Slot> Slots { get; set; } = new List<Slot>();

        public virtual User? User { get; set; }
    }
}