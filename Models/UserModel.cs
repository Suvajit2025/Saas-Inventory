using System.ComponentModel.DataAnnotations;

namespace Invi.Models
{
    public class UserModel
    {
    }
    // Models/SignUpViewModel.cs
    public class SignUpViewModel
    {
        [Required(ErrorMessage = "Company name is required")]
        public string Company { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [Phone(ErrorMessage = "Invalid mobile number")]
        public string Mobile { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
    public class SignInViewModel
    {
        public string EmailOrMobile { get; set; }
        public string Password { get; set; }
    }

}
