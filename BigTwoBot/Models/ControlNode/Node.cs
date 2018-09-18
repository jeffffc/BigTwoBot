using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BigTwoBot.Models.Information;
using BigTwoBot.Models.Requests;

namespace BigTwoBot.Models
{
    public class Node
    {
        public string Id { get; }
        public DateTime StartTime { get; } = DateTime.Now;
        public List<Game> Games = new List<Game>();
        public bool ShuttingDown = false;

        public Node(string id)
        {
            this.Id = id;
        }

        public void SendRequest(IRequest request)
        {
            Program.Control.PushMessage(new ControlNodeMessage(request) { NodeId = Id });
        }

        public void ReceivedInfo(IInfo info)
        {
            switch (info.Type)
            {
                case InfoType.GameStarted:
                    var gsi = (GameStartedInfo)info;
                    Games.Add(new Game(gsi.GameId, gsi.ChatId, gsi.ChatTitle));
                    break;
                case InfoType.PlayerJoined:
                    var pji = (PlayerJoinedInfo)info;
                    Games.FirstOrDefault(x => x.Id == pji.GameId)?.Players.Add(pji.UserId);
                    break;
                case InfoType.PlayerFled:
                    var pfi = (PlayerFledInfo)info;
                    Games.FirstOrDefault(x => x.Id == pfi.GameId)?.Players.Remove(pfi.UserId);
                    break;
                case InfoType.GameEnded:
                    var gei = (GameEndedInfo)info;
                    Games.Remove(Games.First(x => x.Id == gei.GameId));
                    break;
                case InfoType.StatusChanged:
                    var sci = (StatusChangedInfo)info;
                    ShuttingDown = sci.ShuttingDown;
                    break;
                default:
                    break;
            }
        }
    }

}
