using System;

namespace FluentPOS.Shared.DTOs.Identity
{
    public record TokenResponse(string Token, string RefreshToken, DateTime RefreshTokenExpiryTime);
}