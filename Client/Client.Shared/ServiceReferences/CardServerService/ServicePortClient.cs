using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Security;
using System.Threading.Tasks;
using Game.TransactionMap.ServiceMerger;
using Client.Game.Data;

namespace Client.CardServerService
{
    partial class ServicePortClient : global::Game.TransactionMap.ServiceMerger.IServiceMerger
    {
        /// <summary>
        /// Mergres die Transactionmaps
        /// </summary>
        /// <returns></returns>
        public async Task MergeTransacrionMapAsync()
        {
            var heads = await this.getHeadsAsync();

            var otherTransfares = heads.SelectMany(x => x.transfers);
            var myTransfares = global::Game.TransactionMap.Graph.Merge(this);


        }

        public async System.Threading.Tasks.Task<IEnumerable<transaction>> getHeadsAsync()
        {
            return (await this.getHeadsAsync(new getHeadsRequest())).getHeadsResponse1;
        }

        async Task<IEnumerable<ITransaction>> IServiceMerger.GetHeads()
        {
            return await this.getHeadsAsync();
        }

        async Task IServiceMerger.SubmitTransactions(IEnumerable<ITransaction> transactions)
        {
            var t = transactions.Select(x => new transaction()
            {
                a = x.A,
                b = x.B,
                signatureA = x.SignatureA,
                signatureB = x.SignatureB,
                transfers = x.Transfers.Select(y => new transfer()
                {
                    cardId = y.CardId.ToString(),
                    cardTransferIndex = y.CardTransferIndex,
                    creator = y.Creator,
                    giver = y.Giver,
                    previousTransactionHash = y.PreviousTransactionHash,
                    recipient = y.Recipient
                }).ToArray()

            }).ToArray();

            foreach (var item in t)
            {
                var p = item.a.ToSecurety();
                var erg = p.Veryfiy(item.GetBytes(), item.signatureA);
                Logger.Assert(erg, $"sig A nicht gültig {item.transfers.Select(x => x.cardId).Aggregate((s1, s2) => $"{s1} {s2}")}");


                p = item.b.ToSecurety();
                erg = p.Veryfiy(item.GetBytes(), item.signatureB);
                Logger.Assert(erg, $"sig B nicht gültig {item.transfers.Select(x => x.cardId).Aggregate((s1, s2) => $"{s1} {s2}")}");
            }
            var response = await this.submitTransactionsAsync(t);

            if (!response.submitTransactionsResponse.success)
                throw new System.Net.WebException("Fehler beim Submit: " + response.submitTransactionsResponse.errorMessage);
        }
        async Task<ITransaction> IServiceMerger.GetTransaction(byte[] hash)
        {
            var t = await this.getTransactionAsync(new getTransactionRequest() { hash = hash });
            //TODO: Fehelerbehandlung
            return t.getTransactionResponse.transaction;
        }


    }



    partial class transaction : global::Game.TransactionMap.ServiceMerger.ITransaction
    {
        PublicKey ITransaction.A
        {
            get
            {
                return a;
            }
        }

        PublicKey ITransaction.B
        {
            get
            {
                return b;
            }
        }

        byte[] ITransaction.Hash
        {
            get
            {

                {
                    var buffer = GetBytes();

                    return Security.SecurityFactory.HashSha256(buffer);
                }

            }
        }

        public byte[] GetBytes()
        {
            var pka = (this as ITransaction).A;
            var pkb = (this as ITransaction).B;



            var buffer = new List<byte>();

            buffer.AddRange(pka.Modulus);
            buffer.AddRange(pka.Exponent);

            buffer.AddRange(pkb.Modulus);
            buffer.AddRange(pkb.Exponent);

            var bytearraycomparer = new Misc.Portable.ByteArrayComparer();

            var t = (this as ITransaction).Transfers.OrderBy(x => x.CardId.ToBigEndianBytes(), bytearraycomparer).ThenBy(x => x.Creator.Modulus, bytearraycomparer).ThenBy(x => x.Creator.Exponent, bytearraycomparer);
            foreach (var transfare in t)
            {
                // Generate Hash
                buffer.AddRange(transfare.CardId.ToBigEndianBytes());

                var cardCreator = (transfare.Creator);
                buffer.AddRange(cardCreator.Modulus);
                buffer.AddRange(cardCreator.Exponent);


                buffer.AddRange(Misc.BitConverter.GetBytes(transfare.CardTransferIndex));

                buffer.AddRange(transfare.Giver.Modulus);
                buffer.AddRange(transfare.Giver.Exponent);

                buffer.AddRange(transfare.Recipient.Modulus);
                buffer.AddRange(transfare.Recipient.Exponent);

                buffer.AddRange((transfare.PreviousTransactionHash) ?? new byte[0]);

            }

            return buffer.ToArray();
        }

        byte[] ITransaction.SignatureA
        {
            get
            {
                return this.signatureA;
            }
        }

        byte[] ITransaction.SignatureB
        {
            get
            {
                return this.signatureB;
            }
        }

        IEnumerable<ITransfer> ITransaction.Transfers
        {
            get
            {
                return transfers;
            }
        }
    }

    public partial class transfer : global::Game.TransactionMap.ServiceMerger.ITransfer
    {
        Guid ITransfer.CardId
        {
            get
            {
                return Guid.Parse(this.cardId);
            }
        }

        int ITransfer.CardTransferIndex
        {
            get
            {
                return cardTransferIndex;
            }
        }

        PublicKey ITransfer.Creator
        {
            get
            {
                return creator;
            }
        }

