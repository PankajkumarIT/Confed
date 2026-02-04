using API.Model.UserModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Model.ManagementModels.UserModels
{
    public class Administor
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key]
        public string AdministorCode { get; set; }
        [Required]
        public string OrganizationCode { get; set; }
        public Organization Organization { get; set; }

        [Required]
        public string UserCode { get; set; }
        public User User { get; set; }
        [Required]
        public string RoleCode { get; set; }
        public UserRole UserRole { get; set; }
    }
}
