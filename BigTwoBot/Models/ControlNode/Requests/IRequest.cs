using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotApi.Types;

namespace BigTwoBot.Models.Requests
{
    public interface IRequest
    {
        RequestType Type { get; }
        string User { get; set; }
        string Message { get; set; }
        string Query { get; set; }
        string NodeId { get; }
    }

    public enum RequestType
    {
        // Game related request types
        StartGame,
        JoinGame,
        ForceStart,
        FleeGame,
        Extend,
        KillGame,
        ShowDeck,
        Choice,

        // Control-node related request types
        Refresh,
        ChangeStatus,
        Ping,
    }
}
