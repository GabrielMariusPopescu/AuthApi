namespace AuthApi;

public class RefreshToken
{
    public Guid Id { get; set; }
    public required string Token { get; set; }
    public Guid UserId { get; set; }
    public DateTime Expires { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime Created { get; set; }
}