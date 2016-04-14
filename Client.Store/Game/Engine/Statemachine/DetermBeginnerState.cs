using Client.Store.Game.Data;
using System;
using System.Threading.Tasks;

namespace Client.Store.Game.Engine.Statemachine
{
    internal class DetermBeginnerState : AbstracteState
    {
        private readonly WhoIsOdd oddOrEaven;

        public DetermBeginnerState(WhoIsOdd message)
        {
            this.oddOrEaven = message;
        }

        public async override Task<AbstracteState> Execute(GameConnection connection)
        {
            uint coin;
            if (oddOrEaven == WhoIsOdd.MeOdd)
            {
                coin = await connection.Engin.GenerateRandom() & 1;
            }
            else
            {
                coin = await connection.Engin.ProcessGenerateRandom() & 1;
            }

            if ((coin == 1 && oddOrEaven == Data.WhoIsOdd.MeOdd) || (coin == 0 && oddOrEaven == Data.WhoIsOdd.YouOdd))
                connection.Engin.Me = PlayerNumber.Player1;
            else
                connection.Engin.Me = PlayerNumber.Player2;
            System.Diagnostics.Debug.WriteLine(connection.Engin.Me);


            return new InnitDataExchangeState();
        }
    }
}