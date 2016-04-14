/**
 * MyTestGame 
 */
class MyTestGame implements GameRules {
        
    //[DataMember]
    //public GameDataRegions[] Regions { get; set; }
    GetRegions(): GameDataRegions[] {
        var regions: GameDataRegions[] = [];
        regions.push(
            ConstructRegion(Player.None, Player.Any, "Ablage", 0, RegionType.Stack),
            ConstructRegion(Player.Player1, Player.None, "Zugstapel", 1, RegionType.Stack),
            ConstructRegion(Player.Player2, Player.None, "Zugstapel", 1, RegionType.Stack),
            ConstructRegion(Player.Player1, Player.Player1, "Hand", 2, RegionType.Hand),
            ConstructRegion(Player.Player2, Player.Player2, "Hand", 2, RegionType.Hand)
            );
        return regions;
    }
    
    //[DataMember]
    //public TypeScript.TSScript DeterminatePlayerActions { get; set; }
    GetPlayerActions(): AbstractAction[] {
        var erg: AbstractAction[] = [];


        var topCard = this.GetMauMauData(this.TopCard());
        var hand = Region(Me, 'Hand');

        if (this.bauerSelect) {
            erg.push(CreatePlayerAction('Herz', 'Erzwingt als nächstes Herz', function () { SetNumber("Bauer", CardColor.Herz); EndTurn(); }));
            erg.push(CreatePlayerAction('Pick', 'Erzwingt als nächstes Pick', function () { SetNumber("Bauer", CardColor.Pick); EndTurn(); }));
            erg.push(CreatePlayerAction('Karo', 'Erzwingt als nächstes Karo', function () { SetNumber("Bauer", CardColor.Karo); EndTurn(); }));
            erg.push(CreatePlayerAction('Kreuz', 'Erzwingt als nächstes Kreuz', function () { SetNumber("Bauer", CardColor.Kreuz); EndTurn(); }));
        }
        else if (GetNumber("7") > 0 && !this.hasDrawn) {
            var cardCount = Region(Player.None, "Ablage").Count;
            var cardsToDraw = 2 * GetNumber("7");


            for (var i = 0; i < hand.Count; i++) {
                var help = function (j: MentalCard): () => void {
                    return function () {
                        this.PlayCard(j);
                        EndTurn();
                    }
                };

                var card = hand.get_Item(i);
                if (card.Type != undefined) {
                    var data = GetCardData(card.Type);
                    var mauData = this.GetMauMauData(data);
                    if (mauData.Wert == 7)
                        erg.push(CreateCardAction(hand.get_Item(i), data.Name + ' Spielen', 'Spielt eine Karte', help(hand.get_Item(i))));
                }
            }

            erg.push(CreateRegionAction(Me, 'Zugstapel', cardsToDraw + ' Karten Ziehen', 'Zieht ' + cardsToDraw + ' Karten', function () {
                this.DrawCard(Me, cardsToDraw);
                this.hasDrawn = true;
                SetNumber("7", 0);

            }));




        }
        else {

            for (var i = 0; i < hand.Count; i++) {
                var help = function (j: MentalCard): () => void {
                    return function () {
                        this.PlayCard(j);
                        var mau = this.GetMauMauData(GetCardData(j.Type));
                        if (mau.Wert == 11) {
                            this.bauerSelect = true;
                        }
                        else if ((mau.Wert != 8 && mau.Wert != 14) || hand.Count == 0)
                            EndTurn();
                    }
                };

                var card = hand.get_Item(i);
                if (card.Type != undefined) {
                    var data = GetCardData(card.Type);
                    var mauData = this.GetMauMauData(data);
                    if ((mauData.Farbe == topCard.Farbe && GetNumber("Bauer") < 0) || mauData.Farbe == GetNumber("Bauer") || (mauData.Wert == topCard.Wert && mauData.Wert != 11) || (mauData.Wert == 11 && topCard.Wert != 11))
                        erg.push(CreateCardAction(hand.get_Item(i), data.Name + ' Spielen', 'Spielt eine Karte', help(hand.get_Item(i))));
                }
            }


            if (this.hasDrawn) {
                erg.push(CreatePlayerAction('Zug beenden', 'Beendet den Zug', function () { EndTurn(); }));
            }
            else {
                erg.push(CreateRegionAction(Me, 'Zugstapel', 'Karte Ziehen', 'Zieht eine Karte', function () {
                    this.DrawCard();
                    this.hasDrawn = true;

                }));
            }
        }
        return erg;
    }
    
