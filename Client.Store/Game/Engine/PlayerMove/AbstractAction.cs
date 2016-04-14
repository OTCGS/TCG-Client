using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Store.Game.Engine.PlayerMove
{
    public abstract class AbstractAction(String name, String description, Func<Task> perform)
    {

        public String Name { get; }= name;
        public String Description { get; }= description;
        public Func<Task> Perform { get; }= perform;

    }
}