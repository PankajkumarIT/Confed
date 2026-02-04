using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Model.ManagementModels.DepartmentManagement
{
    public class UnitMaster
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key]
        public string UnitCode { get; set; }

        [Required]
        [MaxLength(50)]
        public string UnitName { get; set; } 
        [Range(0, double.MaxValue)]
        public double UnitWeight { get; set; } 
        public bool IsActive { get; set; } = true;
    }

}
