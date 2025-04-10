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
            await botClient.SendMessage(int.Parse(Environment.GetEnvironmentVariable("CHAT_ID")), "Я почав працювати 🔛");

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

        private async Task RepostIfMatchesKeywords(TelegramBotClient botClient, MessageText messageText)
        {
            string[] keyWords = Environment.GetEnvironmentVariable("KEY_WORDS").Split(",");

            if (keyWords.Any(keyWord => messageText.Text.Text.Contains(keyWord,StringComparison.InvariantCultureIgnoreCase)))
            {
                _logger.LogInformation("🆕 Some message has been posted");
                await botClient.SendMessage(int.Parse(Environment.GetEnvironmentVariable("CHAT_ID")), messageText.Text.Text);
            }
        }
    }
}
