namespace Web.Configurations
{
    public class TelegramSettings
    {
        public const string SectionName = "Telegram";

        public string BotToken { get; set; } = string.Empty;

        public long ChannelId { get; set; }

        public int ApiId { get; set; }

        public string ApiHash { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public int MessageHistoryLimit { get; set; } = 1000;
    }
}
