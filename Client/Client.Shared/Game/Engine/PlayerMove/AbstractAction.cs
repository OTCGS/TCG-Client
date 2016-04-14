using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Game.Engine.PlayerMove
{
    public abstract class AbstractAction
    {

        public AbstractAction(String name, String description, Func<Task> perform)
        {
            Name = name;
            Description = description;
            Perform = perform;
        }

        public String Name { get; }
        public String Description { get; }
        public Func<Task> Perform { get; }

    }
}