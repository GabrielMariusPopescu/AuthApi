namespace AuthApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    UserManager<User> userManager,
    TokenService tokenService,
    ApplicationDbContext db,
    IConfiguration config)
    : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var user = new User { FirstName = "Jon", LastName = "Doe",UserName = dto.Email, Email = dto.Email };
        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);

        await userManager.AddToRoleAsync(user, "User");
        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null) return Unauthorized();

        if (!await userManager.CheckPasswordAsync(user, dto.Password)) return Unauthorized();

        var roles = await userManager.GetRolesAsync(user);
        var (accessToken, expiresAt) = tokenService.CreateAccessToken(user, roles);

        var refreshToken = tokenService.CreateRefreshToken();
        var refresh = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id.ToGuid(),
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddDays(int.Parse(config["Jwt:RefreshTokenDays"] ?? "30")),
            IsRevoked = false
        };
        db.RefreshTokens.Add(refresh);
        await db.SaveChangesAsync();

        return Ok(new TokenResponse(accessToken, refreshToken, expiresAt));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest req)
    {
        var stored = await db.RefreshTokens.Include(r => r.UserId).FirstOrDefaultAsync(r => r.Token == req.RefreshToken);
        if (stored == null || stored.IsRevoked || stored.Expires < DateTime.UtcNow) return Unauthorized();

        var user = await userManager.FindByIdAsync(stored.UserId.ToString());
        if (user == null) 
            return Unauthorized();

        stored.IsRevoked = true;

        var roles = await userManager.GetRolesAsync(user);
        var (accessToken, expiresAt) = tokenService.CreateAccessToken(user, roles);
        var newRefresh = tokenService.CreateRefreshToken();
        db.RefreshTokens.Add(new RefreshToken
        {
            Token = newRefresh,
            UserId = user.Id.ToGuid(),
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddDays(int.Parse(config["Jwt:RefreshTokenDays"] ?? "30")),
            IsRevoked = false
        });

        await db.SaveChangesAsync();
        return Ok(new TokenResponse(accessToken, newRefresh, expiresAt));
    }

    [Authorize]
    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke()
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userId == null) 
            return Unauthorized();

        var tokens = db
            .RefreshTokens
            .Where(token => token.UserId == userId.ToGuid() && 
                            !token.IsRevoked);
        foreach (var token in tokens) 
            token.IsRevoked = true;
        
        await db.SaveChangesAsync();
        return NoContent();
    }
}