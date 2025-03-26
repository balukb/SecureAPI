using MF_SecureApi.Data;
using MF_SecureApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MF_SecureApi.Services
{
    public class TokenService
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _dbContext;

        public TokenService(IConfiguration config, AppDbContext dbContext)
        {
            _config = config;
            _dbContext = dbContext;
        }

        public async Task<string> GenerateAccessToken(string userId)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiryMinutes = Convert.ToDouble(_config["Jwt:AccessTokenExpiryMinutes"]);
            var expiryTime = DateTime.UtcNow.AddMinutes(expiryMinutes);
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId)
                },
                expires: expiryTime,
                signingCredentials: creds
            );
            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            // Store in Database
            var accessTokenEntity = new AccessToken
            {
                Token = accessToken,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiryTime
            };

            await _dbContext.AccessTokens.AddAsync(accessTokenEntity);
            await _dbContext.SaveChangesAsync();
            return accessToken;
        }

        public async Task<RefreshToken> GenerateRefreshToken(string userId)
        {
            var refreshToken = new RefreshToken
            {
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddDays(Convert.ToInt32(_config["Jwt:RefreshTokenExpiryDays"]))
            };

            await _dbContext.RefreshTokens.AddAsync(refreshToken);
            await _dbContext.SaveChangesAsync();

            return refreshToken;
        }

        public async Task<bool> ValidateRefreshToken(string refreshToken, string userId)
        {
            var token = await _dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == userId);
            return token != null && !token.IsRevoked && token.ExpiresAt > DateTime.UtcNow;
        }

        public async Task RevokeRefreshToken(string refreshToken)
        {
            var token = await _dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            if (token != null)
            {
                token.IsRevoked = true;
                await _dbContext.SaveChangesAsync();
            }
        }

        public ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _config["Jwt:Audience"],
                    ValidateLifetime = false
                }, out _);

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}