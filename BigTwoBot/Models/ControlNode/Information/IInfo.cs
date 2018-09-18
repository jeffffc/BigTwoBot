using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigTwoBot.Models.Information
{
    public interface IInfo
    {
        InfoType Type { get; }
    }

    public enum InfoType
    {
        GameStarted,
        GameEnded,
        PlayerFled,
        PlayerJoined,

        StatusChanged,
        Refresh,
    }
}
