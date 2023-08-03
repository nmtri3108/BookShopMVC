using System.ComponentModel.DataAnnotations;

namespace AppdevBookShop.ViewModels;

public class ResetPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
        
    [Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Password and confirm password")]
    public string ConfirmPassword { get; set; }

    public string Token { get; set; }
        
    [Required]
    [DataType(DataType.Password)]// Admin123@
    public string Password { get; set; }
}