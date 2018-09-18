using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigTwoBot.Models.Information
{
    [Serializable]
    public class StatusChangedInfo : IInfo
    {
        public InfoType Type { get; } = InfoType.StatusChanged;
        public bool ShuttingDown { get; set; }

        public StatusChangedInfo(bool shuttingDown)
        {
            ShuttingDown = shuttingDown;
        }
    }
}
