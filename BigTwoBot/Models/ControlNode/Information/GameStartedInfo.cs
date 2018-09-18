using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigTwoBot.Models.Information
{
    [Serializable]
    public class GameStartedInfo : IInfo
    {
        public InfoType Type { get; } = InfoType.GameStarted;
        public long ChatId { get; set; }
        public string ChatTitle { get; set; }
        public string GameId { get; set; }

        public GameStartedInfo(string gameId, long chatId, string title)
        {
            GameId = gameId;
            ChatId = chatId;
            ChatTitle = title;
        }
    }
}
