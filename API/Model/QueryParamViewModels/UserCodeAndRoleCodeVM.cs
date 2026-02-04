using System.ComponentModel.DataAnnotations;

namespace API.Model.QueryParamViewModels
{
    public class UserCodeAndRoleCodeVM
    {
        [Required]
        public string UserCode { get; set; }
        [Required]
        public string RoleCode { get; set; }
    }
}
