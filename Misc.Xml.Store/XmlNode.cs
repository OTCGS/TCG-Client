using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using orig = Windows.Data.Xml.Dom;

namespace Misc.Xml.Store
{
    internal class XmlNode : IXmlNode
    {
        private readonly orig.IXmlNode xmlNode;

        public XmlNode(orig.IXmlNode xmlNode)
        {
            this.xmlNode = xmlNode;
        }

        public string Value
        {
            get { return xmlNode.InnerText; }
        }

        public string GetXml()
        {
            return xmlNode.GetXml();
        }
    }
}