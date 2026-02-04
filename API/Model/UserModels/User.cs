
using API.Model.AreaModels;
using API.Model.ManagementModels.UserModels;
using API.Model.ManagementModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model.UserModels
{
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key]
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

        public bool IsActive { get; set; }

        public bool IsEntityUser { get; set; }

        public DateTime CreatedOn { get; set; }

        public string CreatedBy { get; set; }

        public DateTime UpdatedOn { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime LastLogin { get; set; }

    }

    public class UserInClaimVM
    {
        public User User { get; set; }
        public List<UserRole> UserRoles { get; set; }
        public Organization Organization { get; set; }

        public DepartmentUser DepartmentUser { get; set; }

        public BankUser BankUser { get; set; }

        public TransporterUser TransporterUser { get; set; }
        public Administor Administor { get; set; }
        public UserRole CurrentUserRole {  get; set; }

    }

}
