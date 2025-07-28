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
    public class OrganizationModel
    {
        public int OrganizationId { get; set; }

        [Required]
        public int TenantId { get; set; }

        [Required]
        [Display(Name = "Organization Name")]
        public string OrganizationName { get; set; }

        public Guid OrganizationCode { get; set; }

        [Display(Name = "Business Type")]
        public int BusinessTypeId { get; set; }

        [Display(Name = "GSTIN")]
        public string? GSTIN { get; set; }

        [Display(Name = "Address")]
        public string? Address { get; set; }

        public string? City { get; set; }
        public int? StateId { get; set; }

        [Display(Name = "PIN Code")]
        public string? PINCode { get; set; } 
    }
    public class TenantSessionModel
    {
        public int TenantId { get; set; }
        public Guid TenantCode { get; set; }
        public string TenantName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsActive { get; set; }
        public bool OrgExists { get; set; }
        // Add these:
        public int? OrganizationId { get; set; }
        public Guid? OrganizationKey { get; set; }
        public string? OrganizationName { get; set; }
        public bool IsPrimary { get; set; }
        // ✅ New: User Info
        public string LoginUser { get; set; }
        public string Role { get; set; }
    }
}
