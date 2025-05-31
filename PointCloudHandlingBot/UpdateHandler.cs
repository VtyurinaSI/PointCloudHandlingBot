using Microsoft.Extensions.Logging;
using PointCloudHandlingBot.MsgPipeline;
using PointCloudHandlingBot.PointCloudProcesses;
using System.Threading.Tasks;
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
        public UpdateHandler(ITelegramBotClient bot)
        {
            text = new();
            this.bot = bot;
            file = new();

        }
        LoggerProvider lp;
        Logger logger;
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
            var user = GetUserFromUpdate(update);
            if (user is null)
                return;

            List<IMsgPipelineSteps> message = [];
            switch (update.Type)
            {
                case UpdateType.CallbackQuery:
                    message = HandleCallbackQuery(update.CallbackQuery, user);
                    break;

                case UpdateType.Message:
                    if (update.Message is not null)
                        message = await HandleMessageAsync(update.Message, user);
                    break;
            }

            foreach (var msg in message)
                await msg.Send(bot, user);
        }

        private User? GetUserFromUpdate(Update update)
        {
            switch (update.Type)
            {
                case UpdateType.CallbackQuery when update.CallbackQuery?.Message != null:
                    {
                        var chatId = update.CallbackQuery.Message.Chat.Id;
                        var userName = update.CallbackQuery.Message.Chat.Username ?? "noname";
                        return botUsers.GetOrAdd(chatId, id => new User(chatId, userName));
                    }
                case UpdateType.Message when update.Message != null:
                    {
                        var chatId = update.Message.Chat.Id;
                        var userName = update.Message.Chat.Username ?? "noname";
                        return botUsers.GetOrAdd(chatId, id => new User(chatId, userName));
                    }
                default:
                    return null;
            }
        }

        private List<IMsgPipelineSteps> HandleCallbackQuery(CallbackQuery callbackQuery, User user)
        {
            var textMsg = callbackQuery.Data;
            OnHandleUpdateStarted?.Invoke(user, textMsg);
            lp = new(bot, user.ChatId);
            logger = (Logger)lp.CreateLogger("logs");
            return text.WhatDoYouWant(user, textMsg, (Logger)logger);
        }

        private async Task<List<IMsgPipelineSteps>> HandleMessageAsync(Message message, User user)
        {
            var textMsg = message.Text;
            lp = new(bot, user.ChatId);
            logger = (Logger)lp.CreateLogger("logs");
            if (textMsg is not null)
            {

                return text.WhatDoYouWant(user, textMsg, (Logger)logger);
            }
            if (message.Document is not null)
            {
                List<IMsgPipelineSteps> docRes = await HandleDocumentAsync(user, message.Document);
                return docRes;
            }
            return new List<IMsgPipelineSteps>
            {
                new TextMsg("Я не понимаю, что вы хотите от меня!"),
                new KeyboardMsg(Keyboards.MainMenu)
            };
        }

        private async Task<List<IMsgPipelineSteps>> HandleDocumentAsync(User user, Document doc)
        {
            if (doc.FileName is null) return new List<IMsgPipelineSteps>
            {
                new TextMsg("Ошибка в имени документа"),
                new KeyboardMsg(Keyboards.MainMenu)
            };
            user.FileName = doc.FileName;
            var reply = await file.ReadFile(bot, user, doc);
            ResetPcl.ResetPclHandle(user);
            return new List<IMsgPipelineSteps>
            {
                new TextMsg(reply),
                new ImageMsg(Drawing.Make3dImg),
                new HtmlMsg(),
                new KeyboardMsg(Keyboards.MainMenu)
            };
        }
    }
}