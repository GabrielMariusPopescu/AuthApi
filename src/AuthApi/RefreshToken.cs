namespace AuthApi;

public class RefreshToken
{
    public Guid Id { get; init; }
    [StringLength(90)]
    public required string Token { get; init; }
    public Guid UserId { get; init; }
    public DateTime Expires { get; init; }
    public bool IsRevoked { get; set; }
    public DateTime Created { get; init; }
}