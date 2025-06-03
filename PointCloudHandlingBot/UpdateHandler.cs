using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using PointCloudHandlingBot.Configurate;
using PointCloudHandlingBot.DataBaseTables;
using PointCloudHandlingBot.Logging;
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
            db = new NpgsqlConnection(DBConnection.SqlConnectionString);
            lp = new(bot);
            logger = (Logger)lp.CreateLogger("logs");
        }
        private readonly System.Data.IDbConnection db;

        LoggerProvider lp;
        Logger logger;
        ITelegramBotClient bot;
        private readonly FileHandling file;


        public delegate Task MessageHandler(UserData user, string msg);
        public event MessageHandler? OnHandleUpdateCompleted;

        public event MessageHandler? OnHandleUpdateStarted;
        public async Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            await Task.Run(() => logger.LogBot($"{exception.Message}", LogLevel.Error, new(0000) { UserName = "bot" }));
        }
        private TextMessageHandling text;
        public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
        {
            var user = GetUserFromUpdate(update);
            if (user is null)
                return;
            db.Execute("""
        INSERT INTO Users (UserChatID, UserName)
        VALUES (@UserChatID, @UserName)
        ON CONFLICT (UserChatID) DO NOTHING;
    """, new DataBaseTables.User() { UserChatID = user.ChatId, UserName = user.UserName });

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

        private UserData? GetUserFromUpdate(Update update)
        {
            switch (update.Type)
            {
                case UpdateType.CallbackQuery when update.CallbackQuery?.Message != null:
                    {
                        var chatId = update.CallbackQuery.Message.Chat.Id;
                        var userName = update.CallbackQuery.Message.Chat.Username ?? "noname";
                        return botUsers.GetOrAdd(chatId, id => new UserData(chatId, userName));
                    }
                case UpdateType.Message when update.Message != null:
                    {
                        var chatId = update.Message.Chat.Id;
                        var userName = update.Message.Chat.Username ?? "noname";
                        return botUsers.GetOrAdd(chatId, id => new UserData(chatId, userName));
                    }
                default:
                    return null;
            }
        }

        private List<IMsgPipelineSteps> HandleCallbackQuery(CallbackQuery callbackQuery, UserData user)
        {
            var textMsg = callbackQuery.Data;
            OnHandleUpdateStarted?.Invoke(user, textMsg);
            lp = new(bot);
            return text.WhatDoYouWant(user, textMsg, (Logger)logger);
        }

        private async Task<List<IMsgPipelineSteps>> HandleMessageAsync(Message message, UserData user)
        {
            var textMsg = message.Text;
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

        private async Task<List<IMsgPipelineSteps>> HandleDocumentAsync(UserData user, Document doc)
        {
            user.CurrentPcl = null;
            if (doc.FileName is null) return new List<IMsgPipelineSteps>
            {
                new TextMsg("Ошибка в имени документа"),
                new KeyboardMsg(Keyboards.MainMenu)
            };
            user.FileName = doc.FileName;
            var (reply, extention) = await file.ReadFile(bot, user, doc);
            var pcl = new DataBaseTables.PointCloud()
            {
                UserChatID = user.ChatId,
                OriginalFileName = user.FileName,
                TelegramFileID = doc.FileId,
                UploadTimestamp = DateTime.Now,
                FileType = extention,
                InitialPointCount = user.OrigPcl.PointCloud.Count
            };
            db.Execute("""
        INSERT INTO pointclouds (UserChatID, OriginalFileName,TelegramFileID,UploadTimestamp,FileType,InitialPointCount)
        VALUES (@UserChatID, @OriginalFileName,@TelegramFileID,@UploadTimestamp,@FileType,@InitialPointCount);
    """, pcl);
            logger.LogBot($"Полуен файл {user.FileName}", LogLevel.Information, user);
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