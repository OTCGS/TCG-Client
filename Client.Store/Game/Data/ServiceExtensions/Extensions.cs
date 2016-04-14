using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Client.Store.CardServerService

{
    public partial class cardDataId
    {
        public static implicit operator Game.Data.CardDataId(cardDataId data)
        {
            return new Game.Data.CardDataId() { Edition = data.editionField, Number = data.numberField, Revision = data.revisionField };
        }
        public static implicit operator cardDataId(Game.Data.CardDataId data)
        {
            return new cardDataId() { editionField = data.Edition, numberField = data.Number, revisionField = data.Revision };
        }
    }
}
