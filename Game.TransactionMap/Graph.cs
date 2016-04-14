using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SQLite;
using System.IO;
using Client.Game.Data;
using Game.TransactionMap.ServiceMerger;

namespace Game.TransactionMap
{
    public static class Graph
    {
        internal static readonly SQLiteAsyncConnection db;
        private const string DB_NAME = "graph.sqlite";

        static Graph()
        {
#if NETFX_CORE
            var dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, DB_NAME);
#else 
            var dbPath = DB_NAME;
#endif
            db = new SQLiteAsyncConnection(dbPath);


            Init();

        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static async void SanityCheck()
        {
            await Task.Yield(); // Wir wollen so schnell wie möglich zum aufrufe zurück damit Die App initialisiert wird.
            using (await Graph.semaphore.Enter())
            {
                Logger.Information("Database Sanaty Check");
                var allTransfares = await db.Table<Transaction>().ToListAsync();
                foreach (var item in allTransfares)
                {
                    var hash = await item.CheckHash();
                    var sig = await item.CheckSig();
                    var prevTranksaction = await item.CheckPreTransactions();

                    if (!(hash && sig && prevTranksaction))
                        Logger.Failure($"Database Sanaty Failure Primary Key = {item.PrimaryKey}: Hash={hash} Signature={sig} Preview Tranksactions {prevTranksaction}");
                }
                Logger.Information("Database Sanaty Check End");

            }
        }

        public static async Task Merge(ServiceMerger.IServiceMerger servicePortClient)
        {
            var otherheads = (await servicePortClient.GetHeads()).ToDictionary(x => x.Hash, new ByteEnumComparer());
            var myHeads = (await GetHeads()).ToDictionary(x => x.Hash, new ByteEnumComparer());

            // Entferne alle Heads welche bei beiden Identisch sind.
            foreach (var k in myHeads.Keys.ToArray())
            {
                if (otherheads.Remove(k))
                    myHeads.Remove(k);
            }



            var transactionTable = db.Table<Transaction>();
            var transfareTable = db.Table<Transfare>();

            // var otherHeadsWhereIAmFurther = (await Task.WhenAll((await Task.WhenAll(otherheads.Select(async x => new { Value = x, Contains = await transactionTable.Where(y => y.Hash == x.Key.ToArray()).CountAsync() >= 0 }))).Where(x => x.Contains).Select(x => x.Value).Select(x => transactionTable.Where(y => y.Hash == x.Value.Hash).FirstAsync()));
            // var otherHeadsWhereIAmFurther = (await Task.WhenAll((await Task.WhenAll(otherheads.Select(async x => new { Value = x, Contains = await transactionTable.Where(y => y.Hash == x.Key.ToArray()).CountAsync() >= 0 }))).Where(x => x.Contains).Select(x => x.Value).Select(x => transactionTable.Where(y => y.Hash == x.Value.Hash).FirstAsync()));

            var otherHeadsDB = new HashSet<Transaction>((await Task.WhenAll(otherheads.Select(x => transactionTable.Where(y => y.Hash == x.Value.Hash).FirstOrDefaultAsync()))).Where(y => y != null));
            var otherHeadsDBPimerayKey = new HashSet<int>(otherHeadsDB.Select(x => x.PrimaryKey));




            foreach (var element in myHeads.Values.ToArray())
            {
                var data = await servicePortClient.GetTransaction(element.Hash);
                if (data != null)
                    myHeads.Remove(element.Hash); // Entferne alle meine Head, wo der Server bereits weiter ist.                       

            }

            var queue = new Queue<Transaction>(myHeads.Values);
            var toSubmit = new HashSet<Transaction>(myHeads.Values);

            while (queue.Any())
            {
                var element = queue.Dequeue();
                var transfares = await element.Transfares;
                var previousTransfares = (await Task.WhenAll(transfares.Select(x => x.PreviousTransfare))).Where(x => x != null);
                var previosTransaction = await Task.WhenAll(previousTransfares.Select(x => x.Parent));
                foreach (var item in previosTransaction)
                {
                    if (otherHeadsDBPimerayKey.Contains(item.PrimaryKey))
                        continue; // Wenn wir einen Head des Servers haben können wir aufhören.
                    if (toSubmit.Add(item))
                        queue.Enqueue(item); // Wenns noch nicht im Hashset war müssen wir auch dessen Vorgänger Durchscuhen
                }
            }



            await servicePortClient.SubmitTransactions(await Task.WhenAll(toSubmit.Select(x => ServiceMerger.TransactionImplimentation.GetImplementation(x))));

            // Alle Daten Finden welche Ich in meinen Graphen Adden muss.
            var queue2 = new Queue<ServiceMerger.ITransaction>(otherheads.Values);
            var toAdd = new HashSet<ServiceMerger.ITransaction>(otherheads.Values);

            while (queue2.Any())
            {
                var element = queue2.Dequeue();
                foreach (var item in element.Transfers)
                {
                    if (item.CardTransferIndex == 0 || await transactionTable.Where(x => x.Hash == item.PreviousTransactionHash).CountAsync() > 0)
                        continue; // den Vorgänger Kenne Ich brauche ich nix mehr machen.

                    var newTransaction = await servicePortClient.GetTransaction(item.PreviousTransactionHash);

                    if (toAdd.Add(newTransaction))
                        queue2.Enqueue(newTransaction); // Wenns noch nicht im Hashset war müssen wir auch dessen Vorgänger Durchscuhen
                }
            }


            using (await Graph.semaphore.Enter())
            {
                await Graph.db.BeginTransaction();
                try
                {
                    var trans = await Task.WhenAll(toAdd.Select(async x => new Graph.TransactionTransfareData()
                    {
                        Transaction = new Transaction()
                        {
                            AFk = (await PublicKey.GetKey(x.A)).PrimaryKey,
                            BFk = (await PublicKey.GetKey(x.B)).PrimaryKey,
                            SigA = x.SignatureA,
                            SigB = x.SignatureB,
                            Hash = x.Hash
                        },
                        Transfares = await Task.WhenAll(x.Transfers.Select(async t =>
                         new Graph.TransactionTransfareData.TransfareData()
                         {
                             Transfare = new Transfare()
                             {
                                 CardCreatorFK = (await PublicKey.GetKey(t.Creator)).PrimaryKey,
                                 CardID = t.CardId,
                                 CardTransferIndex = t.CardTransferIndex,
                                 ReciverFk = (await PublicKey.GetKey(t.Recipient)).PrimaryKey,
                                 SenderFK = (await PublicKey.GetKey(t.Giver)).PrimaryKey,
                                 Valid = true
                             },
                             PreviousTransactionHash = t.PreviousTransactionHash
                         })),
                    }));

                    await Graph.TransactionTransfareData.InsertTransactionData(trans);
                    await Graph.TransactionTransfareData.CheckTransactionToCreate(trans);

                    await Graph.Commit();

                }
                catch (Exception)
                {
                    await Graph.Rollback();
                    throw;
                }

            }


        }

