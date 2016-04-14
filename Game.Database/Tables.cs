using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Game.Data;
using Security;

namespace Game.Database
{
    // <Type Name = "CardInstance" >
    //   <Property Name="Id" Type="uuid" />
    //   <Property Name = "CardDataId" Type="uuid" />
    //   <Property Name = "Creator" Type="PublicKey"/>
    //   <Property Name = "Signature" Type="bytes"/>
    // </Type>

    class CardInstanceTable
    {

        [Obsolete("Die Statische Methode Nutzen", true)]
        public CardInstanceTable()
        {

        }

        private CardInstanceTable(Guid id, Guid cardDataId, PublicKey key, byte[] signature)
        {
            Id = id;
            CardDataId = cardDataId;
            CreatorFK = key.PrimaryKey;
            Signature = signature;
        }

        [AutoIncrement, PrimaryKey]
        public int PrimaryKey { get; set; }
        public Guid Id { get; set; }
        public Guid CardDataId { get; set; }
        public int CreatorFK { get; set; }
        public byte[] Signature { get; set; }
        [Ignore]
        public Task<PublicKey> Creator
        {
            get
            {
                return Database.db.Table<PublicKey>().Where(x => x.PrimaryKey == CreatorFK).FirstAsync();
            }
        }

        internal async static Task<CardInstanceTable> GetCardInstance(Guid id, PublicKey key)
        {
            var asyncTableQuery = Database.db.Table<CardInstanceTable>();
            var where = asyncTableQuery.Where(x => x.Id == id && x.CreatorFK == key.PrimaryKey);
            var erg = await where.FirstOrDefaultAsync();

            return erg;
        }

        internal async static Task<bool> InserCardInstance(Guid id, Guid cardDataId, PublicKey key, byte[] signature)
        {
            var asyncTableQuery = Database.db.Table<CardInstanceTable>();
            var where = asyncTableQuery.Where(x => x.Id == id && x.CreatorFK == key.PrimaryKey);
            var erg = await where.FirstOrDefaultAsync();

            if (erg == null)
            {
                erg = new CardInstanceTable(id, cardDataId, key, signature);
                await Database.db.InsertAsync(erg);
                return true;
            }
            else
                return false;
        }
    }



    //  <Type Name = "Ruleset " >
    //  < Property Name="Id" Type="uuid"/>
    //  <Property Name = "Creator" Type="PublicKey"/>
    //  <Property Name = "Name" Type="string"/>
    //  <Property Name = "Revision" Type="int32"/>
    //  <Property Name = "Script" Type="string"/>
    //  <Property Name = "MandatoryKeys" Type="KeyValue"/>
    //  <Property Name = "Signature" Type="bytes"/>
    //</Type>
    class RulesetTable
    {

        [Obsolete("Die Statische Methode Nutzen", true)]
        public RulesetTable()
        {

        }

        public RulesetTable(Guid id, PublicKey creator, int revision, string script, string name, byte[] signature)
        {
            Id = id;
            CreatorFK = creator.PrimaryKey;
            Revision = revision;
            Script = script;
            Name = name;
            Signature = signature;
        }

        [AutoIncrement, PrimaryKey]
        public int PrimaryKey { get; set; }
        public Guid Id { get; set; }
        public int CreatorFK { get; set; }
        public Int32 Revision { get; set; }
        public string Script { get; set; }
        public string Name { get; set; }
        public byte[] Signature { get; set; }
        [Ignore]
        public Task<PublicKey> Creator
        {
            get
            {
                return Database.db.Table<PublicKey>().Where(x => x.PrimaryKey == CreatorFK).FirstAsync();
            }
        }
        [Ignore]
        public Task<IEnumerable<Client.Game.Data.Keys>> MandetoryKeys
        {
            get
            {
                return Task.Run<IEnumerable<Client.Game.Data.Keys>>(async () =>
                {
                    var kv = await Database.db.Table<RulesetKeysTable>().Where(x => x.RulesetFK == PrimaryKey).ToListAsync();
                    return kv.Select(x => new Client.Game.Data.Keys() { Name = x.Name, Type = x.Type });
                });
            }
        }

