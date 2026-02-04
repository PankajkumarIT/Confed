using API.Model.AreaModels;
using API.Model.ManagementModels;
using System.ComponentModel.DataAnnotations;

namespace API.Model.UserModels
{
    public class RegisterUserVM
    {
        public string UserCode { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        [Required]
        [StringLength(32)]
        public string Password { get; set; }

        [Required]
        [StringLength(10, MinimumLength = 10)]
        public string MobileNumber { get; set; }


        [Required]
        [StringLength(70, MinimumLength = 10)]
        public string EMail { get; set; }

        [Required]
        [StringLength(70, MinimumLength = 10)]
        public string Address { get; set; }
        [Required]
        public string CtvCode { get; set; }
        public Ctv Ctv { get; set; }
        public string Token { get; set; }
        [Required]
        public string RoleCode { get; set; }
        public UserRole UserRole { get; set; }
    }
}
