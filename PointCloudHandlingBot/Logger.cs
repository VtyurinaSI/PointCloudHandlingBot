using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PointCloudHandlingBot
{
    /// <summary>
    /// Класс для логирования процесса
    /// </summary>
    public class Logger : ILogger
    {
        private readonly ITelegramBotClient _botClient;
        private readonly string _category;
        private readonly LogLevel _minLevel;
        private  User _user;

        public Logger(
            ITelegramBotClient botClient,
            string category,
            LogLevel minLevel)
        {
            _botClient = botClient;
            _category = category;
            _minLevel = minLevel;
        }

        public IDisposable? BeginScope<TState>(TState state) => null!;

        /// <summary>Должны ли логи этого уровня обрабатываться</summary>
        public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel;
        public void LogBot(string consoleMessage, LogLevel logLevel,  User _user,string? botMessage = null)
        {
            this._user = _user;
            Log(
                logLevel,
                eventId: default,
                state: consoleMessage,
                exception: null,
                formatter: (state, ex) => consoleMessage);

            if (botMessage is not null )
            {
                
                _ = _botClient.SendMessage(_user.ChatId, botMessage);
            }
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;
            var text = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss:fff}][{_user.ChatId}][{_user.UserName}][{logLevel}] : {formatter(state, exception)}\n";
            WriteToFile(text);
            Console.Write(text);
        }
        public static void WriteToFile(string message)
        {
            EnsureLogDirectory();

            File.AppendAllText(LogFilePath, message);
        }

        private static void EnsureLogDirectory()
        {
            if (!Directory.Exists(LogDirectory))
                Directory.CreateDirectory(LogDirectory);
        }
        private static string LogFilePath => Path.Combine(LogDirectory, LogFileName);
        /// <summary>Путь к файлу лога</summary>
        public static string LogDirectory = "Logs";

        private static readonly SemaphoreSlim _slim = new(1, 1);
        private static string LogFileName => $"Log_{DateTime.Today:yyyy_MM_dd}.log";
    }
}
