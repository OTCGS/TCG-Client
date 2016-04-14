using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using orig = System.Xml;

namespace Misc.Xml.Desktop
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
            var nsmng = new orig.XmlNamespaceManager(doc.NameTable);

            if (ns != null)
            {
                var reg = new Regex("^xmlns:(?<präfix>.*?)=\"(?<ns>.*)\"$");
                var match = reg.Match(ns);
                if (match.Success)
                    nsmng.AddNamespace(match.Groups["präfix"].Value, match.Groups["ns"].Value);
            }

            return new XmlNode(doc.SelectSingleNode(path, nsmng));
        }

        public string GetXml()
        {
            return doc.OuterXml;
        }
    }
}