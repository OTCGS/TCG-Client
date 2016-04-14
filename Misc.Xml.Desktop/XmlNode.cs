using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using orig = System.Xml;

namespace Misc.Xml.Desktop
{
    internal class XmlNode : IXmlNode
    {
        private readonly orig.XmlNode xmlNode;

        public XmlNode(orig.XmlNode xmlNode)
        {
            this.xmlNode = xmlNode;
        }

        public string Value
        {
            get { return xmlNode.InnerText; }
        }

        public string GetXml()
        {
            return xmlNode.OuterXml;
        }
    }
}