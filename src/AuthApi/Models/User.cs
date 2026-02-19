namespace AuthApi.Models;

public class User : IdentityUser
{
    [StringLength(50)]
    public required string FirstName { get; set; }

    [StringLength(50)]
    public required string LastName { get; set; }
}