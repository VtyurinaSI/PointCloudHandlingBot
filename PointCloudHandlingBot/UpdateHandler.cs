using PointCloudHandlingBot.MsgPipeline;
using PointCloudHandlingBot.PointCloudProcesses;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static PointCloudHandlingBot.Program;
namespace PointCloudHandlingBot
{

    class UpdateHandler : IUpdateHandler
    {
        public UpdateHandler()
        {
            text = new();
            //text.OpenAnalizeKeyboardEvent += OpenPipelineKeyboard;
            //text.OpenColorKeyboardEvent += OpenColorKeyboard;
            //text.SendImageEvent += SendPlot;
            //text.OpenMainEvent += OpenBaseKeyboard;
            //text.SendTextEvent += SendLogToBot;
            file = new();
        }
        ITelegramBotClient bot;
        private readonly FileHandling file;
        

        public delegate Task MessageHandler(User user, string msg);
        public event MessageHandler? OnHandleUpdateCompleted;

        public event MessageHandler? OnHandleUpdateStarted;
        public async Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            await Task.Run(() => Console.WriteLine($"Ошибка: {exception.Message}\n{exception.StackTrace}"), cancellationToken);
        }
        private TextMessageHandling text;
        public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
        {
            this.bot = bot;
            string reply = string.Empty;


            User user = new(0);
            string? textMsg;
            if (update.Type == UpdateType.CallbackQuery)
            {
                var callbackQuery = update.CallbackQuery;
                textMsg = callbackQuery.Data;
                user = botUsers.GetOrAdd(update.CallbackQuery.Message.Chat.Id, id => new User(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.Chat.Username));

                text.WhatDoYouWant(bot, user, textMsg);
            }

            if (update.Message is not null)
            {
                user = botUsers.GetOrAdd(update.Message.Chat.Id, id => new User(update.Message.Chat.Id, update.Message.Chat.Username));

                textMsg = update.Message.Text;
                if (textMsg is not null)
                {
                    OnHandleUpdateStarted?.Invoke(user, textMsg); ;
                    text.WhatDoYouWant(bot, user, textMsg);
                }
                if (update.Message.Document is not null)
                {
                    var doc = update.Message.Document;
                    if (doc.FileName is null) return;
                    OnHandleUpdateStarted?.Invoke(user, $"Received file \"{doc.FileName}\"");
                    reply = await file.ReadFile(bot, user, doc);
                    var keyboard = new Keyboards ();
                    await Task.Run(() =>
                            MsgPipeLine.SendAll(botClient, user,
                                new ImageMsg(Drawing.Make3dImg),
                                new KeyboardMsg(keyboard.MainMenu)
                            ));
                }
            }
        }

        
        
    }
}