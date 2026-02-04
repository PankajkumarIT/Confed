using System.ComponentModel.DataAnnotations;

namespace API.Model.ViewModels
{
    public class SharedUser
    {
        [Required]
        public string FileInfoCode {  get; set; }
        public List<string> UserCode { get; set; }
    }
}
