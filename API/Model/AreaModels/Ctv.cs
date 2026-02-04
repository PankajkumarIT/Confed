using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Model.AreaModels
{
    public class Ctv
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key]
        public string CtvCode { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 3)]
        public string CtvName { get; set; }

        [Required]
        [StringLength(6)]
        public string CtvLGDCode { get; set; }

        [Required]
        public string DistrictCode { get; set; }
        public District District { get; set; }


    }
}
