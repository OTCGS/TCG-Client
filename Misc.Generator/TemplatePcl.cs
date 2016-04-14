using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xml = System.Xml.Linq;

namespace StdNNN
{
    public abstract class ZZZ
    {
        private const string NAMESPACE = "NNN";

        public static string Convert(object toConvert)
        {
            var doc = new Xml.XDocument();

            Xml.XElement ele = PersistRoot(toConvert, doc);

            doc.Add(ele);
            return doc.ToString();
        }

        public static T Convert<T>(string toConvert)
        {
            var doc = Xml.XDocument.Parse(toConvert);

            var type = typeof(T);
            var element = doc.Element(Xml.XName.Get(type.Name, NAMESPACE));
            var erg = DePersist(element);

            return (T)erg;
        }

        public static object Convert(string toConvert)
        {
            var doc = Xml.XDocument.Parse(toConvert);

            var element = doc.Elements().First();
            var erg = DePersist(element);

            return erg;
        }

        private static object DePersist(Xml.XElement element)
        {
            switch (element.Name.LocalName)
            {
                case "int32":
                    return Int32.Parse(element.Value);

                case "int64":
                    return Int64.Parse(element.Value);

                case "uint32":
                    return UInt32.Parse(element.Value);

                case "uint64":
                    return UInt64.Parse(element.Value);

                case "float32":
                    return float.Parse(element.Value);

                case "float64":
                    return double.Parse(element.Value);

                case "bytes":
                    return System.Convert.FromBase64String(element.Value);

                case "string":
                    return System.Net.WebUtility.HtmlDecode(element.Value);

                case "uuid":
                    return Guid.Parse(element.Value);

                case "bool":
                    return bool.Parse(element.Value);

                default:
                    var type = GetTypes().FirstOrDefault(x => x.Name == element.Name.LocalName);
                    var typeInfo = type.GetTypeInfo();
                    if (typeInfo.IsEnum)
                        return Enum.Parse(type, element.Value);

                    var toReturn = Activator.CreateInstance(type) as ZZZ;

                    foreach (var p in typeInfo.DeclaredProperties.Where(x => toReturn.GetPropertysToSerilize.Contains(x.Name)))
                    {
                        var pElement = element.Element(Xml.XName.Get(p.Name, NAMESPACE));
                        if (pElement == null)
                            continue;

                        object value = null;
                        if (p.PropertyType == typeof(Int32))
                        {
                            value = Int32.Parse(pElement.Value);
                        }
                        else if (p.PropertyType == typeof(Int64))
                        {
                            value = Int64.Parse(pElement.Value);
                        }
                        else if (p.PropertyType == typeof(UInt32))
                        {
                            value = UInt32.Parse(pElement.Value);
                        }
                        else if (p.PropertyType == typeof(UInt64))
                        {
                            value = UInt64.Parse(pElement.Value);
                        }
                        else if (p.PropertyType == typeof(float))
                        {
                            value = float.Parse(pElement.Value);
                        }
                        else if (p.PropertyType == typeof(double))
                        {
                            value = double.Parse(pElement.Value);
                        }
                        else if (p.PropertyType == typeof(byte[]))
                        {
                            value = System.Convert.FromBase64String(pElement.Value);
                        }
                        else if (p.PropertyType == typeof(Guid))
                        {
                            value = Guid.Parse(pElement.Value);
                        }
                        else if (p.PropertyType == typeof(string))
                        {
                            value = System.Net.WebUtility.HtmlDecode(pElement.Value);
                        }
                        else if (p.PropertyType == typeof(bool))
                        {
                            value = bool.Parse(pElement.Value);
                        }
                        else if (IsList(p.PropertyType))
                        {
                            value = null;

                            var list = p.GetValue(toReturn) as System.Collections.IList;

                            foreach (var node in pElement.Elements())
                            {
                                list.Add(DePersist(node));
                            }
                            continue;
                        }
                        else
                        {
                            if (pElement.HasElements)
                                value = DePersist(pElement.Elements().First());
                        }

                        p.SetValue(toReturn, value);
                    }

                    return toReturn;
            }
        }

        private static IEnumerable<Type> GetTypes()
        {
            var str = new String[] { EEE };
            return typeof(ZZZ).GetTypeInfo().Assembly.DefinedTypes.Where(x => x.BaseType == typeof(ZZZ)).Select(x => x.AsType()).Concat(typeof(ZZZ).GetTypeInfo().Assembly.DefinedTypes.Where(x => str.Contains(x.FullName) && x.IsEnum).Select(x => x.AsType()));
        }

