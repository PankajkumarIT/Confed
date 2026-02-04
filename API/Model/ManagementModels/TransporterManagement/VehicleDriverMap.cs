using API.Model.UserModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace API.Model.ManagementModels.TransporterManagement
{
    public class VehicleDriverMap
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Key]
        public string VehicleDriverMapCode {  get; set; }
        [Required]
        public string DriverDetailCode { get; set; }
        public DriverDetails DriverDetail { get; set; }
        [Required]
        public string VehicleCode { get; set; }
        public Vehicle Vehicle { get; set; }
        public DateTime AssignedDate { get; set; }
        public DateTime? UnassignedDate { get; set; }
        public bool IsActive { get; set; }
    }
}
