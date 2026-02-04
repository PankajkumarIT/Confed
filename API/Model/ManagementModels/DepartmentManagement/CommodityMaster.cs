using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model.ManagementModels.DepartmentManagement
{
    public class CommodityMaster
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Key]
        public string CommodityCode {  get; set; }
        [Required]

        public string Name { get; set; }    
        public string Description { get; set; }
    }
}
