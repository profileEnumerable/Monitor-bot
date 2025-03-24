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

            _tdClient.UpdateReceived += async (sender, update) =>
            {
                if (update is TdApi.Update.UpdateNewMessage newMessageFromChannel)
                {
                    if (newMessageFromChannel?.Message.Content is MessageText messageText)
                    {
                        await RepostIfMatchesKeywords(botClient, messageText);
                    }
                }
            };

            _tdClient.UpdateReceived += async (sender, update) =>
            {
                if (update is TdApi.Update.UpdateAuthorizationState authUpdate)
                {
                    await HandleAuth(authUpdate.AuthorizationState);
                }
            };
        }

        private async Task HandleAuth(AuthorizationState authState)
        {
            switch (authState)
            {
                case AuthorizationState.AuthorizationStateWaitTdlibParameters:
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
                    break;

                case AuthorizationState.AuthorizationStateWaitPhoneNumber:
                    _logger.LogInformation("Sending phone number ...");
                    await _tdClient.ExecuteAsync(
                        new SetAuthenticationPhoneNumber
                        {
                            PhoneNumber = Environment.GetEnvironmentVariable("PHONE_NUMBER")
                        });
                    break;

                //case AuthorizationState.AuthorizationStateWaitCode:
                //    Console.WriteLine("Введи код, що прийшов у Telegram:");
                //    string code = Console.ReadLine(); // ⚠️ для ASP.NET краще через API (див нижче)
                //    await _client.SendAsync(new TdApi.CheckAuthenticationCode
                //    {
                //        Code = code
                //    });
                //    break;

                case TdApi.AuthorizationState.AuthorizationStateReady:
                    Console.WriteLine("✅ Авторизовано успішно!");
                    break;

                default:
                    Console.WriteLine($"⚠️ Unknown auth state: {authState.GetType().Name}");
                    break;
            }
        }

        private static async Task RepostIfMatchesKeywords(TelegramBotClient botClient, MessageText messageText)
        {
            string[] keyWords = Environment.GetEnvironmentVariable("KEY_WORDS").Split(",");

            if (keyWords.Any(messageText.Text.Text.Contains))
            {
                await botClient.SendMessage(int.Parse(Environment.GetEnvironmentVariable("CHAT_ID")), messageText.Text.Text);
            }
        }
    }
}
