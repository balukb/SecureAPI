using MF_SecureApi.Data;
using MF_SecureApi.Models;

namespace MF_SecureApi.Services
{
    public class ErrorLoggingService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<ErrorLoggingService> _logger;

        public ErrorLoggingService(AppDbContext dbContext, ILogger<ErrorLoggingService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task LogErrorAsync(Exception ex, HttpContext context)
        {
            try
            {
                var errorLog = new ErrorLog
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    Source = ex.Source,
                    RequestPath = context?.Request.Path,
                    UserAgent = context?.Request.Headers["User-Agent"],
                    IpAddress = context?.Connection.RemoteIpAddress?.ToString(),
                    LoggedAt = DateTime.UtcNow
                };

                await _dbContext.ErrorLogs.AddAsync(errorLog);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Failed to log error.");
            }
        }
    }
}