using API.Model.UserModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model.ManagementModels.UserModels
{
    public class TransporterUser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }      

        [Key]
        public string TransportUserCode { get; set; }
        [Required]
        public string OrganizationCode { get; set; }
        public Organization Organization { get; set; }

        [Required]
        public string UserCode { get; set; }
        public User User { get; set; }
        public bool IsDriver { get; set; }
        [Required]
        public string RoleCode { get; set; }
        public UserRole UserRole { get; set; }
        public double TotalStorageSize { get; set; }
        public double UsedStorageSize { get; set; }
    }
}
