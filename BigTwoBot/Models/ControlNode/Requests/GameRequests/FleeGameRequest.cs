using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotApi.Types;

namespace BigTwoBot.Models.Requests
{
    [Serializable]
    public class FleeGameRequest : IRequest
    {
        public RequestType Type { get; } = RequestType.FleeGame;
        public string User { get; set; }
        public string Message { get; set; } = null;
        public string Query { get; set; } = null;
        public string NodeId { get; } = null;

        public FleeGameRequest(User user, Message message)
        {
            User = JsonConvert.SerializeObject(user);
            Message = JsonConvert.SerializeObject(message);
        }
    }
}