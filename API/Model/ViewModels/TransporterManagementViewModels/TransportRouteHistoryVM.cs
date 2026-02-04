using API.Model.ManagementModels.DepartmentManagement;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using API.Model.UserModels;

namespace API.Model.ViewModels.TransporterManagementViewModels
{
    public class TransportRouteHistoryVM
    {
        public string ApprovalCode { get; set; }

        [Required]
        public string TransportRouteCode { get; set; }
        public TransportRoute TransportRoute { get; set; }

        [Required]
        public string ActionByUserCode { get; set; }
        public User ActionByUser { get; set; }

        [Required]
        public string ActionByRoleCode { get; set; }
        public UserRole ActionByRole { get; set; }


        public string AssignedToRoleCode { get; set; }
        public UserRole AssignedToRole { get; set; }


        public string AssignedToUserCode { get; set; }
        public User AssignedToUser { get; set; }

        [Required]
        [RegularExpression("^(Approved|Rejected)$")]
        public string Status { get; set; }

        [MaxLength(500)]
        public string Remarks { get; set; }
        public DateTime? ActionDate { get; set; }
    }

}
