using System.ComponentModel.DataAnnotations;

namespace API.Model.ViewModels.TransporterManagementViewModels
{
    public class VehicleTypeAndOranizationVM
    {
        [Required]

        public string VehicleTypeCode {  get; set; }
        [Required]

        public string OrganizationCode {  get; set; }
    }
}
