
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.ViewModels;

public class ProfileViewModel
{
    public int UserID { get; set; }

    [Required(ErrorMessage = "First name is required.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public DateTime? CreatedAt { get; set; }
}
