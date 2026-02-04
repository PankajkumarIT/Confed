using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Model.AreaModels
{
    public class District
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key]
        public string DistrictCode { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 3)]
        public string DistrictName { get; set; }

        [Required]
        [StringLength(6)]
        public string DistrictLGDCode { get; set; }

        [Required]
        public string StateCode { get; set; }
        public State State { get; set; }

    }
}
