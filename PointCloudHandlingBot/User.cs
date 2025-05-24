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
        public User(long chatId, string userName)
        {
            ChatId = chatId;
            UserName = userName;
        }
        public long ChatId { get; set; }
        public string UserName { get; set; } = null!;
        public List<Vector3>? PointCloud { get; set; }
        public List<Vector3>? Colors { get; set; }
        public PclLims PclLims { get; set; } = new();
    }
}