        internal async static Task<RulesetTable> GetRuleset(Guid id, PublicKey creator)
        {
            var asyncTableQuery = Database.db.Table<RulesetTable>();
            var where = asyncTableQuery.Where(x => x.Id == id && x.CreatorFK == creator.PrimaryKey);
            var erg = await where.FirstOrDefaultAsync();

            return erg;
        }


        internal async static Task<IEnumerable<RulesetTable>> GetRulesets(PublicKey creator)
        {
            var asyncTableQuery = Database.db.Table<RulesetTable>();
            var where = asyncTableQuery.Where(x => x.CreatorFK == creator.PrimaryKey);
            var erg = await where.ToListAsync();

            return erg;
        }


        internal async static Task<bool> InserRuleset(Guid id, PublicKey creator, int revision, string script, string name, IEnumerable<Client.Game.Data.Keys> keyValues, byte[] signature)
        {
            var asyncTableQuery = Database.db.Table<RulesetTable>();
            var where = asyncTableQuery.Where(x => x.Id == id && x.CreatorFK == creator.PrimaryKey).OrderByDescending(x => x.Revision);
            var ergList = await where.ToListAsync();
            var erg = ergList.FirstOrDefault();
            if (revision <= (erg?.Revision ?? -1))
                return false;
            foreach (var item in ergList)
                await Database.db.DeleteAsync(item);


            using (await Database.semaphore.Enter())
            {
                try
                {
                    await Database.db.BeginTransaction();
                    var rule = new RulesetTable(id, creator, revision, script, name, signature);
                    await Database.db.InsertAsync(rule);

                    await Database.db.InsertAllAsync(keyValues.Select(x => new RulesetKeysTable() { Name = x.Name, RulesetFK = rule.PrimaryKey, Type = x.Type }));

                    await Database.db.Commit();
                    return true;

                }
                catch (Exception)
                {
                    await Database.db.Rollback();
                    return false;
                }
            }


        }


    }


    //<Type Name = "Keys" >
    //  < Property Name="Name" Type="string"/>
    //  <Property Name = "Type" Type="string"/>
    //</Type>

    class RulesetKeysTable
    {

        [AutoIncrement, PrimaryKey]
        public int PrimaryKey { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int RulesetFK { get; set; }

    }


    // <Type Name = "CardData" >
    //   < Property Name="Id" Type="uuid" />
    //   <Property Name = "Creator" Type="PublicKey"/>
    //   <Property Name = "Edition" Type="string" />
    //   <Property Name = "CardNumber" Type="int32" />
    //   <Property Name = "CardRevision" Type="int32" />
    //   <Property Name = "ImageId" Type="uuid" />
    //   <Property Name = "Name" Type="string" />
    //   <Property Name = "Values" Type="KeyValue" IsList="true" />
    //   <Property Name = "Signature" Type="bytes"/>
    // </Type>

    class CardDataTable
    {

        [Obsolete("Die Statische Methode Nutzen", true)]
        public CardDataTable()
        {

        }

        private CardDataTable(Guid id, PublicKey creator, string edition, int cardRevision, Guid imageId, string name, byte[] signature)
        {
            Id = id;
            CreatorFK = creator.PrimaryKey;
            Edition = edition;
            CardRevision = cardRevision;
            ImageId = imageId;
            Name = name;
            Signature = signature;
        }

