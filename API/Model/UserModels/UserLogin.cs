using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API.Model.UserModels
{
    public class UserLogin
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key]
        public string LoginCode { get; set; }

        public DateTime LoginTime { get; set; }

        [Required]
        public string UserCode { get; set; }
        [JsonIgnore]
        public User User { get; set; }

        public string Latitude { get; set; }

        public string Longitude { get; set; }
    }
}
