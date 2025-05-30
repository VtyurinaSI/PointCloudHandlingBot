using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot.Commands
{
    internal static class CommandSimpleFactory
    {
        /// <summary>
        /// Создает экземпляр команды по имени
        /// </summary>
        /// <param name="commandName">Имя команды</param>
        /// <returns>Экземпляр команды</returns>
        public static CommandBase CreateCommand(string commandName, ILogger log, Keyboards kb)
        {
            return commandName switch
            {
                "/voxel" => new VoxelCmd(log, kb),
                _ => throw new ArgumentException($"Команда {commandName} не поддерживается.")
            };
        }
    }
}
