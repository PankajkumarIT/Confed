
using System.ComponentModel.DataAnnotations;

namespace API.Model.QueryParamViewModels
{
    public class RouteCodeVM
    {
        [Required]
        public string RouteCode { get; set; }
    }

}
