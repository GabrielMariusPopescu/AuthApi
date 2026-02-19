using System.Security.Cryptography;

namespace AuthApi;

public class TokenService(IConfiguration configuration)
{
    public (string AccessToken, DateTime ExpiresAt) CreateAccessToken(IdentityUser user, IEnumerable<string> roles)
    {
        var jwt = configuration.GetSection("Jwt");
        var jwtKey = jwt["Key"]!;
        var bytes = Encoding.UTF8.GetBytes(jwtKey);
        var key = new SymmetricSecurityKey(bytes);
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var collection = roles.Select(role => new Claim(ClaimTypes.Role, role));
        claims.AddRange(collection);

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: signingCredentials);

        var result = new JwtSecurityTokenHandler().WriteToken(token);
        return new ValueTuple<string, DateTime>(result, token.ValidTo);
    }

    public string CreateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
}