using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BigTwoBot.Models.Information;
using BigTwoBot.Models.Requests;

namespace BigTwoBot.Models
{
    [Serializable]
    public class ControlNodeMessage
    {
        public MessageType Type { get; set; }
        public IRequest Request { get; set; }
        public IInfo Info { get; set; }
        public string NodeId { get; set; } = null;

        public ControlNodeMessage(IRequest request, string nodeId = null)
        {
            Type = MessageType.Request;
            Request = request;
            NodeId = nodeId;
        }

        public ControlNodeMessage(IInfo info, string nodeId)
        {
            Type = MessageType.Info;
            Info = info;
            NodeId = nodeId;
        }
    }

    public enum MessageType
    {
        Request,
        Info
    }
}
