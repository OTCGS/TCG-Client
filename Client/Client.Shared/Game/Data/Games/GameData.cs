using Misc;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Client.CardServerService;
using Client.TypeScript;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Client.Game.Data.Games
{

    public class Ruleset
    {
        public PublicKey Creator { get; private set; }
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public int Revision { get; private set; }
        public TSScript Script { get; private set; }
        public byte[] Signature { get; private set; }
        internal RulesetKey[] MandatoryKeys { get; private set; }


        public Jint.Engine ScriptEngin { get; } = new Jint.Engine();


        //[DataMember]
        //public GameDataRegions[] Regions { get; set; }

        //[DataMember]
        //public bool UsingOwnCards { get; set; }

        //[DataMember]
        //public TypeScript.TSScript DeterminatePlayerActions { get; set; }

        //[DataMember]
        //public TypeScript.TSScript DeterminateWinner { get; set; }

        //[DataMember]
        //public TypeScript.TSScript InitGame { get; set; }

        //[DataMember]
        //public TypeScript.TSScript StandardFunctions { get; set; }

        //[DataMember]
        //public TypeScript.TSScript StartOfTurn { get; set; }

        //[DataMember]
        //public TypeScript.TSScript EndOfTurn { get; set; }

        //[DataMember]
        //public IEnumerable<Data.CardData> StandardCards { get; set; }

        //[DataMember]
        //public IEnumerable<Data.ImageData> StandardCardsImages { get; set; }



        public static implicit operator Ruleset(CardServerService.ruleSet set)
        {
            var newSet = new Ruleset();

            newSet.Id = Guid.Parse(set.id);
            newSet.Creator = set.creator;
            newSet.Name = set.name;
            newSet.Revision = set.revision;
            newSet.Script = new TypeScript.TSScript(set.script);
            newSet.MandatoryKeys = set.mandatoryKeys.Cast<RulesetKey>().ToArray();
            newSet.Signature = set.signature;
            if (!Security.SecurityFactory.CreatePublicKey().Veryfiy(newSet.Bytes(), newSet.Signature))
                throw new Exception("Wrong Signiture of Ruleset");
            return newSet;
        }

        public static implicit operator Ruleset(Client.Game.Data.Ruleset set)
        {
            var newSet = new Ruleset();

            newSet.Id = set.Id;
            newSet.Creator = set.Creator;
            newSet.Name = set.Name;
            newSet.Revision = set.Revision;
            newSet.Script = new TypeScript.TSScript(set.Script);
            newSet.MandatoryKeys = set.MandatoryKeys.Cast<RulesetKey>().ToArray();
            newSet.Signature = set.Signature;
            var key = Security.SecurityFactory.CreatePublicKey();
            key.SetKey(set.Creator.Modulus, set.Creator.Exponent);
            if (!key.Veryfiy(newSet.Bytes(), newSet.Signature))
                throw new Exception("Wrong Signiture of Ruleset");
            return newSet;
        }


        private byte[] Bytes()
        {
            return Id.ToBigEndianBytes().Concat(
                Creator.Bytes().Concat(
                Encoding.UTF8.GetBytes(Name)).Concat(
                Misc.BitConverter.GetBytes(Revision)).Concat(
                UTF8Encoding.UTF8.GetBytes(Script.TS)).Concat(
                MandatoryKeys.OrderBy(x => x.Name).Select(
                    x => x.Bytes()
                ).SelectMany(x => x))).ToArray();
        }



        private bool rulesLoaded;
        internal async Task PrepareRules()
        {
            if (rulesLoaded)
                return;
            rulesLoaded = true;
            var jsScript = await Script.JS;

            // Suche die Startklasse
            var regex = new Regex(@"(?<name>\w+)(\s+|(\s*/\*.*\*/\s*))implements(\s+|(\s*/\*.*\*/\s*))GameRules");
            var match = regex.Match(Script.TS);
            var classname = match.Groups["name"].Value;

            var script = ScriptEngin;

            // Initialisiern wir die Regeln Machen Beide
            await Task.Run(() =>
            {
                script.Execute(jsScript);
                script.Execute($"var {GAMERULSNAME} = new {classname}()");
            });
        }

        internal Jint.Native.JsValue InvokeGameRuleMethod(string methodName, params Jint.Native.JsValue[] parameter)
        {

            var objectInstance = GetJSRulesObject();
            var propertyDescriptor = objectInstance.GetProperty(methodName);
            return propertyDescriptor.Value.Value.Invoke(objectInstance, parameter);
        }

        internal Jint.Native.Object.ObjectInstance GetJSRulesObject()
        {
            var gameruels = ScriptEngin.GetValue(GAMERULSNAME);
            var objectInstance = gameruels.AsObject();
            return objectInstance;
        }

        private const string GAMERULSNAME = "___GAMERULES___";


        internal IEnumerable<string> IsDeckLeagal(IEnumerable<CardData> cardData)
        {
            var jsInstance = Jint.Native.JsValue.FromObject(ScriptEngin, cardData.ToArray());
            var jsValue = InvokeGameRuleMethod("IsDeckLegal", jsInstance);
            var objectArray = (object[])jsValue.ToObject();
            var erg = objectArray.Cast<String>().ToArray();
            return erg;
        }

        //public String Serelize()
        //{
        //    var ser = new DataContractSerializer(typeof(GameData));
        //    var wr = new MemoryStream();
        //    ser.WriteObject(wr, this);
        //    var array = wr.ToArray();
        //    var str = UTF8Encoding.UTF8.GetString(array, 0, array.Length);

        //    return str;
        //}

        //public static GameData Deserelize(string str)
        //{
        //    var ser = new DataContractSerializer(typeof(GameData));
        //    var b = UTF8Encoding.UTF8.GetBytes(str);
        //    var wr = new MemoryStream(b);
        //    var data = (GameData)ser.ReadObject(wr);
        //    return data;
        //}
    }

    class RulesetKey
    {
        public string Name { get; private set; }
        public string ValueType { get; private set; }

        internal byte[] Bytes()
        {
            return UTF8Encoding.UTF8.GetBytes(Name + ValueType);
        }

        public static implicit operator RulesetKey(ruleSetKey otherKey)
        {
            var newKey = new RulesetKey();
            newKey.Name = otherKey.name;
            newKey.ValueType = otherKey.valueType;

            return newKey;
        }
        public static implicit operator RulesetKey(Client.Game.Data.Keys otherKey)
        {
            var newKey = new RulesetKey();
            newKey.Name = otherKey.Name;
            newKey.ValueType = otherKey.Type;

            return newKey;
        }
    }

    public class GameDataRegions
    {

        public GameDataRegions(PlayerNumber player, PlayerNumber visibleForPlayer, string name, int position, RegionType regionType)
        {
            Name = name;
            Player = player;
            VisibleForPlayer = visibleForPlayer;
            Position = position;
            RegionType = regionType;
        }

        public string Name { get; set; }

        public PlayerNumber Player { get; set; }

        public PlayerNumber VisibleForPlayer { get; set; }

        public int Position { get; set; }

        public RegionType RegionType { get; set; }
    }
}
