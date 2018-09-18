using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigTwoBot.Models.Information
{
    [Serializable]
    public class RefreshInfo : IInfo
    {
        public InfoType Type { get; } = InfoType.Refresh;
    }
}
