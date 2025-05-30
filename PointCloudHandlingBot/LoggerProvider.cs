using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace PointCloudHandlingBot
{
    public class LoggerProvider : ILoggerProvider
    {
        private readonly ITelegramBotClient _botClient;
        private readonly long _chatId;
        private readonly LogLevel _minLevel;

        public LoggerProvider(ITelegramBotClient botClient, long chatId, LogLevel minLevel = LogLevel.Information)
        {
            _botClient = botClient;
            _chatId = chatId;
            _minLevel = minLevel;
        }

        public ILogger CreateLogger(string categoryName)
            => new Logger(_botClient, categoryName, _minLevel);

        public void Dispose()
        {
        }
    }
}
