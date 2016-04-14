using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Store.Game.Data
{
    public class CardLookup
    {
        private readonly CardData[] list;
        private readonly Dictionary<CardDataId, int> lookup;

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

        public int this[CardDataId index]
        {
            get { return lookup[index]; }
        }

        public int this[CardData index]
        {
            get { return this[index.Id]; }
        }


        public int Count { get { return list.Length; } }

        public CardLookup(IEnumerable<CardData> cards)
        {

            list = cards.ToArray();
            lookup = cards.Select((x, i) => new { Index = i, Value = x }).ToDictionary(x => x.Value.Id, x => x.Index);

        }
    }
}