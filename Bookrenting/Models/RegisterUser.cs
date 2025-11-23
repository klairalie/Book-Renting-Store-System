using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace BookRenting.Models
{
    [Table("register_users")]
    public class RegisterUser
    {
        [Key]
        public int registered_id { get; set; } // Primary Key

        [Required]
    [MaxLength(50)]
    [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "First name must contain letters only.")]
    public string FirstName { get; set; } = "";

    [Required]
    [MaxLength(50)]
    [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Last name must contain letters only.")]
    public string LastName { get; set; } = "";

    
    [Range(1, 99, ErrorMessage = "Age must be between 1 and 99.")]
    [RegularExpression(@"^(?!0\d)([1-9][0-9]?)$", ErrorMessage = "Age must not begin with 0 or contain two zeros.")]
    public int Age { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Birthdate { get; set; }

        [Required]
        [MaxLength(200)]
        [ExistingAddress(ErrorMessage = "Address must exist.")]
        public string Address { get; set; } = "";

        [Required]
        [RegularExpression(@"^(?:\+63|0)9\d{9}$", ErrorMessage = "Contact number must be a valid Philippine number (+63/09 followed by 9 digits).")]
        public string ContactNumber { get; set; } = "";

        [Required]
        [EmailAddress]
        [UniqueEmail(ErrorMessage = "Email already exists.")]
        [GmailOnly(ErrorMessage = "Only active Gmail accounts are allowed.")]
        public string Email { get; set; } = "";

        [Required]
        [MaxLength(50)]
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[\W_]).+$",
            ErrorMessage = "Username must contain letters, numbers or special characters and be unique.")]
        [UniqueUserName(ErrorMessage = "Username already exists.")]
        public string UserName { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[\W_]).+$",
            ErrorMessage = "Password must have at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string Password { get; set; } = "";

        [NotMapped]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = "";

        [NotMapped]
        public string OTP { get; set; } = "";

        public bool IsEmailVerified { get; set; } = false;
    }

    #region Custom Validation Attributes

    // Example: Validate Gmail only
    public class GmailOnlyAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string email && email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
                return ValidationResult.Success;

            return new ValidationResult(ErrorMessage ?? "Email must be a Gmail account.");
        }
    }

    // Example: Check if username is unique (requires DbContext)
    public class UniqueUserNameAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var context = (ApplicationDbContext)validationContext.GetService(typeof(ApplicationDbContext))!;
            if (value is string username)
            {
                if (context.RegisterUsers.Any(u => u.UserName == username))
                    return new ValidationResult(ErrorMessage);
            }
            return ValidationResult.Success;
        }
    }

    // Example: Check if email is unique
    public class UniqueEmailAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var context = (ApplicationDbContext)validationContext.GetService(typeof(ApplicationDbContext))!;
            if (value is string email)
            {
                if (context.RegisterUsers.Any(u => u.Email == email))
                    return new ValidationResult(ErrorMessage);
            }
            return ValidationResult.Success;
        }
    }

    // Example: Check if address exists (can be enhanced to query real addresses)
    public class ExistingAddressAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string address && !string.IsNullOrWhiteSpace(address))
                return ValidationResult.Success;

            return new ValidationResult(ErrorMessage ?? "Invalid address.");
        }
    }

    #endregion
}
