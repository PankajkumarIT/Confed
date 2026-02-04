using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using API.Model.UserModels;

namespace API.Model.Routes
{
    public class RouteAccess
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key]
        public string AccessCode { get; set; }

        [Required]
        public string RouteCode { get; set; }
        public Route Route { get; set; }

        [Required]
        public string RoleCode { get; set; }
        public UserRole UserRole { get; set; }

        [Required]
        public bool Status { get; set; }


    }
}
