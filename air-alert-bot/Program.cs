using TdLib;
using Telegram.Bot;

class Program
{
    static async Task Main()
    {
        var botClient = new TelegramBotClient("");

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
                if (newMessageFromChannel?.Message.Content is TdApi.MessageContent.MessageText messageText)
                {
                    if (messageText.Text.Text == "")
                    {
                        await botClient.SendMessage(446128502, messageText.Text.Text);
                    }
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
                    ApiId = 25401799,
                    ApiHash = "89214c5fbe668ac9a2d2dc38723c3810",
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
