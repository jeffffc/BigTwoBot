using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotApi.Types;

namespace BigTwoBot.Models.Requests
{
    [Serializable]
    public class ChangeStatusRequest : IRequest
    {
        public RequestType Type { get; } = RequestType.ChangeStatus;
        public string User { get; set; } = null;
        public string Message { get; set; } = null;
        public string Query { get; set; } = null;
        public string NodeId { get; } = null;

        public bool? ShutDown { get; set; } = null;

        public ChangeStatusRequest(bool shutDown, string nodeId)
        {
            ShutDown = shutDown;
            NodeId = nodeId;
        }
    }
}