    //[DataMember]
    //public TypeScript.TSScript DeterminateWinner { get; set; }
    DeterminateWinner(): Player {
        var p1 = Region(Player.Player1, 'Hand').Count == 0 || Region(Player.Player2, 'Zugstapel').Count == 0;
        var p2 = Region(Player.Player2, 'Hand').Count == 0 || Region(Player.Player1, 'Zugstapel').Count == 0;
        if (p1 && !p2)
            return Player.Player1;
        if (!p1 && p2)
            return Player.Player2;

        if (p1 && p2)
            return Player.Any;
        return Player.None;

    }
    
    //[DataMember]
    //public TypeScript.TSScript InitGame { get; set; }
    Init(deckPlayer1: number[], deckPlayer2: number[]): void {
        // Decks Erstellen
        var index = [];
        var region = [];
        var player = [];
        for (var i = 0; i < deckPlayer1.length; i++) {
            index[i] = i;
            region[i] = 'Zugstapel';
            player[i] = Player.Player1;
        }
        AddCards(deckPlayer1, player, region, index);

        var index = [];
        var region = [];
        var player = [];
        for (var i = 0; i < deckPlayer2.length; i++) {
            index[i] = i;
            region[i] = 'Zugstapel';
            player[i] = Player.Player2;
        }
        AddCards(deckPlayer2, player, region, index);

        SetNumber("7", 0);
        SetNumber("Bauer", -1);

        ShuffleStack(Player.Player1, 'Zugstapel');
        ShuffleStack(Player.Player2, 'Zugstapel');
    
        // Karten Ziehen
        this.DrawCard(Player1, 5);
        this.DrawCard(Player2, 5);
        var zugstapel = Region(Player.Player2, 'Zugstapel');
        this.MoveSingelCard(Player.Player2, 'Zugstapel', zugstapel.Count - 1, Player.None, 'Ablage', 0);

    }
    
    //[DataMember]
    //public TypeScript.TSScript StandardFunctions { get; set; }
    
    //[DataMember]
    //public TypeScript.TSScript StartOfTurn { get; set; }
    StartOfTurn(): void {
        this.hasDrawn = false;
        this.bauerSelect = false;
    }
    
    //[DataMember]
    //public TypeScript.TSScript EndOfTurn { get; set; }
    EndOfTurn(): void {
    }

    IsDeckLegal(deck: CardData[]): boolean {
        if (deck.length <= 0)
            return false;
        for (var i = 0; i < deck.length; i++) {
            var data = deck[i];

            var hasWert: boolean = false;
            var hasFarbe: boolean = false;

            for (var i = 0; i < data.Values.Count; i++) {
                var currentItem = data.Values.get_Item(i);
                if (currentItem.Key === "Wert") {

                    var n = parseInt(currentItem.Value);
                    if (isNaN(n))
                        return false;
                    if (n < 7 || n > 14)
                        return false;
                    hasWert = true;
                    if (hasFarbe)
                        return true;
                }
                else if (currentItem.Key === "Farbe") {
                    switch (currentItem.Value) {
                        case "Herz":
                        case "Kreuz":
                        case "Karo":
                        case "Pick":
                            hasFarbe = true;
                            if (hasWert)
                                return true;
                            break;
                        default:
                            return false;
                    }
                }
            }



        }

    }   
        
        
        
