namespace API.Model.UserModels
{
    public class AuthenticateVM
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        public string DFHash { get; set; }
    }
}
