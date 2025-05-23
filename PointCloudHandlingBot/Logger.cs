using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PointCloudHandlingBot
{
    /// <summary>
    /// Класс для логирования процесса
    /// </summary>
    internal class Logger : ILogger
    {
        /// <summary>Минимальный уровень лога </summary>
        public static LogLevel MinLogLevel { get; set; } = LogLevel.Trace;

        public IDisposable? BeginScope<TState>(TState state) => null!;

        /// <summary>Должны ли логи этого уровня обрабатываться</summary>
        public bool IsEnabled(LogLevel logLevel) => logLevel >= MinLogLevel;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
          
        }
        /// <summary>Путь к файлу лога</summary>
        public static string LogDirectory ;//= Path.

        private static readonly SemaphoreSlim _slim = new(1, 1);
        private static string LogFileName => $"log_{DateTime.Today:yyyy_MM_dd}.txt";
    }
}
