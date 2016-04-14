using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace Game.TransactionMap
{
    public abstract class AbstractQuestchener
    {
        internal async Task<bool> HasTransaction(byte[] hash)
        {

            var buffer = new List<byte>();
            buffer.Add((byte)Msg.HasTransaction);
            buffer.AddRange(hash);
            var erg = await SendBytes(buffer.ToArray());
            return erg[0] != 0;

        }

        internal async Task SendToCreate(IEnumerable<Transaction> transactionsToCreate)
        {
            Logger.Assert((await Task.WhenAll(transactionsToCreate.Select(x => x.CheckHash()))).All(x => x), "Fehler bei den Zu sendenden Daten AbstractQuestioner.SendToCreate(...)");
            var transactions = await Task.WhenAll(transactionsToCreate.Select(async x => new
            {
                Hash = x.Hash,
                A = await x.A,
                B = await x.B,
                Transfares = await Task.WhenAll((await x.Transfares).Select(async t => new
                {
                    CardCreator = await t.CardCreator,
                    CardID = t.CardID,
                    CardTransferIndex = t.CardTransferIndex,
                    PreviousTransactionHash = await t.PreviousTransactionHash,
                    Reciver = await t.Reciver,
                    Sender = await t.Sender,
                })),
                SigA = x.SigA,
                SigB = x.SigB
            }));

            var xml = new XElement("Transactions",
                transactions.Select(x => new XElement("Transaction",
                    new XElement("A",
                        new XElement("Modulus", Convert.ToBase64String(x.A.Modulus)),
                        new XElement("Exponent", Convert.ToBase64String(x.A.Exponent))),
                    new XElement("B",
                        new XElement("Modulus", Convert.ToBase64String(x.B.Modulus)),
                        new XElement("Exponent", Convert.ToBase64String(x.B.Exponent))),
                    new XElement("Transfares",
                        x.Transfares.Select(t => new XElement("Transfare",
                            new XElement("CardID", t.CardID),
                            new XElement("CardCreator",
                                new XElement("Modulus", Convert.ToBase64String(t.CardCreator.Modulus)),
                                new XElement("Exponent", Convert.ToBase64String(t.CardCreator.Exponent))),
                            new XElement("CardTransferIndex", t.CardTransferIndex),
                            new XElement("PreviousTransactionHash", t.PreviousTransactionHash != null ? Convert.ToBase64String(t.PreviousTransactionHash) : ""),
                            new XElement("Sender",
                                new XElement("Modulus", Convert.ToBase64String(t.Sender.Modulus)),
                                new XElement("Exponent", Convert.ToBase64String(t.Sender.Exponent))),
                            new XElement("Reciver",
                                new XElement("Modulus", Convert.ToBase64String(t.Reciver.Modulus)),
                                new XElement("Exponent", Convert.ToBase64String(t.Reciver.Exponent)))
                            ))),
                    new XElement("Hash", Convert.ToBase64String(x.Hash)),
                    new XElement("SigA", Convert.ToBase64String(x.SigA)),
                    new XElement("SigB", Convert.ToBase64String(x.SigB))
            )));

            var type = (byte)Msg.CreateTransaction;
            ;
            var str = xml.ToString();
            var dataCount = UTF8Encoding.UTF8.GetByteCount(str);
            var data = new byte[dataCount + 1];
            data[0] = type;
            UTF8Encoding.UTF8.GetBytes(str, 0, str.Length, data, 1);
            await SendBytes(data);

        }


        protected abstract Task<byte[]> SendBytes(byte[] bytes);

        protected async Task<byte[]> ReciveBytes(byte[] bytes)
        {
            var data = bytes.Skip(1).ToArray();
            var type = bytes.First();
            switch ((Msg)type)
            {
                case Msg.HasTransaction:
                    {
                        var count = await Graph.db.Table<Transaction>().Where(x => x.Hash == data).CountAsync();
                        return new byte[] { (byte)(count > 0 ? 1 : 0) };
                    }
                case Msg.CreateTransaction:

                    var xml = XElement.Parse(UTF8Encoding.UTF8.GetString(data, 0, data.Length));
                    var transactions = xml.Elements("Transaction").Select(transaction =>

                    {
                        var modulusA = Convert.FromBase64String(transaction.Element("A").Element("Modulus").Value);
                        var exponentA = Convert.FromBase64String(transaction.Element("A").Element("Exponent").Value);

                        var modulusB = Convert.FromBase64String(transaction.Element("B").Element("Modulus").Value);
                        var exponentB = Convert.FromBase64String(transaction.Element("B").Element("Exponent").Value);

                        var transfares = transaction.Element("Transfares").Elements("Transfare").Select(transfare =>
                        {
                            var cardId = Guid.Parse(transfare.Element("CardID").Value);
                            var cardCreatorModulus = Convert.FromBase64String(transfare.Element("CardCreator").Element("Modulus").Value);
                            var cardCreatorExponent = Convert.FromBase64String(transfare.Element("CardCreator").Element("Exponent").Value);
                            var cardTransfareIndex = int.Parse(transfare.Element("CardTransferIndex").Value);
                            var previousTransactionHash = transfare.Element("PreviousTransactionHash").Value == "" ? null : Convert.FromBase64String(transfare.Element("PreviousTransactionHash").Value);

                            var senderModulus = Convert.FromBase64String(transfare.Element("Sender").Element("Modulus").Value);
                            var senderExponent = Convert.FromBase64String(transfare.Element("Sender").Element("Exponent").Value);
                            var reciverModulus = Convert.FromBase64String(transfare.Element("Reciver").Element("Modulus").Value);
                            var reciverExponent = Convert.FromBase64String(transfare.Element("Reciver").Element("Exponent").Value);

                            return new
                            {
                                CardID = cardId,
                                CardCreator = new Client.Game.Data.PublicKey()
                                {
                                    Exponent = cardCreatorExponent,
                                    Modulus = cardCreatorModulus
                                },
                                CardTransfareIndex = cardTransfareIndex,
                                PreviousTransactionHash = previousTransactionHash,
                                Sender = new Client.Game.Data.PublicKey()
                                {
                                    Exponent = senderExponent,
                                    Modulus = senderModulus
                                },
                                Reciver = new Client.Game.Data.PublicKey()
                                {
                                    Exponent = reciverExponent,
                                    Modulus = reciverModulus
                                },
                            };
                        });

                        var hash = Convert.FromBase64String(transaction.Element("Hash").Value);
                        var sigA = Convert.FromBase64String(transaction.Element("SigA").Value);
                        var sigB = Convert.FromBase64String(transaction.Element("SigB").Value);


                        return new
                        {
                            A = new Client.Game.Data.PublicKey()
                            {
                                Exponent = exponentA,
                                Modulus = modulusA
                            },
                            B = new Client.Game.Data.PublicKey()
                            {
                                Exponent = exponentB,
                                Modulus = modulusB
                            },
                            Transfares = transfares,
                            Hash = hash,
                            SigA = sigA,
                            SigB = sigB
                        };
                    });

                    // Versuche die Transactions Einzufügen

                    using (await Graph.semaphore.Enter())
                    {
                        await Graph.db.BeginTransaction();
                        try
                        {
                            var trans = await Task.WhenAll(transactions.Select(async x => new Graph.TransactionTransfareData()
                            {
                                Transaction = new Transaction()
                                {
                                    AFk = (await PublicKey.GetKey(x.A)).PrimaryKey,
                                    BFk = (await PublicKey.GetKey(x.B)).PrimaryKey,
                                    SigA = x.SigA,
                                    SigB = x.SigB,
                                    Hash = x.Hash
                                },
                                Transfares = await Task.WhenAll(x.Transfares.Select(async t =>
                                 new Graph.TransactionTransfareData.TransfareData()
                                 {
                                     Transfare = new Transfare()
                                     {
                                         CardCreatorFK = (await PublicKey.GetKey(t.CardCreator)).PrimaryKey,
                                         CardID = t.CardID,
                                         CardTransferIndex = t.CardTransfareIndex,
                                         ReciverFk = (await PublicKey.GetKey(t.Reciver)).PrimaryKey,
                                         SenderFK = (await PublicKey.GetKey(t.Sender)).PrimaryKey,
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


                    break;
                default:
                    throw new NotImplementedException("Code: " + bytes[0]);

            }

            return new byte[] { 0 };

        }


        enum Msg : byte
        {
            HasTransaction = 1,
            CreateTransaction = 2,
        }


    }
}
