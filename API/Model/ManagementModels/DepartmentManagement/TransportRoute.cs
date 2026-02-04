using API.Model.ManagementModels.TransporterManagement;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using API.Model.UserModels;
using API.Model.ViewModels.TransporterManagementViewModels;

namespace API.Model.ManagementModels.DepartmentManagement
{
    public class TransportRoute
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key]
        public string TransportRouteCode{ get; set; }

        [Required]
        public string SourceOrganizationCode { get; set; }

        [Required]
        public string PickupAddress { get; set; }
        [Required]
        public string DestinationAddress { get; set; }
        [Required]
        public string DestinationContactNo { get; set; }
        [Required]
        public string VehicleCode { get; set; }
        public Vehicle Vehicle { get; set; }
        public string Commodities { get; set; }

        [Range(0, double.MaxValue)]
        public double Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public double TotalWeight { get; set; }
        [Range(0, double.MaxValue)]
        public double DistanceInKm { get; set; }

        [Range(0, double.MaxValue)]
        public double ExpectedTravelTimeHours { get; set; } 
        public DateTime ExpectedJourneyStart { get; set; }
        public DateTime ExpectedJourneyEnd { get; set; }
        public DateTime? ActualJourneyStart { get; set; }
        public DateTime? ActualJourneyEnd { get; set; }
        
        [Range(0, double.MaxValue)]
        public double BaseAmount { get; set; }

        [Range(0, double.MaxValue)]
        public double TollTax { get; set; }

        [Range(0, double.MaxValue)]
        public double OtherCharges { get; set; }
        public double TotalCharge { get; set; }
        [Required]
        [RegularExpression("^(Pending|Approved|Rejected)$")]
        public string ApprovalStatus { get; set; }
    
        [RegularExpression("^(Created|Dispatched|Delivered)$")]
        public string DestinationStatus { get; set; }
        [Required]
        public string UserCode {  get; set; }
        public User User { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedDate {  get; set; }
        public DateTime UpdatedDate {  get; set; }
        public string UpdatedBy {  get; set; }
        public string CreatedBy {get; set; }
    }
}
