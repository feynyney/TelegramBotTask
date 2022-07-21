using System.Text;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotTask
{
    internal class TelegramBotTask
    {
        static async Task Main()
        {
            await new BotSettings().Initialise();
        }
    }
}