        public static async Task AddTransactions(IEnumerable<ITransaction> toAdd, Func<byte[], Task<byte[]>> sign)
        {

            // Wir müssen die Primary Keys anlegen bevor wir in die Transaktion gehen. :/
            foreach (var x in toAdd)
            {
                await PublicKey.GetKey(x.A);
                await PublicKey.GetKey(x.B);
            }

            using (await Graph.semaphore.Enter())
            {
                await Graph.db.BeginTransaction();
                try
                {
                    var trans = await Task.WhenAll(toAdd.Select(async x => new Graph.TransactionTransfareData()
                    {
                        Transaction = new Transaction()
                        {
                            AFk = (await PublicKey.GetKey(x.A)).PrimaryKey,
                            BFk = (await PublicKey.GetKey(x.B)).PrimaryKey,
                            SigA = x.SignatureA,
                            SigB = x.SignatureB,
                            Hash = x.Hash
                        },
                        Transfares = await Task.WhenAll(x.Transfers.Select(async t =>
                         new Graph.TransactionTransfareData.TransfareData()
                         {
                             Transfare = new Transfare()
                             {
                                 CardCreatorFK = (await PublicKey.GetKey(t.Creator)).PrimaryKey,
                                 CardID = t.CardId,
                                 CardTransferIndex = t.CardTransferIndex,
                                 ReciverFk = (await PublicKey.GetKey(t.Recipient)).PrimaryKey,
                                 SenderFK = (await PublicKey.GetKey(t.Giver)).PrimaryKey,
                                 Valid = true
                             },
                             PreviousTransactionHash = t.PreviousTransactionHash
                         })),
                    }));

                    await Graph.TransactionTransfareData.InsertTransactionData(trans);

                    foreach (var t in trans)
                    {
                        if (t.Transaction.SigA == null && t.Transaction.SigB != null)
                        {
                            if (!t.Transfares.All(x => x.Transfare.ReciverFk == t.Transaction.AFk))
                                throw new ArgumentException("Ich unterschreibe nur Karten die Ich empfange");
                            t.Transaction.SigA = await sign(await t.Transaction.GenereateBytesToSign());
                        }
                        else if (t.Transaction.SigA != null && t.Transaction.SigB == null)
                        {
                            if (!t.Transfares.All(x => x.Transfare.ReciverFk == t.Transaction.BFk))
                                throw new ArgumentException("Ich unterschreibe nur Karten die Ich empfange");
                            var toSig = await t.Transaction.GenereateBytesToSign();
                            t.Transaction.SigB = await sign(toSig);
                            // Updaten der Signatur


                        }
                        else
                            throw new ArgumentException("Mindestens eine Signatur muss gesetzt sein, und eine Muss fehlen.");
                    }
                    await db.UpdateAllAsync(trans.Select(x => x.Transaction));


                    await Graph.TransactionTransfareData.CheckTransactionToCreate(trans);

                    await Graph.Commit();
                }
                catch (Exception)
                {
                    await Graph.Rollback();
                    throw;
                }

            }
        }

