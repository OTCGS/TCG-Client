interface GameRules {
    //[DataMember]
    //public GameDataRegions[] Regions { get; set; }
    GetRegions(): GameDataRegions[];

    //[DataMember]
    //public bool UsingOwnCards { get; set; } // wird  über IsDeckLegal mit leerem Deck getetst

    //[DataMember]
    //public TypeScript.TSScript DeterminatePlayerActions { get; set; }
    GetPlayerActions(): AbstractAction[];

    //[DataMember]
    //public TypeScript.TSScript DeterminateWinner { get; set; }
    DeterminateWinner(): Player;

    //[DataMember]
    //public TypeScript.TSScript InitGame { get; set; }
    Init(deckPlayer1: number[], deckPlayer2: number[]): void;

    //[DataMember]
    //public TypeScript.TSScript StandardFunctions { get; set; }

    //[DataMember]
    //public TypeScript.TSScript StartOfTurn { get; set; }
    StartOfTurn(): void;

    //[DataMember]
    //public TypeScript.TSScript EndOfTurn { get; set; }
    EndOfTurn(): void;

    IsDeckLegal(deck: CardData[]): string[];
}


declare class GameDataRegions {
    Name: string;
    Player: Player;
    VisibleForPlayer: Player;
    Position: number;
    RegionType: RegionType;
}

declare class CardData {
    Name: String;
    Values: List<KeyValue>;
}
declare class KeyValue {
    Key: string;
    Value: string;
}
declare class AbstractAction { }
declare class Stack extends List<MentalCard>{ }
declare class MentalCard {
    Type: number;
}

declare class List<T> {
    //public T this[int index] { get; set; }
    get_Item(index: number): T;
    Count: number;

    Contains(item: T): boolean;
    // Exists(match: (t: T) => boolean): boolean;
    // Find(match: (t: T) => boolean): T;
    // FindAll(match: (t: T) => boolean): List<T>;
    // FindIndex(startIndex: number, count: number, match: (t: T) => boolean): number;

    // FindLast(match: (t: T) => boolean): T;

    // FindLastIndex(startindex: number, count: number, match: (t: T) => boolean): number;
    GetRange(index: number, count: number): List<T>;
    IndexOf(item: T, index?: number, count?: number): number;
    Insert(index: number, item: T): void;
    InsertRange(index: number, collection: T[]);
    LastIndexOf(item: T, index?: number, count?: number): number;

    ToArray(): T[];
    // TrueForAll(match: (t: T) => boolean): boolean;
}

enum Player {
    None = 0,
    Player1 = 1,
    Player2 = 2,
    Any = 3
}
enum RegionType {
    Stack = 1,
    Hand = 2,
    Row = 3
}

/*
 * Liefert den Stack zurück welcher eine Region repräsentiert.
 */
declare function Region(player: Player, name: String): Stack;

/*
 * Spieler 1.
 */
declare var Player1: Player;

/*
 * Spieler 2.
 */
declare var Player2: Player;

/*
 * Kein Spieler
 */
declare var PlayerNone: Player;

/*
 * Alle Spieler.
 */
declare var PlayerAny: Player;

/*
 * Man selbst.
 */
declare var Me: Player;

/*
 * Der andere Spieler.
 */
declare var Other: Player;

/*
 * Gibt den Aktuellen Spieler zurück.
 */
declare function CurrentPlayer(): Player;

/* 
 * Beendete den Zug des Spielers.
 */
declare function EndTurn(): void;

/*
 * Zeigt eine/mehrere Karten einem/allen Spielern.
 * @param playerToShow Der Spieler dem die Karten gezeigt werden sollen.
 * @param positionPlayer Der Spieler dem die Region, in welcher sich die Karte befindet, zugeordnet ist.
 * @param positionRegionName Der Name der Region, in welchem sich die Karte befindet.
 * @param positionIndex Der Index der Karte in der Region.
 */
