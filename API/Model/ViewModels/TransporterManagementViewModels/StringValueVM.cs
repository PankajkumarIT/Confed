using System.ComponentModel.DataAnnotations;

namespace API.Model.ViewModels.TransporterManagementViewModels
{
    public class StringValueVM
    {
        [Required]
        public string Value { get; set; }
    }
}
