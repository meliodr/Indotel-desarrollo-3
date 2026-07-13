using System.ComponentModel.DataAnnotations;

namespace Indotel.Core.DTOs;

public class RefreshTokenRequestDto
{
    private string _refreshToken = string.Empty;

    [Required]
    public string RefreshToken
    {
        get => _refreshToken;
        set => _refreshToken = value ?? string.Empty;
    }
}
