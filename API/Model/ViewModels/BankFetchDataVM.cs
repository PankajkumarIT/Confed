using API.Model.ManagementModels;
using API.Model.UserModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Model.ViewModels
{
    public class BankFetchDataVM
    { 
        public int Id { get; set; }
        public string FileInfoCode { get; set; }

        public string FileNumber { get; set; }
        [Required]
        public string FileName { get; set; }

        [Required]
        public string BankName { get; set; }
        public string OfficeName { get; set; }

        [Required]
        public string BranchName { get; set; }

        [Required]
        public string IFSC { get; set; }

        [Required]
        public string OrganizationCode { get; set; }
        public Organization Organization { get; set; }
        public string FilePath { get; set; }

        [RegularExpression("^(Requested|InProcess|Response|Completed|InternalOnly)$", ErrorMessage = "Invalid file statusType.")]
        public string Status { get; set; }

        [Required]
        [RegularExpression("^(Pending|Approved|Rejected)$")]
        public string DepartmentApprovalStatus { get; set; }
        public string BankResponsePath { get; set; }
        public string BankAcknowledgementFilePath { get; set; }
        public string BankNoAcknowledgementFilePath { get; set; }
        public string ResponseFileName { get; set; }
        public string AcknowledgementFileName { get; set; }
        public string NoAcknowledgementFileName { get; set; }
        public bool IsInternalOnly { get; set; } = false;
        public DateTime InprocessDate { get; set; }
        public DateTime ResponseDate { get; set; }
        public string ResponseUserCode { get; set; }
        public string ResponseUserName { get; set; }
        public string ResponseOrganizationCode { get; set; }
        public int TotalCount { get; set; }
        public int FailedCount { get; set; }
        public int ProcessedCount { get; set; }
        [Required]
        public string UserCode { get; set; }
        public User User { get; set; }
        public double FileSize { get; set; }


    }
}
