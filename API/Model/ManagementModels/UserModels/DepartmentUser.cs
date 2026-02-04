using API.Model.UserModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using API.Model.ManagementModels.DepartmentManagement;

namespace API.Model.ManagementModels.UserModels
{
    public class DepartmentUser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key]
        public string DepartmentUserCode { get; set; }

        [Required]
        public string OfficeCode { get; set; }
        public Office Office { get; set; }

        [Required]
        public string UserCode { get; set; }
        public User User { get; set; }

        [Required]
        public string RoleCode { get; set; }
        public UserRole UserRole { get; set; }
        [Required]
        public string DesignationCode { get; set; }
        public Designation Designation { get; set; }
        public double TotalStorageSize { get; set; }
        public double UsedStorageSize { get; set; }

    }
}
