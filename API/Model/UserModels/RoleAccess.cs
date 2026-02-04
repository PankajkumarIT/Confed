using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Model.UserModels
{
    public class RoleAccess
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key]
        public string AccessId { get; set; }

        [Required]
        public string UserCode { get; set; }

        public User User { get; set; }

        [Required]
        public string RoleCode { get; set; }

        public UserRole UserRole { get; set; }

        [Required]
        public bool AccessToRole { get; set; }
    }
}
