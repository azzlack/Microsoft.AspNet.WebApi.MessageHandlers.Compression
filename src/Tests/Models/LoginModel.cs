namespace Tests.Models
{
    public class LoginModel
    {
        public LoginModel()
        {
        }

        public LoginModel(string username, string password, string returnUrl)
        {
            this.Username = username;
            this.Password = password;
            this.ReturnUrl = returnUrl;
        }

        public string Username { get; set; }

        public string Password { get; set; }

        public string ReturnUrl { get; set; }
    }
}