declare function ShowCard(playerToShow: Player, positionPlayer: Player[], positionRegionName: string[], positionIndex: number[]): void;

/*
 * Mischt einen Stapel (Region).
 * @param player Der Spieler dem die zu mischende Region zugeteilt ist.
 * @param name Der Name der zu mischenden Region.
 */
declare function ShuffleStack(player: Player, name: String): void;

/*
 * Verschiebt eine oder Mehrere Karten von einer Position zu einer anderen. Achtung jeder parameter muss die Selbe länge besitzen. Index 0 aller Parameter verschiebt die erste Karte bis Index n verschiebt die n+1te Karte. 
 * @param fromPositionPlayer Der Spieler welchem die QuellRegion zugeschrieben ist.
 * @param fromPositionRegionName Der Name der QuellRegion.
 * @param fromPositionIndex Der QuellIndex.
 * @param toPositionPlayer Der Spieler welchem die ZielRegion zugeschrieben ist.
 * @param toPositionRegionName Der Name der ZielRegion.
 * @param toPositionIndex Der ZielIndex.
 */
declare function MoveCard(fromPositionPlayer: Player[], fromPositionRegionName: string[], fromPositionIndex: number[], toPositionPlayer: Player[], toPositionRegionName: string[], toPositionIndex: number[]): void;

/*
 * Erzeugt eine/mehrer Karte und fügt diese dem Spiel hinzu. Achtung jeder parameter muss die Selbe länge besitzen. Index 0 aller Parameter erzeugt die erste Karte bis Index n erzeugt die n+1te Karte. In den Meisten Spielen werden die Karten zu Begin erstellt.
 * @param card Ein Array mit den zahlen der Karten welche erstellt werden sollen.
 * @param positionPlayer Der Player, welcher der Region zugeordnet ist, wo die Karte erstellt werden soll.
 * @param positionRegionName Der Name der Region in welcher die Karte erstellt werden soll.
 * @param positionIndex Der Index an der die Karte eingefügt werden soll.
 */
declare function AddCards(card: number[], positionPlayer: Player[], positionRegionName: string[], positionIndex: number[]): void;

/**
* Setzt den Text der angezeigt werden soll. Z.B. Punkte.
* @param text Der Text der Angezeigt werden soll.
*/
declare function ChangeText(text: string): void;


/**
* Setzt den Text der angezeigt werden soll. Z.B. Punkte.
* @param text Der Text der Angezeigt werden soll.
*/
declare function ShowMessage(text: string): void;


/*
 * Erzeugt eine Zufällige Natürliche Zahl.
 */
declare function Random(): number;

/**
 * Erzeugt eine Aktion welche der Spieler losgelößt von allem anderen machen kann. (z.B. Zugende)
 * @param name Der Name der Aktion.
 * @param description Die Beschreibung der Aktion.
 * @param func Die Funktion welche aufgerufen werden soll.
 */
declare function CreatePlayerAction(name: string, description: string, func: () => void): AbstractAction;

/**
 * Erzeugt eine Aktion welche zu dieser Region gehört.
 * @param regionPlayerNumber der zugehörige Spieler zur Region
 * @param regionName der Name der Region welche die Aktion bekommen soll.
 * @param name Der Name der Aktion.
 * @param description Die Beschreibung der Aktion.
 * @param callback Die Funktion welche aufgerufen werden soll.
 */
declare function CreateRegionAction(regionPlayerNumber: Player, regionName: string, name: string, description: string, callback: () => void): AbstractAction;

/**
 * Erzeugt eine Aktion die der Nutzer mit dieser Karte auslösen kann.
 * @param card Die Karte welche diese Aktion bekommen soll (z.B. Jede Karte auf der Hand bekommt die Aktion Auspielen)
 * @param name Der Name der zur Aktion angezeigt werden soll.
 * @param description die Beschreibung der Aktion.
 * @param callback Die Funktion welche aufgerufen werden soll wird Die Aktion ausgelößt.
 */
