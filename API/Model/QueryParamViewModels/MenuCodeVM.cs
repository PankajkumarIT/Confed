using System.ComponentModel.DataAnnotations;

namespace API.Model.QueryParamViewModels
{
    public class MenuCodeVM
    {
        [Required]
        public string MenuCode { get; set; }
    }
}
