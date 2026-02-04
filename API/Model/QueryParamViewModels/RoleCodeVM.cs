using System.ComponentModel.DataAnnotations;

namespace API.Model.QueryParamViewModels
{
    public class RoleCodeVM
    {
        [Required]
        public string RoleCode { get; set; }    
    }
}
