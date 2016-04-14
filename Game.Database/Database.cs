using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Misc;
using SQLite;
using System.IO;
using Client.Game.Data;

namespace Game.Database
{
    public static class Database
    {
        internal static readonly SQLiteAsyncConnection db;
        internal static readonly System.Threading.DisposingUsingSemaphore semaphore = new System.Threading.DisposingUsingSemaphore();

        private const string DB_NAME = "db.sqlite";

        static Database()
        {
#if NETFX_CORE
            var dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, DB_NAME);
#else 
            var dbPath = DB_NAME;
#endif
            db = new SQLiteAsyncConnection(dbPath);


            Init();
        }

        private static void Cleanup()
        {
            db.DropTableAsync<CardDataTable>().Wait();
            db.DropTableAsync<CardInstanceTable>().Wait();
            db.DropTableAsync<ImageDataTable>().Wait();
            db.DropTableAsync<ServerIdTable>().Wait();
            db.DropTableAsync<CardDataTable.KeyValueTable>().Wait();
            db.DropTableAsync<PublicKey>().Wait();
            db.DropTableAsync<RulesetKeysTable>().Wait();
            db.DropTableAsync<RulesetTable>().Wait();
            db.DropTableAsync<DeckTable>().Wait();
            db.DropTableAsync<DeckTable.DeckEntryTable>().Wait();
        }

        private static void Init()
        {
            db.CreateTableAsync<CardDataTable>().Wait();
            db.CreateTableAsync<CardInstanceTable>().Wait();
            db.CreateTableAsync<ImageDataTable>().Wait();
            db.CreateTableAsync<ServerIdTable>().Wait();
            db.CreateTableAsync<CardDataTable.KeyValueTable>().Wait();
            db.CreateTableAsync<PublicKey>().Wait();
            db.CreateTableAsync<RulesetKeysTable>().Wait();
            db.CreateTableAsync<RulesetTable>().Wait();
            db.CreateTableAsync<DeckTable>().Wait();
            db.CreateTableAsync<DeckTable.DeckEntryTable>().Wait();
        }


        public static async Task<bool> AddRuleset(Ruleset c)
        {
            var toValidate = c.Bytes().ToArray();
            var pk = Security.SecurityFactory.CreatePublicKey();

            pk.SetKey(c.Creator.Modulus, c.Creator.Exponent);
            Logger.Assert(pk.ValidParameter, "Public Key besitzt keine gültigen parameter.");

            if (!pk.Veryfiy(toValidate, c.Signature))
                throw new ArgumentException("Signiture not Valid in AddRuleset");
            var key = await PublicKey.GetKey(c.Creator);
            return await RulesetTable.InserRuleset(c.Id, key, c.Revision, c.Script, c.Name, c.MandatoryKeys, c.Signature);

        }

        public static async Task<bool> AddCardInstance(CardInstance c)
        {
            var toValidate = c.Bytes().ToArray();
            var pk = Security.SecurityFactory.CreatePublicKey();
            pk.SetKey(c.Creator.Modulus, c.Creator.Exponent);
            Logger.Assert(pk.ValidParameter, "Public Key besitzt keine gültigen parameter.");

            if (!pk.Veryfiy(toValidate, c.Signature))
                throw new ArgumentException($"Signiture not Valid. CardInstance({c.Id} {c.Creator})");

            var key = await PublicKey.GetKey(c.Creator);
            return await CardInstanceTable.InserCardInstance(c.Id, c.CardDataId, key, c.Signature);
        }
        public static async Task<CardInstance> GetCardInstance(Guid id, Client.Game.Data.PublicKey pk)
        {
            var key = await PublicKey.GetKey(pk);
            var i = await CardInstanceTable.GetCardInstance(id, key);
            if (i == null)
                return null;
            return new CardInstance() { Id = i.Id, CardDataId = i.CardDataId, Signature = i.Signature, Creator = (await i.Creator).ToGameData() };
        }

