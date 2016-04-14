using Misc.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Security
{
    public class Signature
    {
        [XmlElement("Signee")]
        public Guid Signee { get; set; }

        [XmlElement("CertificateId")]
        public Guid CertificateId { get; set; }

        [XmlElement("Signature")]
        public byte[] Value { get; set; }

        public static Signature FromXml(string xml)
        {
            var o = new Misc.Serialization.XmlSerilizer<Signature>();
            return o.Deserilize(xml);
        }

        public string ToXml()
        {
            var o = new Misc.Serialization.XmlSerilizer<Signature>();
            return o.Serialize(this);
        }
    }
}