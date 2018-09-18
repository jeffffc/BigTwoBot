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
    public class ChoiceRequest : IRequest
    {
        public RequestType Type { get; } = RequestType.Choice;
        public string User { get; set; }
        public string Message { get; set; } = null;
        public string Query { get; set; } = null;
        public string NodeId { get; } = null;
        public string GameId { get; set; } = null;
        public string[] Args { get; set; }

        public ChoiceRequest(string gameId, string[] args, User user, CallbackQuery query)
        {
            GameId = gameId;
            Args = args;
            User = JsonConvert.SerializeObject(user);
            Query = JsonConvert.SerializeObject(query);
        }
    }
}