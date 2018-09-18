using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigTwoBot.Models.Information
{
    [Serializable]
    public class GameEndedInfo : IInfo
    {
        public InfoType Type { get; } = InfoType.GameEnded;
        public string GameId { get; set; }

        public GameEndedInfo(string gameId)
        {
            GameId = gameId;
        }
    }
}