        private static void Cleanup()
        {
            db.DropTableAsync<Transaction>().Wait();
            db.DropTableAsync<Transfare>().Wait();
            db.DropTableAsync<PublicKey>().Wait();
        }

        private static void Init()
        {
            db.CreateTableAsync<Transaction>().Wait();
            db.CreateTableAsync<Transfare>().Wait();
            db.CreateTableAsync<PublicKey>().Wait();
        }

        internal static event Action Commited;
        internal static event Action RoledBack;

        internal static Task Commit()
        {
            return Graph.db.Commit();
        }

        internal static Task Rollback()
        {
            return Graph.db.Rollback();
        }

        internal static readonly System.Threading.DisposingUsingSemaphore semaphore = new System.Threading.DisposingUsingSemaphore();

        public async static Task Merge(AbstractQuestchener q, params CardInstance[] cardsToSync)
        {

            var heads = await GetHeads();

            // Finde alle forherigen transactionhashs
            var prevTrans = heads.Select(x => new { Hash = x.Hash, Transaction = x }).ToArray();
            var transactionsToCreate = new List<Transaction>();
            while (prevTrans.Any())
            {
                var t = await Task.WhenAll(prevTrans.Select(async x => new { OtherHave = await q.HasTransaction(x.Hash), Hash = x.Hash, Transaction = x.Transaction }));
                transactionsToCreate.AddRange(t.Where(x => !x.OtherHave).Select(x => x.Transaction));

                // Finde alle forherigen transactionhashs
                prevTrans = (await Task.WhenAll((await Task.WhenAll(t.Where(x => !x.OtherHave).Select(x => x.Transaction.Transfares))).SelectMany(x => x).Where(x => x.PreviousTransfareFK != -1 /* Previous Transfare null*/).Select(async x => new { Hash = await x.PreviousTransactionHash, Transaction = await (await x.PreviousTransfare).Parent }))).Distinct().ToArray();
            }

            transactionsToCreate.Reverse();
            if (transactionsToCreate.Any())
                await q.SendToCreate(transactionsToCreate);




        }

