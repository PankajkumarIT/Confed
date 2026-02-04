using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Model.Routes
{
    public class Route
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key]
        public string RouteCode { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string RouteName { get; set; }

        [Required]
        public string Path { get; set; }

        public bool Status { get; set; }

        public string ParentCode { get; set; }
    }
}
