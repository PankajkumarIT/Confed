using API.Model.UserModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace API.Model.ManagementModels.TransporterManagement
{
    public class DriverDetails
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Key]
        public string DriverDetailCode { get; set; }
        public string DriverName { get; set; }
        public string LicenseNumber { get; set; }
        [RegularExpression("^(MCWOG|MCWG|MC50CC|LMV_NT|LMV_TR|HMV|HGV|HTV|HPMV|HGMV|Trailer|ERickshaw|International|Others)$",
            ErrorMessage = "Invalid License Type. Allowed values are: MCWOG, MCWG, MC50CC, LMV_NT, LMV_TR, HMV, HGV, HTV, HPMV, HGMV, Trailer, ERickshaw, International, Others.")]
        public string LicenseType { get; set; } = null;

        public DateTime LicenseExpiryDate { get; set; } 
        public DateTime DateOfBirth { get; set; } 
        public string EmergencyContact { get; set; }
        [RegularExpression("^(A\\+|A\\-|B\\+|B\\-|AB\\+|AB\\-|O\\+|O\\-)?$", ErrorMessage = "Blood Group Is Invalid.")]
        public string BloodGroup { get; set; } = null;
        [Required]
        public string UserCode {  get; set; }
        public User User { get; set; }
    }
}
