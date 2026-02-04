using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace API.Model.ManagementModels.TransporterManagement
{
    public class VehicleType
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Key]
        public string VehicleTypeCode {  get; set; }
        [Required]
        public string VehicleTypeName { get; set; }


        [Range(0, double.MaxValue)]
        public double MaxLoadCapacity { get; set; } // in tons

        public string Description { get; set; }
        public bool IsActive { get; set; } = true;

    }
}
