using API.Model.ManagementModels.DepartmentManagement;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using API.Model.UserModels;

namespace API.Model.ViewModels
{
    public class FileInfoHistoryVM
    {
        public string UploadFileInfoHistoryCode { get; set; }
        public string FileInfoCode { get; set; }
        public UploadFileInfo UploadFileInfo { get; set; }
        public string ActionByUserCode { get; set; }
        public User ActionByUser { get; set; }
        public string ActionByRoleCode { get; set; }
        public UserRole ActionByRole { get; set; }


        public string AssignedToRoleCode { get; set; }
        public UserRole AssignedToRole { get; set; }


        public string AssignedToUserCode { get; set; }
        public User AssignedToUser { get; set; }
        [Required]
        [RegularExpression("^(Department|Bank)$")]
        public string OrganizationType { get; set; }

        [Required]
        [RegularExpression("^(Approved|Rejected)$")]
        public string Status { get; set; }
        public DateTime? ActionDate { get; set; }
    }
}
