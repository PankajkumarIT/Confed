using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using API.Model.UserModels;

namespace API.Model.ManagementModels.DepartmentManagement
{
    public class TransportRouteHistory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key]
        public string ApprovalCode { get; set; }

        [Required]
        public string TransportRouteCode { get; set; }
        public TransportRoute TransportRoute { get; set; }

        [Required]
        public string ActionByUserCode { get; set; }

        [Required]
        public string ActionByRoleCode { get; set; }

        public string AssignedToRoleCode { get; set; }
     
        public string AssignedToUserCode { get; set; }

        [Required]
        [RegularExpression("^(forwarded|Approved|Rejected)$")]
        public string Status { get; set; }

        [MaxLength(500)]
        public string Remarks { get; set; }
        public DateTime? ActionDate { get; set; }
    }
}
