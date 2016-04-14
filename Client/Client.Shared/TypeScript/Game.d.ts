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
    Exists(match: (t: T) => boolean): boolean;
    Find(match: (t: T) => boolean): T;
    FindAll(match: (t: T) => boolean): List<T>;
    FindIndex(startIndex: number, count: number, match: (t: T) => boolean): number;

    FindLast(match: (t: T) => boolean): T;

    FindLastIndex(startindex: number, count: number, match: (t: T) => boolean): number;
    GetRange(index: number, count: number): List<T>;
    IndexOf(item: T, index?: number, count?: number): number;
    Insert(index: number, item: T): void;
    InsertRange(index: number, collection: T[]);
    LastIndexOf(item: T, index?: number, count?: number): number;

    ToArray(): T[];
    TrueForAll(match: (t: T) => boolean): boolean;
}

declare enum Player {
    None = 0,
    Player1 = 1,
    Player2 = 2,
    Any = 3
}
declare enum RegionType {
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

