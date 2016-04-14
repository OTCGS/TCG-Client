using Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Game.Engine
{
    public class GameConnectivity : AbstractVerifiableConnectivity<object>
    {

        public int Seed = new System.Random().Next();


        public GameConnectivity(Network.IUserConnection connection, Network.User meUser, Game.Data.Games.Ruleset ruleset) : base(connection, meUser, new byte[] { 0xAF })
        {
            if (connection.ConnectionReason != Network.ConnectionReason.Play)
                throw new ArgumentException("Der ConnectionReasen der Connection muss Play sein, war " + connection.ConnectionReason);



            this.Engin = new GameEngine(this, ruleset);
        }

        private Data.GameHistory history = new Data.GameHistory();


        public GameEngine Engin { get; }


        private Statemachine.StatemachineCore Statemchine { get; } = new Statemachine.StatemachineCore();

        public Task<Client.Game.Data.PlayerNumber> Init()
        {
            return Statemchine.Start(this);
        }
        internal async Task<T> Recive<T>()
        {
            return (T)await Recive();
        }

        internal new Task<object> Peek()
        {
            return base.Peek();
        }



        internal new Task SendMessage(object p)
        {
            return base.SendMessage(p);
        }

        protected override Task<object> ConvertFromByte(byte[] data)
        {
            var xml = UTF8Encoding.UTF8.GetString(data, 0, data.Length);
            return Task.FromResult(Data.Protokoll.Convert(xml));
        }

        protected override Task<byte[]> ConvertToByte(object data)
        {
            var str = Data.Protokoll.Convert(data);
            return Task.FromResult(Encoding.UTF8.GetBytes(str));
        }
    }
}