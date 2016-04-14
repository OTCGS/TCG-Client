using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Client.Store.Game.Engine.Statemachine
{
    internal class PassivePlayerState : AbstracteState
    {
        public async override Task<AbstracteState> Execute(GameConnection connection)
        {
            while (connection.Engin.Other == connection.Engin.CurrentTurn)
            {
                var r = await connection.Peek();
                System.Diagnostics.Debug.WriteLine("Passive Player verarbeitet:" + r);
                var method = typeof(GameEngine).GetTypeInfo().DeclaredMethods.SingleOrDefault(x => x.GetCustomAttribute<MessageProcessorAttribute>() != null && x.GetCustomAttribute<MessageProcessorAttribute>().TargetMessage == r.GetType());
                System.Diagnostics.Debug.Assert(method != null, "Konnte keinen Code finden für: " + r);
                Task t = method.Invoke(connection.Engin, new Object[0]) as Task;
                await t;
            }
            
            return new EndOfTurnState();
        }
    }
}