declare function CreateCardAction(card: MentalCard, name: string, description: string, callback: () => void): AbstractAction;

/**
 * Gibt die CardData zu einer bestimmten Nummer zurück. 
 * @param cardNumber Die Nummer der Karte von welcher man die Daten haben will.
 */
declare function GetCardData(cardNumber: number): CardData;

/**
 * Lädt eine Nummer, welche mit SetNumber gespeichert wurde.
 * @param name der Variablen.
 */
declare function GetNumber(name: string): number;

/**
 * Speichert eine Numer in einer Variablen ab. Im gegensatz zu einer normal definierten Variable weiß auch der andere Spieler das diese existiert, und kann auf sie zugreifen.
 * @param name der Variablenname.
 */
declare function SetNumber(name: string, value: number): void;

/**
 * Lädt einen String welcher mit SetString gespeichert wurde. 
 * @param name der Variablenname.
 */
declare function GetString(name: string): string;

/**
 * Speichert einen String in einer Variablen ab. Im gegensatz zu einer normal definierten Variable weiß auch der andere Spieler das diese Existiert, und kann auf sie zugreifen.
 * @param name der Variablenname.
 */
declare function SetString(name: string, value: string): void;

/**
 * Erzeugt eine Region.
 * @param player Der Spieler dem die Region zugeordnet sein kann. Kann auch Player.None oder Player.Any sein.
 * @param visibleForPlayer Welche Spieler karten in dieser Region automatisch sehen.
 * @param name Der Name der Region, muss pro player eindeutig sein.
 * @param position gibt an wie weit die Region von der Mitte entfernt ist.
 * @param regionType gibt an wie die Region dargestellt werden soll.
 */
declare function ConstructRegion(player: Player, visibleForPlayer: Player, name: string, position: number, regionType: RegionType): GameDataRegions;



/**
 * MyTestGame 
 */
class WarGamesRules implements GameRules {


    HAND: string = "Hand";
    DECK: string = "Deck";
    FELD: string = "Feld";
    ABLAGE: string = "Ablage";
    PLAYER_1_POINTS: string = "Player1Points";
    PLAYER_2_POINTS: string = "Player2Points";

    ROWSIZE: number = 3;
    
    //[DataMember]
    //public GameDataRegions[] Regions { get; set; }
    public GetRegions(): GameDataRegions[] {
        var regions: GameDataRegions[] = [];
        regions.push(ConstructRegion(Player.Player1, Player.Player1, this.HAND, 3, RegionType.Hand));
        regions.push(ConstructRegion(Player.Player2, Player.Player2, this.HAND, 3, RegionType.Hand));

        regions.push(ConstructRegion(Player.Player1, Player.None, this.DECK, 3, RegionType.Stack));
        regions.push(ConstructRegion(Player.Player2, Player.None, this.DECK, 3, RegionType.Stack));

        regions.push(ConstructRegion(Player.Player1, Player.Player1, this.FELD, 1, RegionType.Row));
        regions.push(ConstructRegion(Player.Player2, Player.Player2, this.FELD, 1, RegionType.Row));

        regions.push(ConstructRegion(Player.Player1, Player.Any, this.ABLAGE, 3, RegionType.Stack));
        regions.push(ConstructRegion(Player.Player2, Player.Any, this.ABLAGE, 3, RegionType.Stack));



        return regions;
    }

    //[DataMember]
    //public bool UsingOwnCards { get; set; } // wird  über IsDeckLegal mit leerem Deck getetst

