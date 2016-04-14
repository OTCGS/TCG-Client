using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Misc.Xml.Store
{
    public class StoreXmlFactory : XmlFactory
    {
        private StoreXmlFactory()
        {
        }

        public static void Initialize()
        {
            var x = new StoreXmlFactory();
            x.SetFactory(x);
        }

        protected override Task<IXmlDocument> PrivatLoadDocument(string xml)
        {
            return Task.FromResult<IXmlDocument>(new XmlDocument(xml));
        }
    }
}