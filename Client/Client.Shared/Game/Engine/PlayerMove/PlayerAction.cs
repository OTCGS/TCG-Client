using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Game.Engine.PlayerMove
{
    class PlayerAction: AbstractAction
    {
        public PlayerAction(String name, String description, Func<Task> perform) :base(name, description, perform)
        {

        }
    }
}
