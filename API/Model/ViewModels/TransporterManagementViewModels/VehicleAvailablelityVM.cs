using System.ComponentModel.DataAnnotations;

namespace API.Model.ViewModels.TransporterManagementViewModels
{
    public class VehicleAvailablelityVM
    {
        [Required]

        public string VehicleCode {  get; set; }
        [Required]

        public bool IsChecked { get; set; }
    }
}
