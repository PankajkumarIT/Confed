using API.Model.ManagementModels.DepartmentManagement;

namespace API.Model.ViewModels.TransporterManagementViewModels
{
    public class Commodity
    {
        public CommodityMaster CommodityMaster { get; set; }
        public UnitMaster UnitMaster { get; set; }
        public int Quantity {  get; set; }

    }
}
