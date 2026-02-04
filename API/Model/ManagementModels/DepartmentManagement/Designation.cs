using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model.ManagementModels.DepartmentManagement
{
    public class Designation
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key]
        public string DesignationCode { get; set; }

        [Required]
        public string DesignationName { get; set; }

        public bool IsActive { get; set; }
    }
}
