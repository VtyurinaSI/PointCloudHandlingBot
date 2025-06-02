using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot.DataBaseTables
{
    internal class PointCloud
    {
        /// <summary>
        /// PRIMARY KEY
        /// </summary>
        public int PointCloudID { get; set; } 
       /// <summary>
       /// FOREIGN KEY на User
       /// </summary>
        public long UserChatID { get; set; }            
        public string OriginalFileName { get; set; }
        public string TelegramFileID { get; set; }
        public DateTime UploadTimestamp { get; set; }
    }
}

