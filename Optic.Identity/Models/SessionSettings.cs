namespace Optic.Identity.Models
{
    public class SessionSettings
    {
        public int IdleTimeoutMinutes { get; set; } = 20;

        public int AbsoluteTimeoutMinutes { get; set; } = 60;

        public int ActivityUpdateIntervalSeconds { get; set; } = 60;
    }
}