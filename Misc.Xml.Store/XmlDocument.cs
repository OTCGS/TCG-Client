using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using orig = Windows.Data.Xml.Dom;

namespace Misc.Xml.Store
{
    internal class XmlDocument : IXmlDocument
    {
        private readonly orig.XmlDocument doc;

        public XmlDocument(string xml)
        {
            doc = new orig.XmlDocument();
            doc.LoadXml(xml);
        }

        public IXmlNode SelectSingleNode(string path, string ns = null)
        {
            if (ns == null)
                return new XmlNode(doc.SelectSingleNode(path));
            else
                return new XmlNode(doc.SelectSingleNodeNS(path, ns));
        }

        public string GetXml()
        {
            return doc.GetXml();
        }
    }
}