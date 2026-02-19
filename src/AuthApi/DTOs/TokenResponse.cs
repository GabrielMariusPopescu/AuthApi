namespace AuthApi.DTOs;

public record TokenResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);