using API.Model.UserModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model.Menus
{
    public class MenuAccess
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key]
        public string AccessCode { get; set; }

        [Required]
        public string MenuCode { get; set; }
        public Menu Menu { get; set; }

        [Required]
        public string RoleCode { get; set; }
        public UserRole UserRole { get; set; }

        [Required]
        public bool Status { get; set; }

    }
}
