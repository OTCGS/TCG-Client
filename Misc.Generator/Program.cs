using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Misc.Generator
{
    /// <summary>
    /// Dieser Generator erzeugt angegeben nach der Spezifikation eine Codedatei und eine XSD.
    /// Die Implementierung des Codes hat follgende Einschränkungen:
    /// <list type="bullet">
    ///    <item>
    ///        <term>Mehrdeutiger String</term>
    ///        <description>Es ist nicht möglich bei einem String zwichen <code>null</code> und dem Leeren String zu unterscheiden.</description>
    ///    </item>
    ///    <item>
    ///        <term>Mehrdeutige bytes</term>
    ///        <description>Es ist nicht möglich bei einem bytearray zwichen <code>null</code> und dem Array der größe 0 zu unterscheiden.</description>
    ///    </item>
    ///    <item>
    ///        <term>Listen mit <code>null</code></term>
    ///        <description>Listen können keine <code>null</code>-Werte enthalten</description>
    ///    </item>
    ///</list>
    /// </summary>
    internal class Program
    {
        private static System.Xml.XmlNamespaceManager nsmgr;

        private static Tuple<string, Type>[] TypeConverter = new Tuple<string, Type>[]{
                                                                        Tuple.Create(INT32, typeof(Int32)),
                                                                        Tuple.Create(INT64, typeof(Int64)),
                                                                        Tuple.Create(UINT64, typeof(UInt64)),
                                                                        Tuple.Create(UINT32, typeof(UInt32)),
                                                                        Tuple.Create(FLOAT32, typeof(float)),
                                                                        Tuple.Create(FLOAT64, typeof(Double)),
                                                                        Tuple.Create(BYTES, typeof(byte[])),
                                                                        Tuple.Create(UUID, typeof(Guid)),
                                                                        Tuple.Create(STRING, typeof(String)),
                                                                        Tuple.Create(BOOL, typeof(bool)),
        };

        private static string converterName = "Converter";

        private const string INT32 = "int32";
        private const string INT64 = "int64";
        private const string UINT32 = "uint32";
        private const string UINT64 = "uint64";
        private const string FLOAT32 = "float32";
        private const string FLOAT64 = "float64";
        private const string BYTES = "bytes";
        private const string UUID = "uuid";
        private const string STRING = "string";
        private const string BOOL = "bool";

        private const string BASE_URL = "tempuri.org";
        private static string TemplateName;

        private static void Main(string[] args)
        {
            string file;
            string stdNs;
            string targetCode;
            string targetXsd;
            string targetRuntime;
            try
            {
                file = args[0];
                stdNs = args[1];
                targetRuntime = args[2];
                converterName = System.IO.Path.GetFileNameWithoutExtension(file);
                targetCode = System.IO.Path.ChangeExtension(file, "g.cs");
                targetXsd = System.IO.Path.ChangeExtension(file, "g.xsd");

                switch (targetRuntime)
                {
                    case "PCL":
                        TemplateName = "Misc.Generator.TemplatePcl.cs";
                        break;

                    default:
                        TemplateName = "Misc.Generator.Template.cs";
                        break;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Fehler beim Veraarbeiten der Kommandozeilen Elemente (Erstes Argument: XML, Zweites: c# namespace, 3. Target Runtime)");
                throw;
            }
            IEnumerable<GType> types;
            IEnumerable<GEnum> enums;
            try
            {
                // XML Parsen und Validieren
                var d = new System.Xml.XmlDocument();
                d.Load(file);

                using (var str = typeof(Program).Assembly.GetManifestResourceStream("Misc.Generator.Definition.xsd"))
                {
                    var schema = System.Xml.Schema.XmlSchema.Read(str, Handler); ;

                    var set = new System.Xml.Schema.XmlSchemaSet();
                    set.Add(schema);
                    set.Compile();

                    d.Schemas = set;
                    d.Validate(Valid);
                }

                nsmgr = new System.Xml.XmlNamespaceManager(d.NameTable);
                nsmgr.AddNamespace("my", "http://tempuri.org/Definition.xsd");

                types = d.SelectNodes("/my:Root/my:Type", nsmgr).Cast<System.Xml.XmlNode>().Select(x => new GType(x)).ToArray();
                enums = d.SelectNodes("/my:Root/my:Enum", nsmgr).Cast<System.Xml.XmlNode>().Select(x => new GEnum(x)).ToArray();
            }
            catch (Exception)
            {
                Console.WriteLine("Fehler beim Parsen");
                throw;
            }            // Generie Klassen

            GenerateCode(stdNs, targetCode, types, enums);
            GenerateXsd(targetXsd, types, enums);
        }

        private static void GenerateXsd(string targetXsd, IEnumerable<GType> types, IEnumerable<GEnum> enums)
        {
            StringBuilder b = new StringBuilder();

            var name = converterName;

            b.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            b.AppendLine(String.Format("<xs:schema id=\"{0}\"", name));
            b.AppendLine(String.Format("    targetNamespace=\"http://{1}/{0}.xsd\"", name, BASE_URL));
            b.AppendLine("    elementFormDefault=\"qualified\"");
            b.AppendLine(String.Format("    xmlns=\"http://{1}/{0}.xsd\"", name, BASE_URL));
            b.AppendLine(String.Format("    xmlns:mstns=\"http://{1}/{0}.xsd\"", name, BASE_URL));
            b.AppendLine("    xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"");
            b.AppendLine(">");

            foreach (var t in types)
            {
                b.AppendLine(String.Format("<xs:element name=\"{0}\" type=\"{0}\" />", t.Name));
            }

            foreach (var e in enums)
            {
                b.AppendLine(String.Format("<xs:element name=\"{0}\" type=\"{0}\" />", e.Name));
            }

            foreach (var e in enums)
            {
                b.AppendLine(String.Format("  <xs:simpleType name=\"{0}\">                                                   ", e.Name));
                b.AppendLine("    <xs:restriction base=\"xs:string\">                                           ");
                foreach (var v in e.Values)
                {
                    b.AppendLine(String.Format("      <xs:enumeration value=\"{0}\"/>                                            ", v));
                }
                b.AppendLine("    </xs:restriction>                                                             ");
                b.AppendLine("  </xs:simpleType>                                                                ");
            }

            foreach (var t in types)
            {
                b.AppendLine(String.Format("<xs:complexType name=\"{0}\" >", t.Name));
                b.AppendLine("<xs:all>");
                foreach (var p in t.Propertys)
                {
                    if (p.IsList)
                    {
                        string type;

                        if (TypeConverter.Any(x => x.Item2.Name == p.Type))
                            type = TypeConverter.First(x => x.Item2.Name == p.Type).Item1;
                        else
                            type = p.Type;
                        b.AppendLine(String.Format("<xs:element name=\"{0}\" >", p.Name));
                        b.AppendLine("<xs:complexType>");
                        b.AppendLine("<xs:sequence>");
                        b.AppendLine(String.Format("<xs:element name=\"{0}\" type=\"{0}\" minOccurs=\"0\" maxOccurs=\"unbounded\"  />", type));
                        b.AppendLine("</xs:sequence>");
                        b.AppendLine("</xs:complexType>");
                        b.AppendLine("</xs:element> ");
                    }
                    else
                    {
                        if (TypeConverter.Any(x => x.Item2.Name == p.Type))
                            b.AppendLine(String.Format("<xs:element name=\"{0}\" type=\"{1}\" />", p.Name, TypeConverter.First(x => x.Item2.Name == p.Type).Item1));
                        else
                        {
                            b.AppendLine(String.Format("<xs:element name=\"{0}\" >", p.Name));
                            b.AppendLine("<xs:complexType>");
                            b.AppendLine("<xs:all>");
                            b.AppendLine(String.Format("<xs:element name=\"{0}\" type=\"{0}\" minOccurs=\"0\" />", p.Type));
                            b.AppendLine("</xs:all>");
                            b.AppendLine("</xs:complexType>");
                            b.AppendLine("</xs:element> ");
                        }
                    }
                }
                b.AppendLine("</xs:all>");
                b.AppendLine("</xs:complexType>");
            }

            b.AppendLine("  <xs:simpleType name=\"int32\">                                                  ");
            b.AppendLine("    <xs:restriction base=\"xs:int\"/>                                             ");
            b.AppendLine("  </xs:simpleType>                                                                ");
            b.AppendLine("                                                                                  ");
            b.AppendLine("  <xs:simpleType name=\"int64\">                                                  ");
            b.AppendLine("    <xs:restriction base=\"xs:long\"/>                                            ");
            b.AppendLine("  </xs:simpleType>                                                                ");
            b.AppendLine("                                                                                  ");
            b.AppendLine("  <xs:simpleType name=\"uint32\">                                                  ");
            b.AppendLine("    <xs:restriction base=\"xs:unsignedInt\"/>                                             ");
            b.AppendLine("  </xs:simpleType>                                                                ");
            b.AppendLine("                                                                                  ");
            b.AppendLine("  <xs:simpleType name=\"uint64\">                                                  ");
            b.AppendLine("    <xs:restriction base=\"xs:unsignedLong\"/>                                            ");
            b.AppendLine("  </xs:simpleType>                                                                ");
            b.AppendLine("                                                                                  ");
            b.AppendLine("  <xs:simpleType name=\"float32\">                                                ");
            b.AppendLine("    <xs:restriction base=\"xs:float\"/>                                           ");
            b.AppendLine("  </xs:simpleType>                                                                ");
            b.AppendLine("                                                                                  ");
            b.AppendLine("  <xs:simpleType name=\"float64\">                                                ");
            b.AppendLine("    <xs:restriction base=\"xs:double\"/>                                          ");
            b.AppendLine("  </xs:simpleType>                                                                ");
            b.AppendLine("                                                                                  ");
            b.AppendLine("  <xs:simpleType name=\"bytes\">                                                  ");
            b.AppendLine("    <xs:restriction base =\"xs:base64Binary\"/>                                   ");
            b.AppendLine("  </xs:simpleType>                                                                ");
            b.AppendLine("                                                                                  ");
            b.AppendLine("  <xs:simpleType name=\"string\">                                                 ");
            b.AppendLine("    <xs:restriction base=\"xs:string\"/>                                          ");
            b.AppendLine("  </xs:simpleType>                                                                ");
            b.AppendLine("                                                                                  ");
            b.AppendLine("  <xs:simpleType name=\"bool\">                                                   ");
            b.AppendLine("    <xs:restriction base=\"xs:string\">                                           ");
            b.AppendLine("      <xs:enumeration value=\"true\"/>                                            ");
            b.AppendLine("      <xs:enumeration value=\"false\"/>                                           ");
            b.AppendLine("      <xs:enumeration value=\"True\"/>                                            ");
            b.AppendLine("      <xs:enumeration value=\"False\"/>                                           ");
            b.AppendLine("    </xs:restriction>                                                             ");
            b.AppendLine("  </xs:simpleType>                                                                ");
            b.AppendLine("                                                                                  ");
            b.AppendLine("  <xs:simpleType name=\"uuid\">                                                   ");
            b.AppendLine("    <xs:restriction base=\"xs:string\">                                           ");
            b.AppendLine("      <xs:pattern value=\"\\{[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\\}\" />");
            b.AppendLine("    </xs:restriction>                                                             ");
            b.AppendLine("  </xs:simpleType>                                                                ");

            b.AppendLine("</xs:schema>");

            var erg = b.ToString();

            var doc = new System.Xml.XmlDocument();
            doc.LoadXml(erg);

            using (var str = new System.Xml.XmlTextWriter(targetXsd, Encoding.UTF8))
            {
                str.Formatting = System.Xml.Formatting.Indented;
                doc.WriteTo(str);
            }
        }

        private static void GenerateCode(string stdNs, string target, IEnumerable<GType> types, IEnumerable<GEnum> enums)
        {
            string templateClass;
            var name = converterName;
            using (var str = typeof(Program).Assembly.GetManifestResourceStream(TemplateName))
            {
                var txtreader = new System.IO.StreamReader(str);
                templateClass = txtreader.ReadToEnd();

                templateClass = templateClass.Replace("StdNNN", stdNs);
                templateClass = templateClass.Replace("ZZZ", converterName);
                templateClass = templateClass.Replace("NNN", String.Format("http://{1}/{0}.xsd", name, BASE_URL));
                templateClass = templateClass.Replace("EEE", enums.Select(x => x.Name).Select(x => "\"" + stdNs + "." + x + "\"").Aggregate((x, y) => x + ", " + y));
            }

            StringBuilder b = new StringBuilder();

            b.AppendLine(templateClass);
            b.AppendLine();
            b.AppendLine(String.Format(@"namespace {0}", stdNs));
            b.AppendLine("{");

            foreach (var t in types.Select(GenerateClass))
            {
                b.AppendLine(t);
            }

            foreach (var e in enums.Select(GenerateEnum))
            {
                b.AppendLine(e);
            }

            b.AppendLine("}");

            var erg = b.ToString();

            using (var str = new System.IO.StreamWriter(target))
            {
                str.Write(erg);
            }
        }

        private static String GenerateEnum(GEnum arg)
        {
            StringBuilder b = new StringBuilder();

            b.AppendLine(String.Format("public enum {0}", arg.Name));
            b.AppendLine("{");

            foreach (var v in arg.Values)
            {
                b.AppendLine(v + ",");
            }

            b.AppendLine("}");
            return b.ToString();
        }

        private static String GenerateClass(GType arg)
        {
            StringBuilder b = new StringBuilder();

            b.AppendLine();

            b.AppendLine(String.Format("    public partial class {0} : {1}", arg.Name, converterName));
            b.AppendLine("    {");

            foreach (var p in arg.Propertys.Select(GenerateProperty))
            {
                b.AppendLine();
                b.AppendLine(p);
            }
            b.AppendLine();

            b.Append(@"        protected override string[] GetPropertysToSerilize
        {
            get { return new String[] {");

            foreach (var item in arg.Propertys.Select(x => x.Name))
            {
                b.Append("\"");
                b.Append(item);
                b.Append("\",");
            }
            b.Append(@" }; }
        }
");

            b.AppendLine("    }");
            b.AppendLine();
            return b.ToString();
        }

        private static String GenerateProperty(Property arg)
        {
            if (arg.IsList)
            {
                return String.Format(@"        private readonly List<{1}> _{0} = new List<{1}>();

        public List<{1}> {0}
        {{
            get {{ return this._{0}; }}
        }}
", arg.Name, arg.Type);
            }
            else
            {
                return String.Format("        public {1} {0} {{ get; set; }}", arg.Name, arg.Type);
            }
        }

        private static void Valid(object sender, System.Xml.Schema.ValidationEventArgs e)
        {
            Console.WriteLine("Format Exception");
            throw new FormatException();
        }

        private static void Handler(object sender, System.Xml.Schema.ValidationEventArgs e)
        {
            Console.WriteLine("XSD Exception");
            throw new FormatException();
        }

        [System.Diagnostics.DebuggerDisplay("{Name}")]
        internal class GType
        {
            public GType(System.Xml.XmlNode node)
            {
                this.Name = node.Attributes.Cast<System.Xml.XmlAttribute>().First(x => x.LocalName == "Name").Value;
                this.Propertys = node.SelectNodes("my:Property", nsmgr).Cast<System.Xml.XmlNode>().Select(x => new Property(x)).ToArray();
            }

            public string Name { get; set; }

            public Property[] Propertys { get; set; }
        }

        [System.Diagnostics.DebuggerDisplay("{Name}")]
        internal class GEnum
        {
            public GEnum(System.Xml.XmlNode node)
            {
                this.Name = node.Attributes.Cast<System.Xml.XmlAttribute>().First(x => x.LocalName == "Name").Value;
                this.Values = node.SelectNodes("my:Value", nsmgr).Cast<System.Xml.XmlNode>().Select(x => x.InnerText).ToArray();
            }

            public string Name { get; set; }

            public String[] Values { get; set; }
        }

        [System.Diagnostics.DebuggerDisplay("{Name}({Type}), IsList={IsList}")]
        internal class Property
        {
            public Property(System.Xml.XmlNode node)
            {
                this.Name = node.Attributes.Cast<System.Xml.XmlAttribute>().First(x => x.LocalName == "Name").Value;
                var isListString = node.Attributes.Cast<System.Xml.XmlAttribute>().First(x => x.LocalName == "IsList").Value;
                bool isList;
                if (!bool.TryParse(isListString, out isList))
                    isList = int.Parse(isListString) == 1;
                IsList = isList;
                this.Type = node.Attributes.Cast<System.Xml.XmlAttribute>().First(x => x.LocalName == "Type").Value;

                // Die Primitiven Typen müssen richtig bennant werden
                var toConvert = TypeConverter.FirstOrDefault(x => x.Item1 == this.Type);
                if (toConvert != null)
                    this.Type = toConvert.Item2.Name;
            }

            public string Name { get; set; }

            public bool IsList { get; set; }

            public string Type { get; set; }
        }
    }
}