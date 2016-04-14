using Client.Game.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.Game.Engine.Statemachine
{
    internal class DetermBeginnerState : AbstracteState
    {
        private readonly WhoIsOdd oddOrEaven;

        public DetermBeginnerState(WhoIsOdd message)
        {
            this.oddOrEaven = message;
        }

        public async override Task<AbstracteState> Execute(GameConnectivity connection)
        {
            await connection.Engin.InitScriptEngine();
            var deck = await connection.Engin.SelectDeck();
            var goOn = deck == null ? DoWeGoOn.Abort : DoWeGoOn.GoOn;
            await connection.SendMessage(goOn);

            var otherGoOn = await connection.Recive<Data.DoWeGoOn>();

            if ((goOn | otherGoOn).HasFlag(DoWeGoOn.Abort))
                throw new Exception("Game Aborted because User Wants to :(");



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
            Logger.Information(connection.Engin.Me.ToString());


            return new InnitDataExchangeState(deck);
        }
    }
}