        [AutoIncrement, PrimaryKey]
        public int PrimaryKey { get; set; }
        public Guid Id { get; set; }
        public int CreatorFK { get; set; }
        public string Edition { get; set; }
        public Int32 CardRevision { get; set; }
        public Guid ImageId { get; set; }
        public string Name { get; set; }
        public byte[] Signature { get; set; }
        [Ignore]
        public Task<PublicKey> Creator
        {
            get
            {
                return Database.db.Table<PublicKey>().Where(x => x.PrimaryKey == CreatorFK).FirstAsync();
            }
        }
        [Ignore]
        public Task<IEnumerable<Client.Game.Data.KeyValue>> Values
        {
            get
            {
                return Task.Run<IEnumerable<Client.Game.Data.KeyValue>>(async () =>
                {
                    var kv = await Database.db.Table<KeyValueTable>().Where(x => x.CardInstanceCreatorFk == CreatorFK && x.CardInstanceId == Id).ToListAsync();
                    return kv.Select(x => new Client.Game.Data.KeyValue() { Key = x.Key, Value = x.Value });
                });
            }
        }

        internal async static Task<CardDataTable> GetCardData(Guid id, PublicKey creator)
        {
            var asyncTableQuery = Database.db.Table<CardDataTable>();
            var where = asyncTableQuery.Where(x => x.Id == id && x.CreatorFK == creator.PrimaryKey).OrderByDescending(x => x.CardRevision);
            var erg = await where.FirstOrDefaultAsync();

            return erg;
        }

        internal async static Task<bool> InserCardInstance(Guid id, PublicKey creator, string edition, int cardRevision, Guid imageId, string name, IEnumerable<Client.Game.Data.KeyValue> keyValues, byte[] signature)
        {
            var asyncTableQuery = Database.db.Table<CardDataTable>();
            var where = asyncTableQuery.Where(x => x.Id == id && x.CreatorFK == creator.PrimaryKey).OrderByDescending(x => x.CardRevision);
            var erg = await where.FirstOrDefaultAsync();


            if (erg == null || cardRevision > erg.CardRevision)
            {
                using (await Database.semaphore.Enter())
                {
                    try
                    {
                        await Database.db.BeginTransaction();
                        erg = new CardDataTable(id, creator, edition, cardRevision, imageId, name, signature);
                        await Database.db.InsertAsync(erg);

                        var oldKeys = await Database.db.Table<KeyValueTable>().Where(x => x.CardInstanceId == id && x.CardInstanceCreatorFk == creator.PrimaryKey).ToListAsync();
                        foreach (var toDelete in oldKeys)
                            await Database.db.DeleteAsync(toDelete);

                        await Database.db.InsertAllAsync(keyValues.Select(x => new KeyValueTable() { CardInstanceCreatorFk = creator.PrimaryKey, CardInstanceId = id, Key = x.Key, Value = x.Value }));

                        await Database.db.Commit();
                        return true;

                    }
                    catch (Exception)
                    {
                        await Database.db.Rollback();
                        return false;
                    }
                }


            }
            else
                return false;
        }
        // <Type Name = "KeyValue" >
        //   < Property Name="Key" Type="string" />
        //   <Property Name = "Value" Type="string" />
        // </Type>
        internal class KeyValueTable
        {
            [AutoIncrement, PrimaryKey]
            public int PrimaryKey { get; set; }
            public Guid CardInstanceId { get; set; }
            public int CardInstanceCreatorFk { get; set; }
            public string Key { get; set; }
            public string Value { get; set; }

        }
    }





    // <Type Name = "ImageData" >
    //   < Property Name="Id" Type="uuid" />
    //   <Property Name = "Image" Type="bytes" />
    //   <Property Name = "Creator" Type="PublicKey"/>
    //   <Property Name = "Signatur" Type="bytes"/>
    // </Type>

    class ImageDataTable
    {

        [Obsolete("Die Statische Methode Nutzen", true)]
        public ImageDataTable()
        {

        }

        private ImageDataTable(Guid id, byte[] image, PublicKey creator, byte[] signature)
        {
            Id = id;
            Image = image;
            CreatorFK = creator.PrimaryKey;
            Signature = signature;
        }

        [AutoIncrement, PrimaryKey]
        public int PrimaryKey { get; set; }
        public Guid Id { get; set; }
        public byte[] Image { get; set; }
        public int CreatorFK { get; set; }
        public byte[] Signature { get; set; }
        [Ignore]
        public Task<PublicKey> Creator
        {
            get
            {
                return Database.db.Table<PublicKey>().Where(x => x.PrimaryKey == CreatorFK).FirstAsync();
            }
        }

