using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Standard Funktionen und Werte auf die aus JS zugegriffen werden kann.
//function Region(player, name) { return []; };
//var Player1;
//var Player2;
//var PlayerNone;
//var PlayerAny;
//var Me;
//var Other;
//function CurrentPlayer() { };
//function EndTurn() { };
//function ShowCard(playerToShow, positionPlayer, positionRegionName, positionIndex) { };
//function ShuffleStack(player, name) { };
//function PermutateStac(player, name) { };
//function MoveCard(fromPositionPlayer, fromPositionRegionName, fromPositionIndex, toPositionPlayer, toPositionRegionName, toPositionIndex) { };
//function AddCards(card, positionPlayer, positionRegionName, positionIndex) { };
//function EndTurn() { };
//function CreatePlayerAction(name, description, func) { };
//function CreateRegionAction(regionPlayerNumber, regionName, name, description, callback) { };
//function CreateCardAction(card, name, description, callback) { };

namespace Client.Game.Data.Games
{
//    internal class SampleTCG : GameData
//    {
//        public SampleTCG()
//        {
//            this.StandardCards = Enumerable.Empty<Data.CardData>();
//            this.Regions = new GameDataRegions[] {
//                new GameDataRegions(PlayerNumber.Any, PlayerNumber.None,"DECK", 2,  RegionType.Stack),
//                new GameDataRegions(PlayerNumber.Any, PlayerNumber.Any,"FIELD", 1, RegionType.Row),
//                new GameDataRegions(PlayerNumber.Player1, PlayerNumber.Player1,"HAND", 2,   RegionType.Hand),
//                new GameDataRegions(PlayerNumber.Player2, PlayerNumber.Player2,"HAND", 2, RegionType.Hand),
//                new GameDataRegions(PlayerNumber.Any, PlayerNumber.Any,"DISCARD", 1, RegionType.Row),
//            };
//            this.InitGame = @"
//// Zusatzfunktionen Definieren
//function DrawCard(player, number) {
//    if (player === undefined)
//        player = Me;
//    if (number === undefined)
//        number = 1;

//    var fromIndex = [];
//    var fromRegion = [];
//    var fromPlayer = [];
//    var toIndex = [];
//    var toRegion = [];
//    var toPlayer = [];

//    for (var i = 0; i < number; i++) {
//        fromIndex[i] = Region(player, 'DECK').Count-1-i;
//        fromPlayer[i] = player;
//        fromRegion[i] = 'DECK';
//        toIndex[i] = i;
//        toPlayer[i] = player;
//        toRegion[i] = 'HAND';
//    }

//    MoveCard(fromPlayer, fromRegion, fromIndex, toPlayer, toRegion, toIndex);
//}

//function PlayCard(card) {
//    var hand = Region(Me, 'HAND');
//    var index = hand.UndexOf(card);

//    var fromIndex = [];
//    var fromRegion = [];
//    var fromPlayer = [];
//    var toIndex = [];
//    var toRegion = [];
//    var toPlayer = [];

//    fromIndex[0] = index;
//    fromRegion[0] = 'HAND';
//    fromPlayer[0] = Me;
//    toIndex[0] = 0;
//    toPlayer[0] = Me;
//    toRegion[0] = 'DECK';

//    MoveCard(fromPlayer, fromRegion, fromIndex, toPlayer, toRegion, toIndex);
//}

//function MoveSingelCard(fromp, fromn,fromi,top,ton,toi) {
//    var fromPlayer = [];
//    var fromRegion = [];
//    var fromIndex = [];
//    var toPlayer = [];
//    var toRegion = [];
//    var toIndex = [];

//    fromPlayer[0] = fromp;
//    fromRegion[0] = fromn;
//    fromIndex[0] = fromi;
//    toPlayer[0] = top;
//    toRegion[0] = ton;
//    toIndex[0] = toi;

//    MoveCard(fromPlayer, fromRegion, fromIndex, toPlayer, toRegion, toIndex);
//}

//// Decks Erstellen
//var index = [];
//var region = [];
//var player = [];
//for (var i = 0; i < deckPlayer1.length; i++) {
//    index[i] = i;
//    region[i] = 'DECK';
//    player[i] = Player1;
//}
//AddCards(deckPlayer1, player, region, index);

//for (var i = 0; i < deckPlayer2.length; i++) {
//    player[i] = Player2;
//}
//AddCards(deckPlayer2, player, region, index);

//ShuffleStack(Player1, 'DECK');
//ShuffleStack(Player2, 'DECK');

//// Karten Ziehen
//DrawCard(Player1, 5);
//DrawCard(Player2, 5);

//";
//            this.UsingOwnCards = true;
//            this.DeterminatePlayerActions = @"
//var erg = [];
//erg.push(CreatePlayerAction('Runde Beenden', 'Bendet die Runde', function () { EndTurn(); }));
//erg.push(CreateRegionAction(Me, 'DECK', 'Karte Ziehen', 'Zieht eine Karte', function () { DrawCard(); }));
//var hand = Region(Me,'HAND');
//for (var i = 0; i < hand.Count; i++) {
//var help=function(j){ return  function () { MoveSingelCard(Me,'HAND',j ,Me ,'FIELD',0); }};
//    erg.push(CreateCardAction(hand.get_Item(i), 'Karte Ziehen', 'Zieht eine Karte', help(i)));
//}
//return erg;
//";
//        }
//    }
}