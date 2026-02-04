using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Model.ExceptionHandlerModels
{
    public class ExceptionLog
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key]
        public string DataCode { get; set; }
        public string ErrorMessage { get; set; }
        public string OccuredAt { get; set; }
        public string StringException { get; set; }
        public string StackTrace { get; set; }
        public string InnerException { get; set; }
        public string MethodName { get; set; }
        public string ClassName { get; set; }

    }
}
