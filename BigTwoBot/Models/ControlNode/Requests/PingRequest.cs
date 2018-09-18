using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotApi.Types;

namespace BigTwoBot.Models.Requests
{
    [Serializable]
    class PingRequest : IRequest
    {
        public RequestType Type { get; } = RequestType.Ping;
        public string User { get; set; } = null;
        public string Message { get; set; } = null;
        public string Query { get; set; } = null;
        public string NodeId { get; } = null;

        public PingRequest() { }
    }
}
