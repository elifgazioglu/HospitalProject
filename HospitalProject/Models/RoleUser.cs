using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalProject.Models
{
    public class RoleUser
    {
        [Key, Column(Order = 0)]
        public int RoleId { get; set; }
        public Role Role { get; set; }

        [Key, Column(Order = 1)]
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
