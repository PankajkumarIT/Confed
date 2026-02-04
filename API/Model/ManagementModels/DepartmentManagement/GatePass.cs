using API.Model.ManagementModels.TransporterManagement;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Model.ManagementModels.DepartmentManagement
{
    public class GatePass
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key]
        public string GatePassCode { get; set; }

        [Required]
        public string TransportRouteCode { get; set; }
        public TransportRoute TransportRoute { get; set; }
        public string IssuedByUserCode { get; set; }

        public DateTime IssueDate { get; set; } 
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public string QRCodePath { get; set; }
        public string GatePassFilePath { get; set; }
    }
}