        public static async Task<bool> AddCardData(CardData c)
        {
            var toValidate = c.Bytes().ToArray();
            var pk = Security.SecurityFactory.CreatePublicKey();
            pk.SetKey(c.Creator.Modulus, c.Creator.Exponent);
            Logger.Assert(pk.ValidParameter, "Public Key besitzt keine gültigen parameter.");

            if (!pk.Veryfiy(toValidate, c.Signature))
                throw new ArgumentException($"Signiture not Valid CardData {c.Id}, {c.Creator}");

            var key = await PublicKey.GetKey(c.Creator);
            return await CardDataTable.InserCardInstance(c.Id, key, c.Edition, c.CardRevision, c.ImageId, c.Name, c.Values, c.Signature);
        }

        public static async Task<Ruleset> GetRuleset(Guid id, Client.Game.Data.PublicKey pk)
        {
            var key = await PublicKey.GetKey(pk);
            var i = await RulesetTable.GetRuleset(id, key);

            if (i == null)
                return null;

            var erg = new Ruleset() { Id = i.Id, Revision = i.Revision, Script = i.Script, Name = i.Name, Signature = i.Signature, Creator = (await i.Creator).ToGameData() };
            erg.MandatoryKeys.AddRange(await i.MandetoryKeys);
            return erg;
        }
        public static async Task<IEnumerable<Ruleset>> GetRulesets(Client.Game.Data.PublicKey pk)
        {
            var key = await PublicKey.GetKey(pk);
            var rs = await RulesetTable.GetRulesets(key);

            if (rs == null)
                return null;

            var erg = await Task.WhenAll(rs.Select(async i =>
           {
               var ruleset = new Ruleset() { Id = i.Id, Revision = i.Revision, Script = i.Script, Name = i.Name, Signature = i.Signature, Creator = (await i.Creator).ToGameData() };
               ruleset.MandatoryKeys.AddRange(await i.MandetoryKeys);
               return ruleset;
           }));


            return erg;
        }
        public static async Task<CardData> GetCardData(Guid id, Client.Game.Data.PublicKey pk)
        {
            var key = await PublicKey.GetKey(pk);
            var i = await CardDataTable.GetCardData(id, key);

            if (i == null)
                return null;

            var erg = new CardData() { Id = i.Id, CardRevision = i.CardRevision, Edition = i.Edition, ImageId = i.ImageId, Name = i.Name, Signature = i.Signature, Creator = (await i.Creator).ToGameData() };
            erg.Values.AddRange(await i.Values);
            return erg;
        }


        public static async Task<bool> AddImageData(ImageData c)
        {
            var toValidate = c.Bytes().ToArray();
            var pk = Security.SecurityFactory.CreatePublicKey();
            pk.SetKey(c.Creator.Modulus, c.Creator.Exponent);
            Logger.Assert(pk.ValidParameter, "Public Key besitzt keine gültigen parameter.");

            if (!pk.Veryfiy(toValidate, c.Signatur))
                throw new ArgumentException($"Signiture not Valid ImageData {c.Id}, {c.Creator}");

            var key = await PublicKey.GetKey(c.Creator);
            return await ImageDataTable.InserImageData(c.Id, c.Image, key, c.Signatur);
        }

        public static async Task<ImageData> GetImageData(Guid id, Client.Game.Data.PublicKey pk)
        {
            var key = await PublicKey.GetKey(pk);
            var i = await ImageDataTable.GetImageData(id, key);
            if (i == null)
                return null;
            var erg = new ImageData() { Id = i.Id, Image = i.Image, Signatur = i.Signature, Creator = (await i.Creator).ToGameData() };
            return erg;
        }

        public static async Task<bool> AddServerId(ServerId c)
        {
            var toValidate = c.Bytes().ToArray();
            var pk = Security.SecurityFactory.CreatePublicKey();
            pk.SetKey(c.Key.Modulus, c.Key.Exponent);
            Logger.Assert(pk.ValidParameter, "Public Key besitzt keine gültigen parameter.");

            if (!pk.Veryfiy(toValidate, c.Signiture))
                throw new ArgumentException($"Signiture not Valid ServerId  ({c.Name}, {c.Revision}, {c.Uri})");

            var key = await PublicKey.GetKey(c.Key);
            return await ServerIdTable.InsertServerId(c.Name, key, c.Icon, c.Uri, c.Revision, c.Signiture);
        }

