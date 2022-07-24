using System.Text;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotTask
{
    internal class BotSettings
    {
        private int _filesCounter = 1;

        private int _voiceCounter = 1;

        private int _voiceCheckCounter = 1;

        private int _voiceDownloadCounter = 1;

        private Dictionary<string, Telegram.Bot.Types.File?> _files = new Dictionary<string, Telegram.Bot.Types.File?>();
        private Dictionary<string, Telegram.Bot.Types.File?> _music = new Dictionary<string, Telegram.Bot.Types.File?>();
        private Dictionary<string, Telegram.Bot.Types.File?> _voice = new Dictionary<string, Telegram.Bot.Types.File?>();

        private TelegramBotClient _botClient = new TelegramBotClient("5456733839:AAGN28AACaqqMvTtNUXKqAMBD-tvZkroCZQ");

        public async Task Initialize()
        {
            //await _botClient.SetMyCommandsAsync(command);
           
            using var cts = new CancellationTokenSource();

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };

            _botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            var me = await _botClient.GetMeAsync();

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            // Send cancellation request to stop bot
            cts.Cancel();
        }

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.CallbackQuery != null)
            {
                await DownloadFile(botClient, update.CallbackQuery.Data);

                await DownloadMusic(botClient, update.CallbackQuery.Data);

                await DownloadVoice(botClient, update.CallbackQuery.Data);
            }

            if (update.Message is not { } message)
                return;

            var chatId = message.Chat.Id;

            GetDocuments(botClient, chatId, update, cancellationToken, message);

            GetMusic(botClient, chatId, update, cancellationToken, message);

            GetVoice(botClient, chatId, update, cancellationToken, message);

            if (message.Text == "/check")
            {
                await commandCheck(botClient, update, chatId, cancellationToken);
            }

            if (message.Text == "/description")
            {
                _ = await botClient.SendTextMessageAsync(
                chatId,
                text: "This bot saves files, photos, music and voice messages \n after sending all necessary files execute /check command to download chosen files!",
                replyToMessageId: update.Message.MessageId,
                cancellationToken: cancellationToken
               );
            }

            if (message.Type == MessageType.Document)
            {
                return;
            }
        }
       

        private async Task commandCheck(ITelegramBotClient botClient, Update update, long chatId, CancellationToken cancellationToken)
        {
            List<List<InlineKeyboardButton>> inlineKeyboardButtons = new List<List<InlineKeyboardButton>>();
            int i = 0;

            foreach (var fileName in _files.Keys)
            {
                inlineKeyboardButtons.Add(new List<InlineKeyboardButton>() { InlineKeyboardButton.WithCallbackData(fileName, fileName) });
            }

            foreach (var musicName in _music.Keys)
            {
                inlineKeyboardButtons.Add(new List<InlineKeyboardButton>() { InlineKeyboardButton.WithCallbackData(musicName, musicName) });
            }

            foreach (var voiceName in _voice.Keys)
            {
                inlineKeyboardButtons.Add(new List<InlineKeyboardButton>() { InlineKeyboardButton.WithCallbackData($"Voice message # {_voiceCheckCounter++}", voiceName) });
            }

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(inlineKeyboardButtons);

            _ = await botClient.SendTextMessageAsync(
                chatId,
                text: "Download:",
                replyToMessageId: update.Message.MessageId,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken
               );

            _voiceCheckCounter = 1;
            _voiceDownloadCounter = 1;
        }


        async void GetDocuments(ITelegramBotClient botClient, long chatId, Update update, CancellationToken cancellationToken, Message message)
        {
            if (message.Type != MessageType.Document)
            {
                return;
            }

            foreach(var fileName in _files.Keys)
            {
                if (_files.ContainsKey(message.Document.FileName))
                {
                    _ = await botClient.SendTextMessageAsync(
                    chatId,
                    text: $"{message.Document.FileName} already exists!",
                    replyToMessageId: update.Message.MessageId,
                    cancellationToken: cancellationToken);

                    return;
                }
            }
            
            _files.Add(message.Document.FileName, await botClient.GetFileAsync(message.Document.FileId));

            
            _ = await botClient.SendTextMessageAsync(
                chatId,
                text: $"{_filesCounter++}) {message.Document.FileName} has been successfuly received!",
                replyToMessageId: update.Message.MessageId,
                cancellationToken: cancellationToken);
        }


        async void GetMusic(ITelegramBotClient botClient, long chatId, Update update, CancellationToken cancellationToken, Message message)
        {
            
            if (message.Type != MessageType.Audio)
            {
                return;
            }

            foreach (var musicName in _music.Keys)
            {
                if (_music.ContainsKey(message.Audio.FileName))
                {
                    _ = await botClient.SendTextMessageAsync(
                    chatId,
                    text: $"{message.Audio.FileName} already exists!",
                    replyToMessageId: update.Message.MessageId,
                    cancellationToken: cancellationToken);

                    return;
                }
            }

                _music.Add(message.Audio.FileName, await botClient.GetFileAsync(message.Audio.FileId));


                _ = await botClient.SendTextMessageAsync(
                    chatId,
                    text: $"{_filesCounter++}) {message.Audio.FileName} has been successfuly received!",
                    replyToMessageId: update.Message.MessageId,
                    cancellationToken: cancellationToken);
            
        }


        async void GetVoice(ITelegramBotClient botClient, long chatId, Update update, CancellationToken cancellationToken, Message message)
        {
            if (message.Type != MessageType.Voice)
            {
                return;
            }

            int counter_voices = 0;
            foreach (var voiceName in _voice.Keys)
            {
                if (_voice.ContainsKey(message.Voice.FileUniqueId))
                {
                    _ = await botClient.SendTextMessageAsync(
                    chatId,
                    text: $"This voice message already exists!",
                    replyToMessageId: update.Message.MessageId,
                    cancellationToken: cancellationToken);

                    return;
                }
            }

            _voice.Add(message.Voice.FileUniqueId, await botClient.GetFileAsync(message.Voice.FileId));

            _ = await botClient.SendTextMessageAsync(
                chatId,
                text: $"Voice message # {_voiceCounter++} has been successfuly received!",
                replyToMessageId: update.Message.MessageId,
                cancellationToken: cancellationToken);

            
        }


        private async Task DownloadFile(ITelegramBotClient botClient, string fileName)
        {
            foreach (var file in _files)
            {
                if (fileName != file.Key)
                {
                    continue;
                }

                using (FileStream Fs = new FileStream($"{file.Key}", FileMode.Create))
                {
                    await botClient.DownloadFileAsync(file.Value.FilePath, Fs);
                    Fs.Close();
                }
            }
        }


        private async Task DownloadMusic(ITelegramBotClient botClient, string musicName)
        {
            foreach (var soundtrack in _music)
            {
                if (musicName != soundtrack.Key)
                {
                    continue;
                }

                using (FileStream Fs = new FileStream($"{soundtrack.Key}", FileMode.Create))
                {
                    await botClient.DownloadFileAsync(soundtrack.Value.FilePath, Fs);
                    Fs.Close();
                }
            }
        }


        private async Task DownloadVoice(ITelegramBotClient botClient, string voiceMessageName)
        {
            foreach (var voiceMessage in _voice)
            {
                if (voiceMessageName != voiceMessage.Key)
                {
                    continue;
                }

                using (FileStream Fs = new FileStream($"Voice message # {_voiceDownloadCounter++}.ogg", FileMode.Create))
                {
                    await botClient.DownloadFileAsync(voiceMessage.Value.FilePath, Fs);
                    Fs.Close();
                }
            }
        }


        Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}