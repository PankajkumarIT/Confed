using System.ComponentModel.DataAnnotations;

namespace API.Model.ViewModels
{
    public class SecureDownloadVM
    {
        [Required]
        public string FileInfoCode { get; set; }

        [Required]
        public string Password { get; set; }
    }

}
