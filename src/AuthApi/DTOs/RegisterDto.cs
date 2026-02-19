namespace AuthApi.DTOs;

public class RegisterDto
{
    [Display(Name = "First Name")]
    public required string FirstName { get; set; }

    [Display(Name = "Last Name")]
    public required string LastName { get; set; }

    [Display(Name = "Phone Number")]
    public required string Phone { get; set; }

    [Display(Name = "Email Address")]
    public required string Email { get; set; }

    [Display(Name = "Password")]
    public required string Password { get; set; }
}