        private static Xml.XElement PersistRoot(Object input, Xml.XDocument doc)
        {
            if (input == null)
                return null;
            var type = input.GetType();
            var typeInfo = type.GetTypeInfo();

            var ele = new Xml.XElement(Xml.XName.Get(type.Name, NAMESPACE));

            if (type == typeof(Int32))
            {
                ele = new Xml.XElement(Xml.XName.Get("int32", NAMESPACE));
                ele.Value = input.ToString();
            }
            else if (type == typeof(Int64))
            {
                ele = new Xml.XElement(Xml.XName.Get("int64", NAMESPACE));
                ele.Value = input.ToString();
            }
            else if (type == typeof(UInt32))
            {
                ele = new Xml.XElement(Xml.XName.Get("uint32", NAMESPACE));
                ele.Value = input.ToString();
            }
            else if (type == typeof(UInt64))
            {
                ele = new Xml.XElement(Xml.XName.Get("uint64", NAMESPACE));
                ele.Value = input.ToString();
            }
            else if (type == typeof(float))
            {
                ele = new Xml.XElement(Xml.XName.Get("float32", NAMESPACE));
                ele.Value = input.ToString();
            }
            else if (type == typeof(double))
            {
                ele = new Xml.XElement(Xml.XName.Get("float64", NAMESPACE));
                ele.Value = input.ToString();
            }
            else if (type == typeof(byte[]))
            {
                ele = new Xml.XElement(Xml.XName.Get("bytes", NAMESPACE));
                var value = input as byte[];
                if (value != null)
                {
                    ele.Value = System.Convert.ToBase64String(value);
                }
            }
            else if (type == typeof(Guid))
            {
                ele = new Xml.XElement(Xml.XName.Get("uuid", NAMESPACE));
                ele.Value = ((Guid)input).ToString("B");
            }
            else if (type == typeof(string))
            {
                ele = new Xml.XElement(Xml.XName.Get("string", NAMESPACE));
                var value = input as String;
                ele.Value = System.Net.WebUtility.HtmlEncode(value ?? "");
            }
            else if (type == typeof(bool))
            {
                ele = new Xml.XElement(Xml.XName.Get("bool", NAMESPACE));
                ele.Value = input.ToString();
            }
            else if (type.GetTypeInfo().IsEnum)
            {
                ele = new Xml.XElement(Xml.XName.Get(type.Name, NAMESPACE));
                ele.Value = input.ToString();
            }
            else
            {
                ZZZ toConvert = (ZZZ)input;

                foreach (var p in typeInfo.DeclaredProperties.Where(x => toConvert.GetPropertysToSerilize.Contains(x.Name)))
                {
                    var pChild = new Xml.XElement(Xml.XName.Get(p.Name, NAMESPACE));
                    ele.Add(pChild);

                    if (p.PropertyType == typeof(Int32))
                    {
                        pChild.Value = p.GetValue(toConvert).ToString();
                    }
                    else if (p.PropertyType == typeof(Int64))
                    {
                        pChild.Value = p.GetValue(toConvert).ToString();
                    }
                    else if (p.PropertyType == typeof(UInt32))
                    {
                        pChild.Value = p.GetValue(toConvert).ToString();
                    }
                    else if (p.PropertyType == typeof(UInt64))
                    {
                        pChild.Value = p.GetValue(toConvert).ToString();
                    }
                    else if (p.PropertyType == typeof(float))
                    {
                        pChild.Value = p.GetValue(toConvert).ToString();
                    }
                    else if (p.PropertyType == typeof(double))
                    {
                        pChild.Value = p.GetValue(toConvert).ToString();
                    }
                    else if (p.PropertyType == typeof(byte[]))
                    {
                        var value = p.GetValue(toConvert) as byte[];
                        if (value != null)
                        {
                            pChild.Value = System.Convert.ToBase64String(value);
                        }
                    }
                    else if (p.PropertyType == typeof(Guid))
                    {
                        pChild.Value = ((Guid)p.GetValue(toConvert)).ToString("B");
                    }
                    else if (p.PropertyType == typeof(string))
                    {
                        var value = p.GetValue(toConvert) as String;
                        pChild.Value = System.Net.WebUtility.HtmlEncode(value ?? "");
                    }
                    else if (p.PropertyType == typeof(bool))
                    {
                        pChild.Value = p.GetValue(toConvert).ToString();
                    }
                    else if (IsList(p.PropertyType))
                    {
                        foreach (var value in GetListValus(p.GetValue(toConvert)))
                        {
                            if (value == null)
                                throw new ArgumentException("Listen dürfen keine null werte beinhalten.");
                            var vElement = PersistRoot(value, doc);
                            pChild.Add(vElement);
                        }
                    }
                    else
                    {
                        object value;

                        try
                        {
                            value = p.GetValue(toConvert);
                        }
                        catch (InvalidCastException e)
                        {
                            throw new Exception("Dies sollte nicht Passieren", e);
                        }

                        var vElement = PersistRoot(value, doc);
                        if (vElement != null)
                            pChild.Add(vElement);
                    }
                }
            }

            return ele;
        }

        private static IEnumerable<object> GetListValus(object p)
        {
            var en = p as System.Collections.IEnumerable;
            foreach (var item in en)
                yield return item;
        }

        private static bool IsList(Type type)
        {
            if (!type.GetTypeInfo().IsGenericType)
                return false;
            var listType = typeof(List<>);
            var genericType = type.GetTypeInfo().GetGenericTypeDefinition();
            return genericType == listType;
        }

        protected abstract string[] GetPropertysToSerilize { get; }
    }
}