using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Store.Game.Engine.PlayerMove
{
    class PlayerAction( String name, String description, Func<Task> perform) : AbstractAction(name, description, perform)
    {
    }
}
