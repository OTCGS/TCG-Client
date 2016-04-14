using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Store.Game.Engine.PlayerMove
{
    class RegionAction(Data.PlayerNumber player, String  region, String name, String description, Func<Task> perform) : AbstractAction(name, description, perform)
    {
        public String  Region { get; } = region;

        public  Data.PlayerNumber Player { get; }=player;

    }
}
