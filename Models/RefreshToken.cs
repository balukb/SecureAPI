namespace MF_SecureApi.Data
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; } = false;
    }
}