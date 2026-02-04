using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model.Menus
{
    public class Menu
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key]
        public string MenuCode { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string MenuName { get; set; }

        [Required]
        public string Path { get; set; }

        public string Icon { get; set; }

        public bool Status { get; set; }

        public string ParentCode { get; set; }
        public int SequenceNumber { get; set; }

    }
}