        PublicKey ITransfer.Giver
        {
            get
            {
                return giver;
            }
        }

        byte[] ITransfer.PreviousTransactionHash
        {
            get
            {
                return previousTransactionHash;
            }
        }

        PublicKey ITransfer.Recipient
        {
            get
            {
                return recipient;
            }
        }
    }

    public partial class key : Security.Interfaces.IPublicKeyData
    {
        public byte[] Exponent
        {
            get
            {
                return exponent;
            }
        }

        public byte[] Modulus
        {
            get
            {
                return modulus;
            }
        }

        public void SetKey(byte[] modulus, byte[] exponent)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PublicKey(key k)
        {
            return new PublicKey() { Exponent = k.exponent, Modulus = k.modulus };
        }
        public static implicit operator key(PublicKey k)
        {
            return new key() { exponent = k.Exponent, modulus = k.Modulus };
        }



    }

    public partial class cardData
    {
        public CardData ToGameData()
        {
            var erg = new CardData()
            {
                CardRevision = this.cardRevision,
                Creator = this.creator,
                Edition = this.edition,
                Id = Guid.Parse(this.id),
                ImageId = Guid.Parse(this.imageId),
                Name = this.name,
                Signature = this.signature,
            };
            if (this.values != null)
                erg.Values.AddRange(this.values.Select(x => x.ToGameData()));

            Logger.Assert(erg.Creator != null, $"{nameof(cardData)}.{nameof(cardData.creator)} ist Null");
            Logger.Assert(erg.Edition != null, $"{nameof(cardData)}.{nameof(cardData.edition)} ist Null");
            Logger.Assert(erg.Id != Guid.Empty, $"{nameof(cardData)}.{nameof(cardData.id)} ist default");
            Logger.Assert(erg.ImageId != Guid.Empty, $"{nameof(cardData)}.{nameof(cardData.imageId)} ist default");
            Logger.Assert(erg.Name != null, $"{nameof(cardData)}.{nameof(cardData.name)} ist null");


            return erg;
        }

    }

    public partial class ruleSetKey
    {
        public Keys ToGameData()
        {
            Logger.Assert(name != null, $"{nameof(ruleSetKey)}.{nameof(name)} ist Null");
            Logger.Assert(valueType != null, $"{nameof(ruleSetKey)}.{nameof(valueType)} ist Null");

            return new Keys()
            {
                Name = this.name,
                Type = this.valueType
            };
        }
    }
    public partial class ruleSet
    {
        public Ruleset ToGameData()
        {
            var erg = new Ruleset()
            {
                Creator = this.creator,
                Id = Guid.Parse(this.id),
                Name = this.name,
                Script = this.script,
                Revision = this.revision,
                Signature = this.signature,
            };
            if (this.mandatoryKeys != null)
                erg.MandatoryKeys.AddRange(this.mandatoryKeys.Select(x => x.ToGameData()));

            Logger.Assert(creator != null, $"{nameof(ruleSet)}.{nameof(creator)} ist Null");
            Logger.Assert(erg.Id != Guid.Empty, $"{nameof(ruleSet)}.{nameof(id)} ist default");
            Logger.Assert(name != null, $"{nameof(ruleSet)}.{nameof(name)} ist Null");
            Logger.Assert(script != null, $"{nameof(ruleSet)}.{nameof(script)} ist Null");
            Logger.Assert(signature != null, $"{nameof(ruleSet)}.{nameof(signature)} ist Null");


            return erg;
        }
    }

    public partial class imageData
    {
        public ImageData ToGameData()
        {
            var erg = new ImageData()
            {
                Image = this.image,
                Creator = this.creator,
                Id = Guid.Parse(this.id),
                Signatur = this.signature,
            };
            Logger.Assert(image != null, $"{nameof(imageData)}.{nameof(image)} ist Null");
            Logger.Assert(creator != null, $"{nameof(imageData)}.{nameof(creator)} ist Null");
            Logger.Assert(signature != null, $"{nameof(imageData)}.{nameof(signature)} ist Null");
            Logger.Assert(erg.Id != Guid.Empty, $"{nameof(imageData)}.{nameof(id)} ist defalt");


            return erg;
        }
    }

    public partial class cardInstance
    {
        public static implicit operator CardInstance(cardInstance i)
        {
            if (i == null)
                return null;
            return new CardInstance() { CardDataId = Guid.Parse(i.cardDataId), Creator = i.creator, Id = Guid.Parse(i.id), Signature = i.signature };
        }

        public static implicit operator cardInstance(CardInstance i)
        {
            if (i == null)
                return null;
            return new cardInstance() { cardDataId = i.CardDataId.ToString(), creator = i.Creator, id = i.Id.ToString(), signature = i.Signature };
        }
    }

    public partial class serverIdentity
    {
        public static implicit operator serverIdentity(ServerId i)
        {
            return new serverIdentity() { icon = i.Icon.ToString(), key = i.Key, name = i.Name, revision = i.Revision, signature = i.Signiture, uri = i.Uri };
        }
        public static explicit operator ServerId(serverIdentity i)
        {
            return new ServerId() { Icon = String.IsNullOrWhiteSpace(i.icon) ? Guid.Empty : Guid.Parse(i.icon), Key = i.key, Name = i.name, Revision = i.revision, Signiture = i.signature, Uri = i.uri };
        }
    }

    public partial class cardDataKeyValue
    {
        public KeyValue ToGameData()
        {
            return new KeyValue()
            {
                Key = this.key,
                Value = this.value
            };
        }
    }
}
