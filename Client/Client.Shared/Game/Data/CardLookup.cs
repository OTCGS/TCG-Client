using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Game.Data
{
    public class CardLookup
    {
        private readonly CardData[] list;
        private readonly Dictionary<UuidServer, int> lookup;

        public CardData this[int index]
        {
            get { return list[index]; }
        }

        public CardData this[MentalCardGame.Card index]
        {
            get
            {
                if (index.Type.HasValue)
                    return list[index.Type.Value];
                return null;
            }
        }

        public int this[UuidServer index]
        {
            get { return lookup[index]; }
        }
        public int this[CardData index]
        {
            get { return lookup[new UuidServer() { Uuid = index.Id, Server = index.Creator }]; }
        }
        public int this[Guid id, PublicKey key]
        {
            get { return lookup[new UuidServer() { Uuid = id, Server = key }]; }
        }


        public int Count { get { return list.Length; } }

        public CardLookup(IEnumerable<CardData> cards)
        {

            list = cards.ToArray();
            lookup = cards.Select((x, i) => new { Index = i, Value = new UuidServer() { Uuid = x.Id, Server = x.Creator } }).ToDictionary(x => x.Value, x => x.Index);

        }
    }
}