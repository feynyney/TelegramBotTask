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
        private int _counter { get; set; } = 1;

        private List<string> _docNames = new List<string>();
        private List<string> _photoNames = new List<string>();
        private List<string> _soundNames = new List<string>();
        private string _checkAllFiles = String.Empty;
        private int _checkCounter = 1;

        private TelegramBotClient _botClient = new TelegramBotClient("5456733839:AAGN28AACaqqMvTtNUXKqAMBD-tvZkroCZQ");

        public async Task Initialise()
        {
            //await _botClient.SetMyCommandsAsync(list);
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
            if (update.Message is not { } message)
                return;

            var chatId = message.Chat.Id;

            


            var Keyboard = new[]
                {
                    new[]
                    {
                        new KeyboardButton("\U0001F601 Check")
                    },

                    new[]
                    {
                        new KeyboardButton("\U0001F601 Download")
                    }
                };

            var keyBoard = new ReplyKeyboardMarkup(Keyboard);
           

            //if (message.ReplyToMessage.Type == MessageType.Document)
            //{
            //    _ = await botClient.SendTextMessageAsync(
            //    chatId,
            //    text: "Yeah, hello you to!? \n",
            //    replyToMessageId: update.Message.MessageId,
            //    replyMarkup: keyBoard,
            //    cancellationToken: cancellationToken
            //   );
            //}

            GetDocuments(botClient, chatId, update, cancellationToken, message, _docNames);

            //if (message.Type == MessageType.Voice)
            //{
            //    Console.WriteLine(message.Voice.FileId);
            //    Console.WriteLine(message.Voice.Duration);
            //    Console.WriteLine(message.Voice.FileSize);

            //    var filename = @$"{message.Voice.Duration}.ogg"; //save voice message
            //    var file = await botClient.GetFileAsync(message.Voice.FileId);
            //    FileStream fs = new FileStream(filename, FileMode.Create);

            //    await botClient.DownloadFileAsync(file.FilePath, fs);
            //    fs.Close();
            //    fs.Dispose();

            //    Message Voicemessage;
            //    using (var stream = System.IO.File.OpenRead("13.ogg")) //return voice file.ogg
            //    {
            //        message = await botClient.SendAudioAsync(
            //            chatId: chatId,
            //            audio: stream,
            //            cancellationToken: cancellationToken);
            //    }
            //}

           


            

            if (message.Text == "/check" || message.Text == "\U0001F601 check")
            {
                await commandCheck(botClient, update, chatId, keyBoard, cancellationToken);
            }

           

            //foreach(var e in _docNames)
            //{
            //    if (message.Text == e)
            //    { 
            //        var file = await botClient.GetFileAsync(message.Text);
            //            FileStream fs = new FileStream(message.Document.FileName, FileMode.Create);
            //            await botClient.DownloadFileAsync(file.FilePath, fs);
            //            fs.Close();
            //            fs.Dispose();
            //    }
            //}

            //if(message.ReplyToMessage.Type == MessageType.)
            //{
            //    var file = await botClient.GetFileAsync(message.Document.FileId);
            //    FileStream fs = new FileStream(message.Document.FileName, FileMode.Create);
            //    await botClient.DownloadFileAsync(file.FilePath, fs);
            //    fs.Close();
            //    fs.Dispose();

            //}

            if (message.Text == "/download" || message.Text == "\U0001F601 download")
            {
                _ = await botClient.SendTextMessageAsync(
                chatId,
                text: "What file do you want to download? \n",
                replyToMessageId: update.Message.MessageId,
                replyMarkup: keyBoard,
                cancellationToken: cancellationToken
               );

                //foreach (var e in _docNames)
                //{
                //    if (message.Text == e)
                //    {

                //    }
                //}
            }
        }

        private async Task commandCheck(ITelegramBotClient botClient, Update update, long chatId, ReplyKeyboardMarkup keyBoard, CancellationToken cancellationToken)
        {
            foreach (var e in _docNames)
            {
                _checkAllFiles = _checkAllFiles + _checkCounter + ")" + e + "\n";
                _checkCounter++;
            }

            _ = await botClient.SendTextMessageAsync(
                chatId,
                text: $"{_checkAllFiles} - all your files!",
                replyToMessageId: update.Message.MessageId,
                replyMarkup: keyBoard,
                cancellationToken: cancellationToken
               );
            _checkAllFiles = String.Empty;
            _checkCounter = 1;
        }

        async void GetDocuments(ITelegramBotClient botClient, long chatId, Update update, CancellationToken cancellationToken, Message message, List<string> doc_name)
        {
            if (message.Type == MessageType.Document)
            {

               


                //Console.WriteLine(message.Document.FileId);
                //Console.WriteLine($"{message.Document.FileName} has been received successfuly!");
                doc_name.Add(message.Document.FileName);

                _ = await botClient.SendTextMessageAsync(
                    chatId,
                    text: $"{_counter++}) {message.Document.FileName} has been successfuly received!",
                    replyToMessageId: update.Message.MessageId,
                    cancellationToken: cancellationToken);


            }

            //var file = await botClient.GetFileAsync(message.Document.FileId);
            //FileStream fs = new FileStream("_" + message.Document.FileName + "has been downloaded successfuly", FileMode.Create);
            //await botClient.DownloadFileAsync(file.FilePath, fs);
            //fs.Close();
            //fs.Dispose();
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
