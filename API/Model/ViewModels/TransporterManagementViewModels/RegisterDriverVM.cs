using API.Model.AreaModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using API.Model.UserModels;
using API.Model.ManagementModels;

namespace API.Model.ViewModels.TransporterManagementViewModels
{
    public class RegisterDriverVM
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

        public bool IsActive { get; set; }
        [Required]
        
        public string LicenseNumber { get; set; }
        [RegularExpression("^(MCWOG|MCWG|MC50CC|LMV_NT|LMV_TR|HMV|HGV|HTV|HPMV|HGMV|Trailer|ERickshaw|International|Others)$",
            ErrorMessage = "Invalid License Type. Allowed values are: MCWOG, MCWG, MC50CC, LMV_NT, LMV_TR, HMV, HGV, HTV, HPMV, HGMV, Trailer, ERickshaw, International, Others.")]
        public string LicenseType { get; set; } = null;
        [Required]

        public DateTime LicenseExpiryDate { get; set; }
        [Required]

        public DateTime DateOfBirth { get; set; }
        public string EmergencyContact { get; set; } // Emergency contact for the driver
        [RegularExpression("^(A\\+|A\\-|B\\+|B\\-|AB\\+|AB\\-|O\\+|O\\-)?$", ErrorMessage = "Blood Group Is Invalid.")]

        public string BloodGroup { get; set; } = null;
        [Required]
        public string RoleCode { get; set; }
        public UserRole UserRole { get; set; }
        [Required]
        public string OrganizationCode { get; set; }
        public Organization Organization { get; set; }

    }
}
