using TdLib;
using Telegram.Bot;
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
            var botClient = new TelegramBotClient(Environment.GetEnvironmentVariable("TELEGRAM_TOKEN"));
            await botClient.SendMessage(int.Parse(Environment.GetEnvironmentVariable("CHAT_ID")), "Я працюю !");

            _tdClient.UpdateReceived += async (sender, update) =>
        {
            if (update is TdApi.Update.UpdateNewMessage newMessageFromChannel)
            {
                await botClient.SendMessage(int.Parse(Environment.GetEnvironmentVariable("CHAT_ID")), "Нове повідомлення !");
                if (newMessageFromChannel?.Message.Content is MessageText messageText)
                {
                    string[] keyWords = Environment.GetEnvironmentVariable("KEY_WORDS").Split(",");

                    if (keyWords.Any(keyWord => messageText.Text.Text.Contains(keyWord)))
                    {
                        await botClient.SendMessage(int.Parse(Environment.GetEnvironmentVariable("CHAT_ID")), messageText.Text.Text);
                    }
                }
            }
        };

            await Task.Delay(-1);
        }
    }
}
