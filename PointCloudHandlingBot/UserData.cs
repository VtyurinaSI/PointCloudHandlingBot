using OxyPlot;
using PointCloudHandlingBot.Commands;
using PointCloudHandlingBot.PointCloudProcesses;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot
{
    public class UserData
    {
        public UserData(long chatId, string userName = "noname")
        {
            ChatId = chatId;
            UserName = userName;
        }
        public long ChatId { get; set; }
        public string UserName { get; set; } = null!;

        public Func<float, float, float, OxyColor> ColorMap = Drawing.MapSpring;

        public string FileName { get; set; } = null!;
        public PclFeatures OrigPcl { get; set; } = new();

        public PclFeatures? CurrentPcl { get; set; }

        public CommandBase? Command { get; set; }

    }
}
