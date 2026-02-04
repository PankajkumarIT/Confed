using API.Model.ManagementModels;
using API.Model.UserModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Model.ViewModels
{
    public class GetUploadFileInfoVM
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Key]
        public string FileInfoCode { get; set; }

        public string FileNumber { get; set; }
        [Required]
        public string FileName { get; set; }

        [Required]
        public string BankName { get; set; }

        [Required]
        public string BranchName { get; set; }

        [Required]
        public string IFSC { get; set; }

        public string OfficeName { get; set; }
        public string OfficeCode { get; set; }

        [Required]
        public string OrganizationCode { get; set; }
        public Organization Organization { get; set; }
        public string FilePath { get; set; }

        [RegularExpression("^(Requested|InProcess|Response|Completed|InternalOnly)$", ErrorMessage = "Invalid file statusType.")]
        public string Status { get; set; }

        [Required]
        [RegularExpression("^(Pending|Approved|Rejected)$")]
        public string DepartmentApprovalStatus { get; set; }
        public bool IsInternalOnly { get; set; } = false;
        public string ResponseFilePath { get; set; }
        public string AcknowledgementFileNamePath { get; set; }
        public string NoAcknowledgementFileNamePath { get; set; }


        public string ResponseFileName { get; set; }
        public string AcknowledgementFileName { get; set; }
        public string NoAcknowledgementFileName { get; set; }
        public DateTime RequestedDate { get; set; }
        public DateTime InprocessDate { get; set; }
        public DateTime ResponseDate { get; set; }
        public string ResponseUserCode { get; set; }
        public string ResponseOrganizationCode { get; set; }
        public int TotalCount { get; set; }
        public int FailedCount { get; set; }
        public int ProcessedCount { get; set; }
        public string UserCode { get; set; }
        public double FileSize { get; set; }

        public User User { get; set; }
        public List<SharedUser> SharedUsers { get; set; }

        public List<FileInfoHistoryVM> FileInfoHistory{  get; set; }

    }
}