        public async static Task Trade(Security.IPublicKey a, Security.IPublicKey b, IEnumerable<CardInstance> aToB, IEnumerable<CardInstance> bToA, Func<byte[], Task<byte[]>> singA, Func<byte[], Task<byte[]>> singB)
        {
            using (await semaphore.Enter())
            {


                // vorberechnungen
                var pka = await PublicKey.GetKey(a);
                var pkb = await PublicKey.GetKey(b);

                var pkLookup = new Dictionary<int, PublicKey>();
                pkLookup[pka.PrimaryKey] = pka;
                pkLookup[pkb.PrimaryKey] = pkb;



                // Überprüfe ob A dar eigentümer der Karten ist oder sie erstellt
                foreach (var fromA in aToB)
                {
                    if (!(await CheckOwner(pka, fromA)))
                        throw new ArgumentException(String.Format("Karte gehört nicht A ({0})", fromA));
                }


                // Überprüfe ob B dar eigentümer der Karten ist oder sie ersellt
                foreach (var fromB in bToA)
                {
                    if (!(await CheckOwner(pkb, fromB)))
                        throw new ArgumentException(String.Format("Karte gehört nicht B ({0})", fromB));
                }

                // Erstelle die Transaktion
                var toCreate = await Task.WhenAll(aToB.Select(x => new { Card = x, From = pka, To = pkb }).Concat(bToA.Select(x => new { Card = x, From = pkb, To = pka }))
                    .Select(async x =>
                    {
                        var creator = await PublicKey.GetKey(x.Card.Creator);
                        var lastTransfare = await LastTransfare(x.Card.Id, creator);
                        var previousTransaction = await (lastTransfare?.Parent ?? Task.FromResult<Transaction>(null));


                        Transfare lastInvalidTransfares;
                        if (lastTransfare != null)
                        {
                            var lastPk = lastTransfare.PrimaryKey;
                            var table = db.Table<Transfare>();
                            var where = table.Where(y => y.PreviousTransfareFK == lastPk);
                            var orderBy = where.OrderByDescending(y => y.CardTransferIndex);
                            lastInvalidTransfares = await orderBy.FirstOrDefaultAsync();
                        }
                        else
                            lastInvalidTransfares = null;

                        return new { CardID = x, Creator = creator, From = x.From, To = x.To, PreviousTransfare = lastTransfare, InvalidTranfare = lastInvalidTransfares, PreviousTransaction = previousTransaction };
                    }));

                var bufferWaiter = new TaskCompletionSource<byte[]>();
                var sigAWaiter = new TaskCompletionSource<byte[]>();
                var sigBWaiter = new TaskCompletionSource<byte[]>();
                var transActionWaiter = new TaskCompletionSource<Transaction>();
                var sigCheckWaiter = new TaskCompletionSource<Tuple<bool, bool>>();


                var transactionTask = db.RunInTransactionAsync((SQLiteConnection connction) =>
                {
                    var transaction = new Transaction() { AFk = pka.PrimaryKey, BFk = pkb.PrimaryKey };
                    transActionWaiter.SetResult(transaction);
                    connction.Insert(transaction);

                    var buffer = new List<byte>();

                    buffer.AddRange(pka.Modulus);
                    buffer.AddRange(pka.Exponent);

                    buffer.AddRange(pkb.Modulus);
                    buffer.AddRange(pkb.Exponent);
                    var bytearraycomparer = new Misc.Portable.ByteArrayComparer();

                    foreach (var t in toCreate.OrderBy(x => x.CardID.Card.Id.ToBigEndianBytes(), bytearraycomparer).ThenBy(x => x.Creator.Modulus, bytearraycomparer).ThenBy(x => x.Creator.Exponent, bytearraycomparer))
                    {

                        var transfareIndex = t.PreviousTransfare?.CardTransferIndex + 1 ?? 0;
                        // Setze Transfare index auf 1 höher des höchsten Invaliden Indexes.
                        if (t.InvalidTranfare != null)
                            transfareIndex = t.InvalidTranfare.CardTransferIndex + 1;
                        // TODO FIX

                        var transfare = new Transfare()
                        {
                            CardCreatorFK = t.Creator.PrimaryKey,
                            CardID = t.CardID.Card.Id,
                            ParentFK = transaction.PrimaryKey,
                            ReciverFk = t.To.PrimaryKey,
                            SenderFK = t.From.PrimaryKey,
                            PreviousTransfareFK = t.PreviousTransfare?.PrimaryKey ?? -1,
                            CardTransferIndex = transfareIndex,
                            Valid = true,
                        };


                        // Generate Hash



                        buffer.AddRange(transfare.CardID.ToBigEndianBytes());

                        buffer.AddRange(t.Creator.Modulus);
                        buffer.AddRange(t.Creator.Exponent);

                        buffer.AddRange(Misc.BitConverter.GetBytes(transfare.CardTransferIndex));

                        buffer.AddRange(pkLookup[transfare.SenderFK].Modulus);
                        buffer.AddRange(pkLookup[transfare.SenderFK].Exponent);

                        buffer.AddRange(pkLookup[transfare.ReciverFk].Modulus);
                        buffer.AddRange(pkLookup[transfare.ReciverFk].Exponent);

                        buffer.AddRange(t.PreviousTransaction?.Hash ?? new byte[0]);





                        connction.Insert(transfare);
                    }
                    transaction.Hash = Security.SecurityFactory.HashSha256(buffer.ToArray());
                    bufferWaiter.SetResult(buffer.ToArray());


                    Task.WaitAll(sigAWaiter.Task, sigBWaiter.Task);

                    transaction.SigA = sigAWaiter.Task.Result;
                    transaction.SigB = sigBWaiter.Task.Result;


                    connction.Update(transaction);

                    Task.WaitAll(sigCheckWaiter.Task);
                    if (!(sigCheckWaiter.Task.Result.Item1 && sigCheckWaiter.Task.Result.Item1))
                        throw new ArgumentException($"Fehler bei überprüfung der Signatur. A ist {(sigCheckWaiter.Task.Result.Item1 ? "gültig" : "ungültig")}, B ist {(sigCheckWaiter.Task.Result.Item2 ? "gültig" : "ungültig")} ");
                });


                // Unterschreibe die Transaktionen
                sigAWaiter.SetResult(await singA(await bufferWaiter.Task));
                sigBWaiter.SetResult(await singB(await bufferWaiter.Task));

                var aVerify = a.Veryfiy(await bufferWaiter.Task, await sigAWaiter.Task);
                var bVerify = b.Veryfiy(await bufferWaiter.Task, await sigBWaiter.Task);
                // überprüfe die transaktionen
                sigCheckWaiter.SetResult(Tuple.Create(aVerify, bVerify));




                await transactionTask;

                if (!(await transActionWaiter.Task.Result.CheckSig()))
                    throw new ArgumentException("Fehler bei überprüfung der Signatur Obwohl erste überprüfung klappt.");

            }
        }


