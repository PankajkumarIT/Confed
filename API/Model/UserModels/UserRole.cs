
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace API.Model.UserModels
{
    public class UserRole
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key]
        public string RoleCode { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string RoleName { get; set; }

        [Required]
        public RoleLevels RoleLevel { get; set; }
        [RegularExpression("^(department|transporter|bank)$", ErrorMessage = "Invalid RoleType .")]
        public string RoleType { get; set; } = null;
    }
}
