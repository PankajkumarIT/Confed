using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using API.Model.ManagementModels.UserModels;
using API.Model.UserModels;
using API.Model.ViewModels;

namespace API.Model.ManagementModels.DepartmentManagement
{
    public class UploadFileInfo
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Key]
        public string FileInfoCode { get; set; }

        public string FileNumber { get; set; }
        [Required]
        public string FileName { get; set; }
        public string BankName { get; set; }
        public string BranchName { get; set; }
        public string BankBranchCode { get; set; }

        public string OfficeName { get; set; }
        public string OfficeCode { get; set; }
        public string IFSC { get; set; }
        [Required]
        public string OrganizationCode { get; set; }
        public Organization Organization { get; set; }
        public string RequestFilePath { get; set; }
        public string ResponseFilePath { get; set; }
        public string InProcessFilePath { get; set; }
        public string BankProcessFilePath { get; set; }
        public string RejectFilePath { get; set; }
        public string AcknowledgementFileNamePath { get; set; }
        public string NoAcknowledgementFileNamePath { get; set; }
        public string BankResponsePath { get; set; }
        public string BankAcknowledgementFilePath { get; set; }
        public string BankNoAcknowledgementFilePath { get; set; }
        public string InternalFilePath { get; set; }

        [RegularExpression("^(Draft|Requested|InProcess|Response|Completed|InternalOnly|Rejected)$", ErrorMessage = "Invalid file statusType.")]
        public string Status { get; set; }
        [RegularExpression("^(Pending|Approved|Rejected)$")]
        public string DepartmentApprovalStatus { get; set; }
        public bool IsInternalOnly { get; set; } = false;

        public DateTime RequestedDate { get; set; }
        public DateTime InprocessDate { get; set; }
        public DateTime ResponseDate { get; set; }
        public string ResponseFileName { get; set; }
        public string AcknowledgementFileName { get; set; }
        public string NoAcknowledgementFileName { get; set; }
        public string ResponseUserCode { get; set; }
        public double FileSize { get; set; }
        public string ResponseOrganizationCode { get; set; }
        public string FileType { get; set; }
        public int TotalCount { get; set; }
        public int FailedCount { get; set; }
        public int ProcessedCount { get; set; }
        [Required]
        public string UserCode { get; set; }
        public User User { get; set; }
        public bool IsPrivate {  get; set; }
        public  string SharedUsers { get; set; }
        public string UserFileDirectoryCode { get; set; }


    }
}