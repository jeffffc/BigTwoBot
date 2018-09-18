using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigTwoBot.Models.Information
{
    [Serializable]
    public class PlayerJoinedInfo : IInfo
    {
        public InfoType Type { get; } = InfoType.PlayerJoined;
        public string GameId { get; set; }
        public int UserId { get; set; }

        public PlayerJoinedInfo(string gameId, int userId)
        {
            GameId = gameId;
            UserId = userId;
        }
    }
}
