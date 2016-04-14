using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Game.Engine.PlayerMove
{
    class RegionAction : AbstractAction
    {

        public RegionAction(Data.PlayerNumber player, String region, String name, String description, Func<Task> perform) : base(name, description, perform)
        {
            Region = region;
            Player = player;
        }

        public String Region { get; }

        public Data.PlayerNumber Player { get; }

    }
}
