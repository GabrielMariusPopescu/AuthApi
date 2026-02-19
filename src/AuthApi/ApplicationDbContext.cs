namespace AuthApi;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
    : IdentityDbContext<User>(options)
{
    public DbSet<RefreshToken> RefreshTokens { get; init; } = null!;
}