        public static async Task<ServerId> GetServerId(Client.Game.Data.PublicKey pk)
        {
            var key = await PublicKey.GetKey(pk);
            var i = await ServerIdTable.GetServerId(key);
            if (i == null)
                return null;
            var erg = new ServerId() { Key = (await i.Key).ToGameData(), Icon = i.Icon, Name = i.Name, Revision = i.Revision, Signiture = i.Signature, Uri = i.Uri };
            return erg;
        }

        public static async Task<bool> IsServerIdTrustworthy(Client.Game.Data.PublicKey pk)
        {
            var key = await PublicKey.GetKey(pk);
            return await ServerIdTable.IsTrustworthy(key);
        }
        public static async Task SetServerIdTrustworthy(Client.Game.Data.PublicKey pk, bool trustworthy)
        {
            var key = await PublicKey.GetKey(pk);
            await ServerIdTable.SetTrustworty(key, trustworthy);
        }


        public static async Task<IEnumerable<ServerId>> GetServerIds()
        {
            var table = db.Table<ServerIdTable>();
            var list = await table.ToListAsync();

            var maxRevision = list.GroupBy(x => x.KeyFK).Select(x => x.OrderByDescending(y => y.Revision).FirstOrDefault());

            return await Task.WhenAll(maxRevision.Select(async i => new ServerId() { Key = (await i.Key).ToGameData(), Icon = i.Icon, Name = i.Name, Revision = i.Revision, Signiture = i.Signature, Uri = i.Uri }));
        }


        public static async Task<IEnumerable<Guid>> GetDeckIds()
        {
            var list = await db.Table<DeckTable>().ToListAsync();
            return list.Select(x => x.PrimaryKey);
        }
        public static async Task<string> GetDeckName(Guid id)
        {
            var element = await db.Table<DeckTable>().Where(x => x.PrimaryKey == id).FirstOrDefaultAsync();
            return element?.Name;
        }

        public static async Task CreateOrRenameDeck(Guid id, string name)
        {
            var element = await db.Table<DeckTable>().Where(x => x.PrimaryKey == id).FirstOrDefaultAsync();
            if (element != null)
            {
                element.Name = name;
                await db.UpdateAsync(element);
            }
            else
            {
                await DeckTable.InsertDeckTable(id, name);
            }
        }
        public static Task DeleteDeck(Guid id)
        {
            return DeckTable.RemoveDeckTable(id);
        }

        public static async Task AddCardToDeck(Guid id, CardInstance instance)
        {
            var deck = await DeckTable.GetDeckTable(id);

            var key = await PublicKey.GetKey(instance.Creator);
            var i = await CardInstanceTable.GetCardInstance(instance.Id, key);
            await deck.AddCard(i);
        }
        public static async Task RemoveCardToDeck(Guid id, CardInstance instance)
        {
            var deck = await DeckTable.GetDeckTable(id);

            var key = await PublicKey.GetKey(instance.Creator);
            var i = await CardInstanceTable.GetCardInstance(instance.Id, key);
            await deck.RemoveCard(i);
        }

        public static async Task<IEnumerable<CardInstance>> GetCardsOfDeck(Guid id)
        {
            var deck = await DeckTable.GetDeckTable(id);
            if (deck == null)
                return null;
            var cards = await deck.Entrys;
            var dbCardInstance = await Task.WhenAll(cards.Select(x => x.CardInstance));
            var instances = await Task.WhenAll(dbCardInstance.Select(async i => new CardInstance() { Id = i.Id, CardDataId = i.CardDataId, Signature = i.Signature, Creator = (await i.Creator).ToGameData() }));

            return instances;
        }



    }
}
