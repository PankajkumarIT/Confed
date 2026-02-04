namespace API.Model.UserModels
{
    public class ChangePasswordVM
    {
        public string OldPassword { get; set; }
        public string UserCode { get; set; }

        public string NewPassword { get; set; }
    }
}
