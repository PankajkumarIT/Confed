using System.Text.Json.Serialization;

namespace API.Helpers.Models
{
    public class ReturnResponse
    {
        public bool status { get; set; }
        [JsonIgnore]
        public StatusType statusTypeEnum { get; set; }
        public string statusType => statusTypeEnum.ToString();
        public string message { get; set; }
        public dynamic data { get; set; }
    }
}