    //[DataMember]
    //public TypeScript.TSScript DeterminatePlayerActions { get; set; }
    GetPlayerActions(): AbstractAction[] {
        var erg: AbstractAction[] = [];

        var hand = Region(Me, this.HAND);

        for (var i = 0; i < hand.Count; i++) {
            var help = function(j: MentalCard): () => void {
                return function() {
                    this.PlayCard(j);
                    EndTurn();
                }
            };

            var card = hand.get_Item(i);
            if (card.Type != undefined) {
                var data = GetCardData(card.Type);
                var mauData = this.GetElmentCardData(data);
                erg.push(CreateCardAction(hand.get_Item(i), data.Name + ' Spielen', 'Spielt eine Karte', help(hand.get_Item(i))));
            }
        }

        return erg;
    }

    //[DataMember]
    //public TypeScript.TSScript DeterminateWinner { get; set; }
    DeterminateWinner(): Player {
        var p1 = GetNumber(this.PLAYER_1_POINTS);
        var p2 = GetNumber(this.PLAYER_2_POINTS);

        var erg = Player.None;
        if (p1 >= 7)
            erg |= Player1;
        if (p2 >= 7)
            erg |= Player2;

        return erg;
    }

    //[DataMember]
    //public TypeScript.TSScript InitGame { get; set; }
    Init(deckPlayer1: number[], deckPlayer2: number[]): void {

        SetNumber(this.PLAYER_1_POINTS, 0);
        SetNumber(this.PLAYER_2_POINTS, 0);

        var index = [];
        var region = [];
        var player = [];
        for (var i = 0; i < deckPlayer1.length; i++) {
            index[i] = i;
            region[i] = this.DECK;
            player[i] = Player.Player1;
        }
        AddCards(deckPlayer1, player, region, index);

        index = [];
        region = [];
        player = [];
        for (var i = 0; i < deckPlayer2.length; i++) {
            index[i] = i;
            region[i] = this.DECK;
            player[i] = Player.Player2;
        }
        AddCards(deckPlayer2, player, region, index);

        ShuffleStack(Player.Player1, this.DECK);
        ShuffleStack(Player.Player2, this.DECK);
    
        // Karten Ziehen
        this.DrawCard(Player1, 3);
        this.DrawCard(Player2, 3);

    }

    DrawCard(player?: Player, count?: number): void {
        if (player === undefined)
            player = Me;
        if (count === undefined)
            count = 1;

        var zugstapel = Region(player, this.DECK);

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
            fromRegion[i] = this.DECK;
            toIndex[i] = i;
            toPlayer[i] = player;
            toRegion[i] = this.HAND;
        }

