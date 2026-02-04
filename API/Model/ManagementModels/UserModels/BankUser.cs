using API.Model.UserModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using API.Model.ManagementModels.BankManagement;
using API.Model.ManagementModels.DepartmentManagement;

namespace API.Model.ManagementModels.UserModels
{
    public class BankUser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key]
        public string BankUserCode { get; set; }

        [Required]
        public string DesignationCode { get; set; }
        public Designation Designation { get; set; }
        

        [Required]
        public string BankBranchCode { get; set; }
        public BankBranch BankBranch { get; set; }

        [Required]
        public string UserCode { get; set; }
        public User User { get; set; }

        [Required]
        public string RoleCode { get; set; }
        public UserRole UserRole { get; set; }

        public double TotalStorageSize { get; set; }
        public double UsedStorageSize { get; set; }
    }
}
