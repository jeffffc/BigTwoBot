using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotApi.Types;
using Newtonsoft.Json;

namespace BigTwoBot.Models.Requests
{
    [Serializable]
    public class ForceStartRequest : IRequest
    {
        public RequestType Type { get; } = RequestType.ForceStart;
        public string User { get; set; }
        public string Message { get; set; } = null;
        public string Query { get; set; } = null;
        public string NodeId { get; } = null;

        public ForceStartRequest(User user, Message message)
        {
            User = JsonConvert.SerializeObject(user);
            Message = JsonConvert.SerializeObject(message);
        }
    }
}