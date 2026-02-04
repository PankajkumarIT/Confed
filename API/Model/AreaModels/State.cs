using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Model.AreaModels
{
    public class State
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key]
        public string StateCode { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 3)]
        public string StateName { get; set; }

        [Required]
        [StringLength(6)]
        public string StateLGDCode { get; set; }

    }
}
