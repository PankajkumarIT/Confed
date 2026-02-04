using API.Model.AreaModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Model.ViewModels
{
    public class RegisterAdministorVM
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key]
        public string AdministorCode { get; set; }
        public string OrganizationCode { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        [Required]
        [StringLength(32)]
        public string Password { get; set; }

        [Required]
        [StringLength(10, MinimumLength = 10)]
        public string MobileNumber { get; set; }


        [Required]
        [StringLength(70, MinimumLength = 10)]
        public string EMail { get; set; }


        [Required]
        public string CtvCode { get; set; }
        public Ctv Ctv { get; set; }

        [Required]
        [StringLength(70, MinimumLength = 10)]
        public string Address { get; set; }
    }
}
