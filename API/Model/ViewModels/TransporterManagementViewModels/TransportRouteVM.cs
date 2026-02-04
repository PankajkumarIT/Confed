using API.Model.ManagementModels.DepartmentManagement;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using API.Model.ManagementModels;
using API.Model.ManagementModels.TransporterManagement;

namespace API.Model.ViewModels.TransporterManagementViewModels
{
    public class TransportRouteVM
    { 
        public int Id { get; set; }
        public string TransportRouteCode { get; set; }
        public string SourceOrganizationCode { get; set; }

        [Required]
        public string DestinationAddress { get; set; }
        public string PickupAddress { get; set; }
        [Required]
        public string DestinationContactNo { get; set; }
        [Required]
        public string VehicleCode { get; set; }
       
        public List<Commodity> Commodities { get; set; }
        [Required]


        [Range(0, double.MaxValue)]
        public double Quantity { get; set; }
        [Required]


        [Range(0, double.MaxValue)]
        public double TotalWeight { get; set; }
        [Range(0, double.MaxValue)]
        [Required]

        public double DistanceInKm { get; set; }
        [Required]

        [Range(0, double.MaxValue)]
        public double ExpectedTravelTimeHours { get; set; }
        [Required]

        public DateTime ExpectedJourneyStart { get; set; }
        [Required]

        public DateTime ExpectedJourneyEnd { get; set; }

        public DateTime? ActualJourneyStart { get; set; }
        public DateTime? ActualJourneyEnd { get; set; }
        [Required]

        [Range(0, double.MaxValue)]
        public double BaseAmount { get; set; }

        [Range(0, double.MaxValue)]
        public double TollTax { get; set; }

        [Range(0, double.MaxValue)]
        public double OtherCharges { get; set; }
        [Required]
        public double TotalCharge { get; set; }

        public string AssignedToRoleCode { get; set; }

        public string AssignedToUserCode { get; set; }


    }
}
