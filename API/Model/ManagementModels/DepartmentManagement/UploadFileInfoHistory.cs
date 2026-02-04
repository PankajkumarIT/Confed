using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Model.ManagementModels.DepartmentManagement
{
    public class UploadFileInfoHistory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key]
        public string UploadFileInfoHistoryCode { get; set; }

        [Required]
        public string FileInfoCode { get; set; }
        public UploadFileInfo UploadFileInfo { get; set; }

        [Required]
        public string ActionByUserCode { get; set; }

        [Required]
        public string ActionByRoleCode { get; set; }

        public string AssignedToRoleCode { get; set; }

        public string AssignedToUserCode { get; set; }


        [Required]
        [RegularExpression("^(Forwarded|Approved|Rejected)$")]
        public string Status { get; set; }

        public DateTime? ActionDate { get; set; }
    }
}
