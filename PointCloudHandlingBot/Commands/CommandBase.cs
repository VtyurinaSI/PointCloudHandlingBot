using PointCloudHandlingBot.MsgPipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudHandlingBot.Commands
{
    internal abstract class CommandBase
    {
        internal CommandBase(string name)
        {
            CommandName = name;
        }
        public List<string> ParamsDescription;
        internal int ParsePartsNum { get; set; }
        internal string CommandName { get; set; } 
        private protected List<string> ParseParts { get; set; } 
        public void SetParseParts(string textMsg) => ParseParts.Add(textMsg.Trim());

        public abstract List<IMsgPipelineSteps> Process(User user);
        private protected void InvokeSendConditionEvent(User user,  string msg)
        {
            SendConditionEvent?.Invoke(user, msg);
        }
        public delegate void SendConditionDelegate(User user, string msg);
        public event SendConditionDelegate? SendConditionEvent;
    }
}
