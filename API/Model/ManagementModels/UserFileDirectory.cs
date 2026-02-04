using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model.ManagementModels
{
    public class UserFileDirectory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Key]
        public string UserFileDirectoryCode { get; set; }
        [Required]
        public string DirectoryName { get; set; }
        public string ParentDirectoryCode { get; set; }
        public string CreatedByUserCode { get; set; }
        public DateTime CreatedDate {  get; set; }
        public bool IsDeleted { get; set; } = false;

    }
}
