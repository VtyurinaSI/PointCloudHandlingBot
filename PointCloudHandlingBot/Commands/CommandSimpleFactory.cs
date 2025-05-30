using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace PointCloudHandlingBot.Commands
{
    internal static class CommandSimpleFactory
    {
        /// <summary>
        /// Создает экземпляр команды по имени
        /// </summary>
        /// <param name="commandName">Имя команды</param>
        /// <returns>Экземпляр команды</returns>
        public static CommandBase CreateCommand(string commandName, Logger log)
        {
            return commandName switch
            {
                "/voxel" => new VoxelCmd(log),
                "/DBSCANfilt" => new DBSCANfiltCmd(log),
                "/cluster" => new ClusterCmd(log),
                "/analyze" => new AnalyzeCmd(log),
                "/colorMap" => new AnalyzeCmd(log),
                _ => throw new ArgumentException($"Команда {commandName} не поддерживается.")
            };
        }
    }
}
