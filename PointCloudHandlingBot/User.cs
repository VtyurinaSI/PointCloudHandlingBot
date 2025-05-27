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
    public class User
    {
        public User(long chatId, string userName = "noname")
        {
            ChatId = chatId;
            UserName = userName ;
        }
        public long ChatId { get; set; }
        public string UserName { get; set; } = null!;

        public Func<float, float, float, Rgba32> ColorMap = Drawing.MapSpring;

        public UserPclFeatures OrigPcl { get; set; } = new();
        public UserPclFeatures? CurrentPcl 
        { get; 
            set; }
    }
}
