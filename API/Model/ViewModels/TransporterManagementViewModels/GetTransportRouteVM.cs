using API.Model.ManagementModels;
using API.Model.ManagementModels.DepartmentManagement;
using API.Model.ManagementModels.TransporterManagement;
using API.Model.UserModels;
using System.ComponentModel.DataAnnotations;

namespace API.Model.ViewModels.TransporterManagementViewModels
{
    public class GetTransportRouteVM
    {
        public string TransportRouteCode { get; set; }
        public string PickupAddress { get; set; }
        public string SourceOrganization { get; set; }
        public Organization Organization { get; set; }
        public string DestinationAddress { get; set; }
        public string DestinationContactNo { get; set; }
        public string VehicleCode { get; set; }
        public Vehicle Vehicle { get; set; }

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
         [RegularExpression("^(Pending|Approved|Rejected)$")]
        public string ApprovalStatus { get; set; }
        [RegularExpression("^(InTransit|Dispatched|Delivered)$")]
        public string DestinationStatus { get; set; }
        public bool IsCompleted { get; set; }
        public List<VehicleDriverMap> DriverList { get; set; }
        public List<TransportRouteHistoryVM> TransportRouteHistoryVM { get; set; }
        public List<Commodity> Commodities { get; set; }

        public GatePass GatePass { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public string CreatedBy { get; set; }
    }
}