        MoveCard(fromPlayer, fromRegion, fromIndex, toPlayer, toRegion, toIndex);
    }

    PlayCard(card: MentalCard): void {
        var hand = Region(Me, this.HAND);
        var ablage = Region(Me, this.FELD);
        var index = hand.IndexOf(card);

        var mau = this.GetElmentCardData(GetCardData(card.Type));


        var fromIndex = [];
        var fromRegion = [];
        var fromPlayer = [];
        var toIndex = [];
        var toRegion = [];
        var toPlayer = [];

        fromIndex[0] = index;
        fromRegion[0] = this.HAND;
        fromPlayer[0] = Me;
        toIndex[0] = ablage.Count;
        toPlayer[0] = Me;
        toRegion[0] = this.FELD;

        MoveCard(fromPlayer, fromRegion, fromIndex, toPlayer, toRegion, toIndex);
    }

    //[DataMember]
    //public TypeScript.TSScript StandardFunctions { get; set; }

    //[DataMember]
    //public TypeScript.TSScript StartOfTurn { get; set; }
    StartOfTurn(): void {
        var deck = Region(Me, this.DECK);
        var ablage = Region(Me, this.ABLAGE);
        if (deck.Count == 0) {
            var fromIndex = [];
            var fromRegion = [];
            var fromPlayer = [];
            var toIndex = [];
            var toRegion = [];
            var toPlayer = [];

            for (var i = 0; i < ablage.Count; i++) {
                fromIndex[i] = 0;
                fromPlayer[i] = Me;
                fromRegion[i] = this.ABLAGE;
                toIndex[i] = i;
                toPlayer[i] = Me;
                toRegion[i] = this.DECK;
            }
            MoveCard(fromPlayer, fromRegion, fromIndex, toPlayer, toRegion, toIndex);
            ShuffleStack(Me, this.DECK);
        }
        this.DrawCard();
    }

    //[DataMember]
    //public TypeScript.TSScript EndOfTurn { get; set; }
    EndOfTurn(): void {
        if (Me == Player2 && Region(Me, this.FELD).Count >= this.ROWSIZE) {
            // Auswerten
            // erste Zahl in Variablennamen Spieler Zweite Karte.
            var m11 = Region(Player1, this.FELD).get_Item(0);
            var m21 = Region(Player2, this.FELD).get_Item(0);

            var m12 = Region(Player1, this.FELD).get_Item(1);
            var m22 = Region(Player2, this.FELD).get_Item(1);

            var c11 = this.GetElmentCardData(GetCardData(m11.Type));
            var c21 = this.GetElmentCardData(GetCardData(m21.Type));
            var c12 = this.GetElmentCardData(GetCardData(m12.Type));
            var c22 = this.GetElmentCardData(GetCardData(m22.Type));

            var v1 = this.CalculateAttackValue(c11, c12, c21);
            var v2 = this.CalculateAttackValue(c21, c22, c11);

            // Ergebniss bestimmen
            if (v1 > v2){
                SetNumber(this.PLAYER_1_POINTS, GetNumber(this.PLAYER_1_POINTS) + 1);
                ShowMessage("Punkt für Spieler 1.");
            }
            else if (v1 < v2)
            {
                SetNumber(this.PLAYER_2_POINTS, GetNumber(this.PLAYER_2_POINTS) + 1);
                ShowMessage("Punkt für Spieler 2.");                
            }
            else
            {
                ShowMessage("Unentschieden");                                
            }
            
            ChangeText("Spiler 1: " + GetNumber(this.PLAYER_1_POINTS)+"\nSpiler 2: "+GetNumber(this.PLAYER_2_POINTS));

            // Abräumen
            var ablage = Region(Player1, this.ABLAGE);

            var fromIndex = [];
            var fromRegion = [];
            var fromPlayer = [];
            var toIndex = [];
            var toRegion = [];
            var toPlayer = [];

            fromIndex[0] = 0;
            fromRegion[0] = this.FELD;
            fromPlayer[0] = Player1;
            toIndex[0] = ablage.Count;
            toPlayer[0] = Player1;
            toRegion[0] = this.ABLAGE;

            MoveCard(fromPlayer, fromRegion, fromIndex, toPlayer, toRegion, toIndex);

            ablage = Region(Player2, this.ABLAGE);

            fromIndex = [];
            fromRegion = [];
            fromPlayer = [];
            toIndex = [];
            toRegion = [];
            toPlayer = [];

            fromIndex[0] = 0;
            fromRegion[0] = this.FELD;
            fromPlayer[0] = Player2;
            toIndex[0] = ablage.Count;
            toPlayer[0] = Player2;
            toRegion[0] = this.ABLAGE;

            MoveCard(fromPlayer, fromRegion, fromIndex, toPlayer, toRegion, toIndex);

        }
        if (Region(Me, this.FELD).Count >= 2) {
            
            // Die Nächsten Karten Zeigen.
            var positionPlayer = [Player1, Player1, Player2, Player2];
            var positionRegionName = [this.FELD, this.FELD, this.FELD, this.FELD];
            var positionIndex = [0, 1, 0, 1];

        }
        else if (Region(Me, this.FELD).Count >= 2) {
            var positionPlayer = [Player1, Player2];
            var positionRegionName = [this.FELD, this.FELD];
            var positionIndex = [0, 0];
        }
        if (Me == Player2 && positionPlayer != null)
            ShowCard(PlayerAny, positionPlayer, positionRegionName, positionIndex)
    }

    CalculateAttackValue(attacker: ElementCardData, supporter: ElementCardData, defender: ElementCardData): number {
        var attack = attacker.Strength;
        if (this.IsEffective(defender.Element, attacker.Element) == 2)
            attack *= 2;
        if (attacker.Strength < supporter.Strength) {
            var supportEfectivness = this.IsEffective(defender.Element, supporter.Element);
            switch (supportEfectivness) {
                case 2:
                    attack += 2 * supporter.Strength;
                    break;
                case 1:
                    attack += 1.5 * supporter.Strength;
                    break;
            }
        }
        return attack;
    }

    IsDeckLegal(deck: CardData[]): string[] {
        var erg: string[] = [];
        if (deck.length != 10) {
            erg.push("Deckgröße muss exakt 10 sein.");
        }
        deck.forEach(card => {
            var elemet = false;
            var strength = false;

            for (var index = 0; index < card.Values.Count; index++) {
                var keyvalue = card.Values.get_Item(index);
                if (keyvalue.Key == "Element") {
                    elemet = true;
                    switch (keyvalue.Value) {
                        case "Holz":
                        case "Feuer":
                        case "Wasser":
                        case "Metall":
                        case "Erde":
                            break;
                        default:
                            erg.push("Die Karte " + card.Name + " Besitzt ein nicht unterstütztes Element (" + keyvalue.Value + ").")
                            break;
                    }
                }
                else if (keyvalue.Key == "Stärke") {
                    strength = true;
                    try {
                        parseInt(keyvalue.Value);
                    } catch (error) {
                        erg.push("Die Karte " + card.Name + " Besitzt einen ungeültigen Stärke Wert (" + keyvalue.Value + ").")
                    }
                }
            }

            if (!strength && !elemet)
                erg.push("Die Karte " + card.Name + " Besitzt weder die Eigenschaft Stärke noch Element.")
            else if (!elemet)
                erg.push("Die Karte " + card.Name + " Besitzt die Eigenschaft Element nicht.")
            if (!strength)
                erg.push("Die Karte " + card.Name + " Besitzt die Eigenschaft Stärke nicht.")

        });
        return erg;
    }


    GetElmentCardData(data: CardData): ElementCardData {
        var erg = new ElementCardData();

        for (var i = 0; i < data.Values.Count; i++) {
            var currentItem = data.Values.get_Item(i);
            if (currentItem.Key === "Stärke")
                erg.Strength = parseInt(currentItem.Value);
            else if (currentItem.Key === "Element") {
                switch (currentItem.Value) {
                    case "Holz":
                        erg.Element = CardElement.Wood;
                        break;
                    case "Feuer":
                        erg.Element = CardElement.Fire;
                        break;
                    case "Wasser":
                        erg.Element = CardElement.Water;
                        break;
                    case "Metall":
                        erg.Element = CardElement.Metal;
                        break;
                    case "Erde":
                        erg.Element = CardElement.Earth;
                        break;
                }
            }
        }
        return erg;
    }

    IsEffective(target: CardElement, attacker: CardElement): number {
        var elementArray = [CardElement.Wood, CardElement.Fire, CardElement.Earth, CardElement.Metal, CardElement.Water];

        var targetIndex = elementArray.indexOf(target);
        var excelent = elementArray[(targetIndex + 4) % elementArray.length];
        var good = elementArray[(targetIndex + 2) % elementArray.length];
        var bad = elementArray[(targetIndex + 3) % elementArray.length];
        var extreamBad = elementArray[(targetIndex + 1) % elementArray.length];

        switch (attacker) {
            case excelent:
                return 2;
            case good:
                return 1;
            case bad:
                return -1;
            case extreamBad:
                return -2;
            default:
                return 0;
        }

    }

}



enum CardElement {
    Fire,
    Water,
    Wood,
    Metal,
    Earth
}

class ElementCardData {
    Element: CardElement;
    Strength: number;
}
