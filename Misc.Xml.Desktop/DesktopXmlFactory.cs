using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Misc.Xml.Desktop
{
    public class DesktopXmlFactory : XmlFactory
    {
        private DesktopXmlFactory()
        {
        }

        public static void Initialize()
        {
            var x = new DesktopXmlFactory();
            x.SetFactory(x);
        }

        protected override async Task<IXmlDocument> PrivatLoadDocument(string xml)
        {
            return new XmlDocument(xml);
        }
    }
}