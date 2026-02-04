using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Model.ViewModels
{
    public class RegisterTransportOrganizationVM
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Key]
        public string OrganizationCode { get; set; }
        [Required]
        public string OrganizationName { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string ContactNumber { get; set; }
        public string ShortName { get; set; }

        [Required]
        public string Email { get; set; }
        [Required]
        public string GstNumber { get; set; }
        [Required]
        public string PanNumber { get; set; }
        public string ParentOrganizationCode { get; set; }
        public bool IsActive { get; set; }
    }
}