    // Non interface Game Methods
       
       
    DrawCard(player?: Player, count?: number): void {
        if (player === undefined)
            player = Me;
        if (count === undefined)
            count = 1;

        var zugstapel = Region(player, 'Zugstapel');
        var ablage = Region(Player.None, 'Ablage');

        var fromIndex = [];
        var fromRegion = [];
        var fromPlayer = [];
        var toIndex = [];
        var toRegion = [];
        var toPlayer = [];

        count = Math.min(zugstapel.Count, count);


        var fromIndex = [];
        var fromRegion = [];
        var fromPlayer = [];
        var toIndex = [];
        var toRegion = [];
        var toPlayer = [];

        for (var i = 0; i < count; i++) {
            fromIndex[i] = zugstapel.Count - 1 - i;
            fromPlayer[i] = player;
            fromRegion[i] = 'Zugstapel';
            toIndex[i] = i;
            toPlayer[i] = player;
            toRegion[i] = 'Hand';
        }

        MoveCard(fromPlayer, fromRegion, fromIndex, toPlayer, toRegion, toIndex);
    }

    PlayCard(card: MentalCard): void {
        var hand = Region(Me, 'Hand');
        var ablage = Region(Player.None, 'Ablage');
        var index = hand.IndexOf(card);

        var mau = this.GetMauMauData(GetCardData(card.Type));
        if (mau.Wert == 7) {
            SetNumber("7", GetNumber("7") + 1);
        }
        else {
            SetNumber("7", 0);
        }
        if (mau.Wert != 11) {
            SetNumber("Bauer", -1);
        }

        var fromIndex = [];
        var fromRegion = [];
        var fromPlayer = [];
        var toIndex = [];
        var toRegion = [];
        var toPlayer = [];

        fromIndex[0] = index;
        fromRegion[0] = 'Hand';
        fromPlayer[0] = Me;
        toIndex[0] = ablage.Count;
        toPlayer[0] = Player.None;
        toRegion[0] = 'Ablage';

        MoveCard(fromPlayer, fromRegion, fromIndex, toPlayer, toRegion, toIndex);
    }

    MoveSingelCard(fromp: Player, fromn: string, fromi: number, top: Player, ton: string, toi: number): void {
        var fromPlayer = [];
        var fromRegion = [];
        var fromIndex = [];
        var toPlayer = [];
        var toRegion = [];
        var toIndex = [];

        fromPlayer[0] = fromp;
        fromRegion[0] = fromn;
        fromIndex[0] = fromi;
        toPlayer[0] = top;
        toRegion[0] = ton;
        toIndex[0] = toi;

        MoveCard(fromPlayer, fromRegion, fromIndex, toPlayer, toRegion, toIndex);
    }



    GetMauMauData(data: CardData): MauMauData {
        var erg = new MauMauData();

        for (var i = 0; i < data.Values.Count; i++) {
            var currentItem = data.Values.get_Item(i);
            if (currentItem.Key === "Wert")
                erg.Wert = parseInt(currentItem.Value);
            else if (currentItem.Key === "Farbe") {
                switch (currentItem.Value) {
                    case "Herz":
                        erg.Farbe = CardColor.Herz;
                        break;
                    case "Kreuz":
                        erg.Farbe = CardColor.Kreuz;
                        break;
                    case "Karo":
                        erg.Farbe = CardColor.Karo;
                        break;
                    case "Pick":
                        erg.Farbe = CardColor.Pick;
                        break;
                }
            }
        }

        return erg;

    }

    TopCard(): CardData {
        return this.CardFromTop(0);
    }

    CardFromTop(index: number): CardData {
        var stack = Region(Player.None, "Ablage");
        return GetCardData(stack.get_Item(stack.Count - 1 - index).Type);
    }

    hasDrawn = false;
    bauerSelect = false;

}

enum CardColor {
    Herz,
    Kreuz,
    Karo,
    Pick
}

class MauMauData {
    Farbe: CardColor;
    Wert: number;
}
