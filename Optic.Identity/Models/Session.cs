namespace Optic.Identity.Models
{
    public class Session
    {
        public long Id { get; set; }

        public Guid SessionId { get; set; }

        public int UserId { get; set; }

        public DateTime LoginAt { get; set; }

        public DateTime LastActivityAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime? LoggedOutAt { get; set; }

        public bool IsRevoked { get; set; }

        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }
    }
}