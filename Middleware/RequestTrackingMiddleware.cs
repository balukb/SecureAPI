using MF_SecureApi.Data;
using MF_SecureApi.Models;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace MF_SecureApi.Middleware
{
    public class RequestTrackingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestTrackingMiddleware> _logger;
        private const int MaxBodyLogLength = 4096; // 4KB max for request body logging

        public RequestTrackingMiddleware(RequestDelegate next, ILogger<RequestTrackingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using var scope = context.RequestServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var stopwatch = Stopwatch.StartNew();
            var requestLog = new RequestLog
            {
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.Request.Headers.TryGetValue("User-Agent", out var agent) ? agent.ToString() : null,
                RequestPath = context.Request.Path,
                RequestMethod = context.Request.Method,
                QueryString = context.Request.QueryString.Value,
                RequestTime = DateTime.UtcNow
            };

            try
            {
                // Capture User ID (even if unauthorized)
                requestLog.UserId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";

                // Log request body (for non-file uploads)
                if (ShouldLogRequestBody(context))
                {
                    context.Request.EnableBuffering();
                    using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 4096, leaveOpen: true);
                    requestLog.RequestBody = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;
                }
                else
                {
                    requestLog.RequestBody = string.Empty; // Ensure it's never NULL
                }
                // Store headers as JSON (excluding sensitive headers)
                requestLog.Headers = JsonSerializer.Serialize(
                    context.Request.Headers
                        .Where(h => !h.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                        .ToDictionary(h => h.Key, h => h.Value.ToString())
                );

                await _next(context); // Process the request
                requestLog.StatusCode = context.Response.StatusCode;
            }
            catch (Exception ex)
            {
                requestLog.StatusCode = 500; // Log internal server errors
                throw;
            }
            finally
            {
                stopwatch.Stop();
                requestLog.ResponseTimeMs = (int?)stopwatch.ElapsedMilliseconds;

                try
                {
                    // Ensure request logs are saved even for authorized requests
                    await dbContext.RequestLogs.AddAsync(requestLog);
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception dbEx)
                {
                    // Log database save errors
                    Console.WriteLine($"Failed to save request log: {dbEx.Message}");
                }
            }
        }

        private bool ShouldLogRequestBody(HttpContext context)
        {
            return context.Request.ContentLength > 0 &&
                   context.Request.ContentLength < 4096 && // 4KB max for request body logging
                   !context.Request.ContentType?.Contains("multipart/form-data") == true;
        }
    }
}