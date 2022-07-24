namespace TelegramBotTask
{
    internal class TelegramBotTask
    {
        static async Task Main()
        {
            await new BotSettings().Initialize();
        }
    }
}