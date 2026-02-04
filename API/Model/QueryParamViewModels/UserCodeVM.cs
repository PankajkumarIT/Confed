using System.ComponentModel.DataAnnotations;

namespace API.Model.QueryParamViewModels
{
    public class UserCodeVM
    {
        [Required]
        public string UserCode { get; set; }

        public string Date { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

    }
}
