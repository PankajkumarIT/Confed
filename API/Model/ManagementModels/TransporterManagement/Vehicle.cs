using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model.ManagementModels.TransporterManagement
{
    public class Vehicle
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key]
        public string VehicleCode { get; set; }
        [Required]
        public string VehicleNumber { get; set; } 
        [Required]
        public string VehicleTypeCode { get; set; }

        public VehicleType VehicleType { get; set; }

        [Required]
        public string Manufacturer { get; set; }

        [Required]
        public string RCNumber { get; set; }

        [Required]
        public string ModelName { get; set; }

        [Range(1900, 2100)]
        public int YearOfManufacture { get; set; }
        public string Color { get; set; }
        [Required]
        public string ChassisNumber { get; set; }

        [Range(0, 1000)]
        public double CapacityInTons { get; set; }

        [Range(0, 2000)]
        public double FuelTankCapacityInLiters { get; set; }

        [Required]
        [RegularExpression("^(petrol|diesel|electric|hybrid|cng)$", ErrorMessage = "Invalid Fuel Type.")]
        public string FuelType { get; set; }
        [Required]

        [StringLength(100)]
        public string GPSDeviceId { get; set; }
        public bool IsAvailable { get; set; }
        [Required]
        public string OrganizationCode { get; set; }

        public Organization Organization { get; set; }
        [Required(ErrorMessage = "Rate per kilometer is required.")]
        [Range(0, 1000, ErrorMessage = "Rate per kilometer must be between 0 and 1000.")]
        public decimal RatePerKm { get; set; }
        public DateTime CreatedDate { get; set; }
      
        public string Createdby { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdatedBy { get; set; }

        public bool IsActive { get; set; } 
    }
}