        public static async Task<bool> CheckOwner(Security.IPublicKey pk, CardInstance c)
        {
            var p = await PublicKey.GetKey(pk);
            return await CheckOwner(p, c);
        }

        public static async Task<bool> CheckOwner(Client.Game.Data.PublicKey pk, CardInstance c)
        {
            var p = await PublicKey.GetKey(pk);
            return await CheckOwner(p, c);
        }

        public static async Task<IEnumerable<UuidServer>> GetCardsOf(Security.IPublicKey owner)
        {
            var p = await PublicKey.GetKey(owner);

            var erg = await db.QueryAsync<Transfare>(@"
SELECT * 
FROM 'Transfare'
WHERE ReciverFk = " + p.PrimaryKey + @"
AND PrimaryKey     NOT IN  (SELECT PreviousTransfareFK
                            FROM 'Transfare'                                                        
                            )
");
            return (await Task.WhenAll(erg.Select(async x => new { ID = x.CardID, Creator = await x.CardCreator }))).Select(x => new UuidServer() { Uuid = x.ID, Server = x.Creator.ToGameData() });


        }

        public static async Task<Client.Game.Data.PublicKey> GetOwner(CardInstance c)
        {
            var lastTransfare = await LastTransfare(c);
            return (await lastTransfare.Reciver).ToGameData();
        }

        private static async Task<bool> CheckOwner(PublicKey pk, CardInstance c)
        {
            Transfare lastTransfare = await LastTransfare(c);
            if (lastTransfare != null)
            {
                if (lastTransfare.ReciverFk != pk.PrimaryKey)
                    return false;
            }
            else
            {
                if (!(c.Creator.Exponent.SequenceEqual(pk.Exponent) && c.Creator.Modulus.SequenceEqual(pk.Modulus)))
                    return false;
            }
            return true;
        }

        private static async Task<IEnumerable<Transaction>> GetHeads()
        {

            var erg = await db.QueryAsync<Transaction>(@"
SELECT * 
FROM 'Transaction'
WHERE PrimaryKey     IN (SELECT ParentFK
                            FROM 'Transfare'
                            WHERE PrimaryKey NOT IN  (SELECT PreviousTransfareFK
                                                        FROM 'Transfare'                                                        
                                                        )
)
");

            return erg;
        }

        private static async Task<Transfare> LastTransfare(CardInstance c)
        {
            var creatorpk = await PublicKey.GetKey(c.Creator);
            Transfare lastTransfare = await LastTransfare(c.Id, creatorpk);
            return lastTransfare;
        }

        private static async Task<Transfare> LastTransfare(Guid c, PublicKey creatorpk)
        {
            return await db.Table<Transfare>().Where(x => x.CardCreatorFK == creatorpk.PrimaryKey && x.CardID == c && x.Valid).OrderByDescending(x => x.CardTransferIndex).FirstOrDefaultAsync();
        }

        private class ByteEnumComparer : IEqualityComparer<IEnumerable<byte>>
        {
            public bool Equals(IEnumerable<byte> x, IEnumerable<byte> y)
            {
                return x.SequenceEqual(y);
            }

            public int GetHashCode(IEnumerable<byte> obj)
            {
                const int prime = 31;
                int result = 1;
                foreach (var b in obj)
                    result = result * prime + b;
                return result;
            }
        }



        internal class TransactionTransfareData
        {
            public Transaction Transaction { get; set; }
            public TransfareData[] Transfares { get; set; }

            internal class TransfareData
            {
                public byte[] PreviousTransactionHash { get; set; }
                public Transfare Transfare { get; set; }
            }

            public static async Task InsertTransactionData(Graph.TransactionTransfareData[] trans)
            {
                await Graph.db.InsertAllAsync(trans.Select(x => x.Transaction));
                foreach (var t in trans)
                    foreach (var transfare in t.Transfares)
                        transfare.Transfare.ParentFK = t.Transaction.PrimaryKey;

                await Graph.db.InsertAllAsync(trans.SelectMany(x => x.Transfares).Select(x => x.Transfare));

                foreach (var t in trans)
                    foreach (var transfare in t.Transfares)
                    {
                        var prevTransaction = await Graph.db.Table<Transaction>().Where(x => x.Hash == transfare.PreviousTransactionHash).FirstOrDefaultAsync();
                        if (prevTransaction != null)
                        {

                            var prevTransfrae = (await prevTransaction.Transfares).Where(x => x.CardID == transfare.Transfare.CardID && x.CardCreatorFK == transfare.Transfare.CardCreatorFK).Single();
                            transfare.Transfare.PreviousTransfareFK = prevTransfrae.PrimaryKey;
                        }
                        else
                            transfare.Transfare.PreviousTransfareFK = -1;

                        await Graph.db.UpdateAsync(transfare.Transfare);
                    }
            }

            public static async Task CheckTransactionToCreate(Graph.TransactionTransfareData[] trans)
            {
                // Überprüfe die Hashs 
                if (!(await Task.WhenAll(trans.Select(x => x.Transaction).Select(x => x.CheckHash()))).All(x => x))
                    throw new InvalidOperationException("Hashs Stimmen nicht");

                //und Signaturen
                if (!(await Task.WhenAll(trans.Select(x => x.Transaction).Select(x => x.CheckSig()))).All(x => x))
                    throw new InvalidOperationException("Signaturen Stimmen nicht");

                //und ob alle vorherigen Transaktionen da sind
                if (!(await Task.WhenAll(trans.Select(x => x.Transaction).Select(x => x.CheckPreTransactions()))).All(x => x))
                    throw new InvalidOperationException("Es fehlen transacktionen");




                // Finde die neuen Fehler 

                var transfareTable = Graph.db.Table<Transfare>();

                foreach (var t in trans)
                    foreach (var transfare in t.Transfares)
                    {
                        var asyncTableQuery = transfareTable.Where(x =>
                        x.CardTransferIndex == transfare.Transfare.CardTransferIndex &&
                        x.PreviousTransfareFK == transfare.Transfare.PreviousTransfareFK
                        && x.CardCreatorFK == transfare.Transfare.CardCreatorFK
                        && x.CardID == transfare.Transfare.CardID
                        );
                        var count = await asyncTableQuery.CountAsync();
                        if (count > 1)
                        {
                            var queue = new Queue<Transfare>(await asyncTableQuery.ToListAsync());

                            while (queue.Count != 0)
                            {
                                var element = queue.Dequeue();
                                if (!element.Valid)
                                    continue;
                                element.Valid = false;
                                await Graph.db.UpdateAsync(element);
                                // Alle transfares der Transaction als ungültig erklären
                                foreach (var item in await (await element.Parent).Transfares)
                                    if (item.Valid)
                                        queue.Enqueue(item);


                                // Alle nachfolgetransfares als ungültig erklären
                                foreach (var item in await transfareTable.Where(x => x.PreviousTransfareFK == element.PrimaryKey).ToListAsync())
                                    if (item.Valid)
                                        queue.Enqueue(item);


                            }

                        }
                        else if (count == 1)
                        {
                            var element = await asyncTableQuery.FirstAsync();
                            var pre = await element.PreviousTransfare;
                            if (pre != null && pre.Valid != element.Valid)
                            {
                                element.Valid = pre.Valid;
                                await Graph.db.UpdateAsync(element);
                            }
                        }
                        else
                            throw new Exception("Es muss genau einen Geben!");
                    }
            }

        }
    }

}