        internal async static Task<ImageDataTable> GetImageData(Guid id, PublicKey key)
        {
            var asyncTableQuery = Database.db.Table<ImageDataTable>();
            var where = asyncTableQuery.Where(x => x.Id == id && x.CreatorFK == key.PrimaryKey);
            var erg = await where.FirstOrDefaultAsync();

            return erg;
        }

        internal async static Task<bool> InserImageData(Guid id, byte[] image, PublicKey creator, byte[] signature)
        {
            var asyncTableQuery = Database.db.Table<ImageDataTable>();
            var where = asyncTableQuery.Where(x => x.Id == id && x.CreatorFK == creator.PrimaryKey);
            var erg = await where.FirstOrDefaultAsync();

            if (erg == null)
            {
                erg = new ImageDataTable(id, image, creator, signature);
                await Database.db.InsertAsync(erg);
                return true;
            }
            else
                return false;
        }
    }


    // <Type Name = "ServerId" >
    //   <Property Name = "Name" Type="string"/>
    //   <Property Name = "Key" Type="PublicKey"/>
    //   <Property Name = "Icon" Type="uuid"/>
    //   <Property Name = "Uri" Type="string"/>
    //   <Property Name = "Revision" Type="int32"/>
    //   <Property Name = "Signiture" Type="bytes"/>
    // </Type>
    class ServerIdTable
    {

        [Obsolete("Die Statische Methode Nutzen", true)]
        public ServerIdTable()
        {

        }

        private ServerIdTable(String name, PublicKey key, Guid icon, string uri, int revision, byte[] signature)
        {
            Name = name;
            KeyFK = key.PrimaryKey;
            Icon = icon;
            Uri = uri;
            Revision = revision;
            Signature = signature;
        }

        [AutoIncrement, PrimaryKey]
        public int PrimaryKey { get; set; }
        public string Name { get; set; }
        public int KeyFK { get; set; }
        public Guid Icon { get; set; }
        public string Uri { get; set; }
        public int Revision { get; set; }
        public byte[] Signature { get; set; }
        public bool? Trustworthy { get; set; }
        [Ignore]
        public Task<PublicKey> Key
        {
            get
            {
                return Database.db.Table<PublicKey>().Where(x => x.PrimaryKey == KeyFK).FirstAsync();
            }
        }

        internal async static Task SetTrustworty(PublicKey key, bool trustworthy)
        {
            var server = await GetServerId(key);
            if (server == null)
                throw new ArgumentException($"Public Key ist Nicht bekannt (Fingerprint={key.ToIPublicKey().FingerPrint()})");
            server.Trustworthy = trustworthy;
            await Database.db.UpdateAsync(server);
        }

        internal async static Task<bool> IsTrustworthy(PublicKey key)
        {
            var server = await GetServerId(key);
            if (server == null)
                throw new ArgumentException($"Public Key ist Nicht bekannt (Fingerprint={key.ToIPublicKey().FingerPrint()})");
            return server.Trustworthy.HasValue && server.Trustworthy.Value;
        }


        internal async static Task<ServerIdTable> GetServerId(PublicKey key)
        {
            var asyncTableQuery = Database.db.Table<ServerIdTable>();
            var where = asyncTableQuery.Where(x => x.KeyFK == key.PrimaryKey).OrderBy(x => x.Revision);
            var erg = await where.FirstOrDefaultAsync();

            foreach (var item in await where.Skip(1).ToListAsync())
                await Database.db.DeleteAsync(item);


            return erg;
        }

        internal async static Task<bool> InsertServerId(String name, PublicKey key, Guid icon, string uri, int revision, byte[] signature)
        {
            var asyncTableQuery = Database.db.Table<ServerIdTable>();
            var where = asyncTableQuery.Where(x => x.Revision == revision && x.KeyFK == key.PrimaryKey);
            var erg = await where.FirstOrDefaultAsync();

            if (erg == null)
            {
                erg = new ServerIdTable(name, key, icon, uri, revision, signature);
                await Database.db.InsertAsync(erg);
                return true;
            }
            else
                return false;
        }
    }

