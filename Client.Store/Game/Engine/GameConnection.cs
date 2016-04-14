using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Store.Game.Engine
{
    public class GameConnection : AbstractVerifiableConnection<object>
    {

        public int Seed = new System.Random().Next();


        public GameConnection(Network.IUserConnection connection, Network.User meUser) : base(connection, meUser, new byte[] { 0xAF })
        {
            // SPIELE DATEN LADEN

            this.Engin = new GameEngine(this, Viewmodel.CentralViewmodel.Instance.GameData);
        }

        private Data.GameHistory history = new Data.GameHistory();


        public readonly GameEngine Engin;


        private Statemachine.StatemachineCore Statemchine { get; } = new Statemachine.StatemachineCore();

        public async Task Init()
        {

            await Statemchine.Start(this);
        }
        internal async Task<T> Recive<T>()
        {
            return (T)await Recive();
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