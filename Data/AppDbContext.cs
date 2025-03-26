using MF_SecureApi.Models;
using Microsoft.EntityFrameworkCore;

namespace MF_SecureApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<AccessToken> AccessTokens { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<RequestLog> RequestLogs { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }
    }
}