    class DeckTable
    {


        [Obsolete("Die Statische Methode Nutzen", true)]
        public DeckTable()
        {

        }

        private DeckTable(Guid id, string name)
        {
            this.PrimaryKey = id;
            Name = name;
        }

        [PrimaryKey]
        public Guid PrimaryKey { get; set; }
        public string Name { get; set; }
        [Ignore]
        internal Task<IEnumerable<DeckEntryTable>> Entrys
        {
            get
            {
                return Database.db.Table<DeckTable.DeckEntryTable>().Where(x => x.DeckFK == PrimaryKey).ToListAsync().ContinueWith(x => (IEnumerable<DeckEntryTable>)x.Result);
            }
        }

        internal async Task AddCard(CardInstanceTable instance)
        {
            if (await Database.db.Table<DeckEntryTable>().Where(x => x.DeckFK == this.PrimaryKey && x.CardInstanceFK == instance.PrimaryKey).CountAsync() != 0)
                return;

            var entry = new DeckEntryTable() { CardInstanceFK = instance.PrimaryKey, DeckFK = this.PrimaryKey };
            await Database.db.InsertAsync(entry);
        }
        internal async Task RemoveCard(CardInstanceTable instance)
        {
            var entry = await Database.db.Table<DeckEntryTable>().Where(x => x.DeckFK == this.PrimaryKey && x.CardInstanceFK == instance.PrimaryKey).FirstOrDefaultAsync();
            if (entry != null)
                await Database.db.DeleteAsync(entry);

        }

        private static System.Threading.DisposingUsingSemaphore semaphor = new System.Threading.DisposingUsingSemaphore();
        internal async static Task<bool> InsertDeckTable(Guid id, String name)
        {
            using (await semaphor.Enter())
            {

                var asyncTableQuery = Database.db.Table<DeckTable>();
                var where = asyncTableQuery.Where(x => x.PrimaryKey == id);
                var erg = await where.FirstOrDefaultAsync();

                if (erg == null)
                {
                    erg = new DeckTable(id, name);
                    await Database.db.InsertAsync(erg);
                    return true;
                }
                else
                    return false;
            }
        }
        internal async static Task<bool> RemoveDeckTable(Guid id)
        {
            var asyncEntryQuery = Database.db.Table<DeckTable.DeckEntryTable>();
            var cards = await asyncEntryQuery.Where(x => x.DeckFK == id).ToListAsync();
            foreach (var c in cards)
                await Database.db.DeleteAsync(c);

            var asyncTableQuery = Database.db.Table<DeckTable>();
            var where = asyncTableQuery.Where(x => x.PrimaryKey == id);
            var erg = await where.FirstOrDefaultAsync();
            if (erg != null)
            {
                await Database.db.DeleteAsync(erg);
                return true;
            }
            else
                return false;
        }
        internal async static Task<DeckTable> GetDeckTable(Guid id)
        {
            var asyncTableQuery = Database.db.Table<DeckTable>();
            var where = asyncTableQuery.Where(x => x.PrimaryKey == id);
            return await where.FirstOrDefaultAsync();
        }



        internal class DeckEntryTable
        {


            [AutoIncrement, PrimaryKey]
            public int PrimaryKey { get; set; }
            public int CardInstanceFK { get; set; }
            public Guid DeckFK { get; set; }

            [Ignore]
            public Task<CardInstanceTable> CardInstance
            {
                get
                {
                    return Database.db.Table<CardInstanceTable>().Where(x => x.PrimaryKey == CardInstanceFK).FirstAsync();
                }
            }
            [Ignore]
            public Task<DeckTable> Deck
            {
                get
                {
                    return Database.db.Table<DeckTable>().Where(x => x.PrimaryKey == DeckFK).FirstAsync();
                }
            }


        }

    }


}
