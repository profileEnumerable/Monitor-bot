using TdLib;
using Telegram.Bot;
using static TdLib.TdApi.MessageContent;

namespace AspNet_Air_Alert_Bot.Workers
{
    public class TelegramWorker : BackgroundService
    {
        private readonly ILogger<TelegramWorker> _logger;

        public TelegramWorker(ILogger<TelegramWorker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var botClient = new TelegramBotClient(Environment.GetEnvironmentVariable("TELEGRAM_TOKEN"));
            await botClient.SendMessage(int.Parse(Environment.GetEnvironmentVariable("CHAT_ID")), "Я працюю !");

            var tdClient = new TdClient();

            tdClient.UpdateReceived += async (sender, update) =>
            {
                if (update is TdApi.Update.UpdateAuthorizationState authUpdate)
                {
                    await HandleAuth(tdClient, authUpdate.AuthorizationState);
                }
            };

            tdClient.UpdateReceived += async (sender, update) =>
            {
                if (update is TdApi.Update.UpdateNewMessage newMessageFromChannel)
                {
                    await botClient.SendMessage(int.Parse(Environment.GetEnvironmentVariable("CHAT_ID")), "Нове повідомлення !");
                    if (newMessageFromChannel?.Message.Content is MessageText messageText)
                    {
                        string[] keyWords = Environment.GetEnvironmentVariable("KEY_WORDS").Split(",");

                        if (keyWords.Any(keyWord => messageText.Text.Text.Contains(keyWord)))
                        {
                        }
                        await botClient.SendMessage(int.Parse(Environment.GetEnvironmentVariable("CHAT_ID")), messageText.Text.Text);
                    }
                }
            };

            await Task.Delay(-1);
        }

        static async Task HandleAuth(TdClient client, TdApi.AuthorizationState state)
        {
            switch (state)
            {
                case TdApi.AuthorizationState.AuthorizationStateWaitTdlibParameters:
                    Console.WriteLine("Sending TDLib params ...");
                    await client.ExecuteAsync(new TdApi.SetTdlibParameters
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

                case TdApi.AuthorizationState.AuthorizationStateReady:
                    Console.WriteLine("You're authtorized!");
                    break;
            }
        }

    }

}
