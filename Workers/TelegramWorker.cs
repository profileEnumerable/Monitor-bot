using TdLib;
using Telegram.Bot;
using static TdLib.TdApi;
using static TdLib.TdApi.MessageContent;

namespace AspNet_Air_Alert_Bot.Workers
{
    public class TelegramWorker : BackgroundService
    {
        private readonly ILogger<TelegramWorker> _logger;
        private readonly TdClient _tdClient;

        public TelegramWorker(
            ILogger<TelegramWorker> logger,
            TdClient tdClient)
        {
            _logger = logger;
            _tdClient = tdClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("I started working !");

            var botClient = new TelegramBotClient(Environment.GetEnvironmentVariable("TELEGRAM_TOKEN"));
            await botClient.SendMessage(int.Parse(Environment.GetEnvironmentVariable("CHAT_ID")), "Я почав працювати з новими силами ! 🔛");

            _logger.LogInformation("Sending params ...");
            await _tdClient.ExecuteAsync(new SetTdlibParameters
            {
                UseTestDc = false,
                DatabaseDirectory = "tdlib",
                UseFileDatabase = true,
                UseChatInfoDatabase = true,
                UseMessageDatabase = true,
                UseSecretChats = false,
                ApiId = int.Parse(Environment.GetEnvironmentVariable("API_ID")),
                ApiHash = Environment.GetEnvironmentVariable("API_HASH"),
                SystemLanguageCode = "en",
                DeviceModel = "PC",
                ApplicationVersion = "1.0",
            });

            _tdClient.UpdateReceived += async (sender, update) =>
            {
                switch (update)
                {
                    case Update.UpdateNewMessage newMessageFromChannel:
                        if (newMessageFromChannel?.Message.Content is MessageText messageText)
                        {
                            await RepostIfMatchesKeywords(botClient, messageText);
                        }
                        break;
                    case Update.UpdateAuthorizationState authUpdate:
                        await HandleAuth(authUpdate.AuthorizationState);
                        break;
                }
            };
        }

        private async Task HandleAuth(AuthorizationState authState)
        {
            switch (authState)
            {
                case AuthorizationState.AuthorizationStateWaitPhoneNumber:
                    _logger.LogInformation("Sending phone number ...");
                    await _tdClient.ExecuteAsync(
                        new SetAuthenticationPhoneNumber
                        {
                            PhoneNumber = Environment.GetEnvironmentVariable("PHONE_NUMBER")
                        });
                    break;

                case AuthorizationState.AuthorizationStateReady:
                    Console.WriteLine("✅ Successfully authtorized!");
                    break;

                default:
                    Console.WriteLine($"⚠️ Unknown auth state: {authState.GetType().Name}");
                    break;
            }
        }

        private async Task RepostIfMatchesKeywords(TelegramBotClient botClient, MessageText message)
        {
            string[] allowedKeyWords = Environment.GetEnvironmentVariable("ALLOWED_KEY_WORDS").Split(",");
            string[] deniedKeyWords = Environment.GetEnvironmentVariable("DENIED_KEY_WORDS").Split(",");

            string messageText = message.Text.Text;

            if (ShouldPostMessage(allowedKeyWords, deniedKeyWords, messageText))
            {
                _logger.LogInformation("🆕 Some message has been posted");
                await botClient.SendMessage(int.Parse(Environment.GetEnvironmentVariable("CHAT_ID")), messageText);
            }

        }

        public static bool ShouldPostMessage(string[] allowedKeyWords, string[] deniedKeyWords, string messageText)
        {
            return ContainsKeywords(messageText, allowedKeyWords) && !ContainsKeywords(messageText, deniedKeyWords);
        }

        private static bool ContainsKeywords(string messageText, string[] allowedKeyWords)
        {
            return allowedKeyWords.Any(keyWord => messageText.Contains(keyWord, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
