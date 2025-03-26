using MF_SecureApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MF_SecureApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly IConfiguration _config;

        public AuthController(TokenService tokenService, IConfiguration config)
        {
            _tokenService = tokenService;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Validate credentials (replace with actual validation)
            var userId = request.Username; // Example user ID

            var accessToken = await _tokenService.GenerateAccessToken(userId);
            var refreshToken = await _tokenService.GenerateRefreshToken(userId);

            return Ok(new AuthResponse
            {
                AccessToken = accessToken,
                //RefreshToken = refreshToken.Token,
                ExpiresIn = Convert.ToInt32(TimeSpan.FromMinutes(Convert.ToDouble(_config["Jwt:AccessTokenExpiryMinutes"])).TotalSeconds)
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            var principal = _tokenService.GetPrincipalFromToken(request.AccessToken);
            var userId = principal?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId != null && await _tokenService.ValidateRefreshToken(request.RefreshToken, userId))
            {
                var newAccessToken = await _tokenService.GenerateAccessToken(userId);
                return Ok(new AuthResponse
                {
                    AccessToken = newAccessToken,
                    ExpiresIn = Convert.ToInt32(TimeSpan.FromMinutes(Convert.ToDouble(_config["Jwt:AccessTokenExpiryMinutes"])).TotalSeconds)
                });
            }

            return Unauthorized();
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] RevokeRequest request)
        {
            await _tokenService.RevokeRefreshToken(request.RefreshToken);
            return NoContent();
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RefreshRequest
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

    public class RevokeRequest
    {
        public string RefreshToken { get; set; }
    }

    public class AuthResponse
    {
        public string AccessToken { get; set; }

        //     public string RefreshToken { get; set; }
        public double ExpiresIn { get; set; }

        public string TokenType { get; } = "bearer";
    }
}