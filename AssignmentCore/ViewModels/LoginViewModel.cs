namespace AssignmentCore.ViewModels
{
    public class LoginViewModel
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;

        public bool RememberMe { get; set; }
    }
}
