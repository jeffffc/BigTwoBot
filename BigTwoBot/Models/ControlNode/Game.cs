using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigTwoBot.Models
{
    public class Game
    {
        public string Id;
        public long GroupId;
        public string GroupTitle;
        public List<int> Players = new List<int>();

        public Game(string id, long groupId, string title)
        {
            Id = id;
            GroupId = groupId;
            GroupTitle = title;
        }
    }
}
