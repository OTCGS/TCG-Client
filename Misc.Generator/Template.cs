using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xml = Windows.Data.Xml.Dom;

namespace StdNNN
{
    public abstract class ZZZ
    {
        private const string NAMESPACE = "NNN";

        public static string Convert(object toConvert)
        {
            var doc = new Xml.XmlDocument();

            Xml.XmlElement ele = PersistRoot(toConvert, doc);

            doc.AppendChild(ele);
            return doc.GetXml();
        }

        public static T Convert<T>(string toConvert)
        {
            var doc = new Xml.XmlDocument();

            doc.LoadXml(toConvert);

            var type = typeof(T);
            var element = doc.SelectSingleNodeNS("/std:" + type.Name, "xmlns:std=\"" + NAMESPACE + "\"");
            var erg = DePersist(element);

            return (T)erg;
        }

        public static object Convert(string toConvert)
        {
            var doc = new Xml.XmlDocument();

            doc.LoadXml(toConvert);

            var element = doc.SelectSingleNodeNS("/std:*", "xmlns:std=\"" + NAMESPACE + "\"");
            var erg = DePersist(element);

            return erg;
        }

        private static object DePersist(Xml.IXmlNode element)
        {
            switch (element.NodeName)
            {
                case "int32":
                    return Int32.Parse(element.InnerText);

                case "int64":
                    return Int64.Parse(element.InnerText);

                case "uint32":
                    return UInt32.Parse(element.InnerText);

                case "uint64":
                    return UInt64.Parse(element.InnerText);

                case "float32":
                    return float.Parse(element.InnerText);

                case "float64":
                    return double.Parse(element.InnerText);

                case "bytes":
                    return System.Convert.FromBase64String(element.InnerText);

                case "string":
                    return System.Net.WebUtility.HtmlDecode(element.InnerText);

                case "uuid":
                    return Guid.Parse(element.InnerText);

                case "bool":
                    return bool.Parse(element.InnerText);

                default:
                    var type = GetTypes().FirstOrDefault(x => x.Name == element.NodeName);
                    var typeInfo = type.GetTypeInfo();
                    if (typeInfo.IsEnum)
                        return Enum.Parse(type, element.InnerText);

                    var toReturn = Activator.CreateInstance(type) as ZZZ;

                    foreach (var p in typeInfo.DeclaredProperties.Where(x => toReturn.GetPropertysToSerilize.Contains(x.Name)))
                    {
                        var pElement = element.SelectSingleNodeNS("std:" + p.Name, "xmlns:std=\"" + NAMESPACE + "\"");
                        if (pElement == null)
                            continue;

                        object value = null;
                        if (p.PropertyType == typeof(Int32))
                        {
                            value = Int32.Parse(pElement.InnerText);
                        }
                        else if (p.PropertyType == typeof(Int64))
                        {
                            value = Int64.Parse(pElement.InnerText);
                        }
                        else if (p.PropertyType == typeof(UInt32))
                        {
                            value = UInt32.Parse(pElement.InnerText);
                        }
                        else if (p.PropertyType == typeof(UInt64))
                        {
                            value = UInt64.Parse(pElement.InnerText);
                        }
                        else if (p.PropertyType == typeof(float))
                        {
                            value = float.Parse(pElement.InnerText);
                        }
                        else if (p.PropertyType == typeof(double))
                        {
                            value = double.Parse(pElement.InnerText);
                        }
                        else if (p.PropertyType == typeof(byte[]))
                        {
                            value = System.Convert.FromBase64String(pElement.InnerText);
                        }
                        else if (p.PropertyType == typeof(Guid))
                        {
                            value = Guid.Parse(pElement.InnerText);
                        }
                        else if (p.PropertyType == typeof(string))
                        {
                            value = System.Net.WebUtility.HtmlDecode(pElement.InnerText);
                        }
                        else if (p.PropertyType == typeof(bool))
                        {
                            value = bool.Parse(pElement.InnerText);
                        }
                        else if (IsList(p.PropertyType))
                        {
                            value = null;

                            var list = p.GetValue(toReturn) as System.Collections.IList;

                            foreach (var node in pElement.ChildNodes)
                            {
                                list.Add(DePersist(node));
                            }
                            continue;
                        }
                        else
                        {
                            if (pElement.FirstChild != null)
                                value = DePersist(pElement.FirstChild);
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

        private static Xml.XmlElement PersistRoot(Object input, Xml.XmlDocument doc)
        {
            if (input == null)
                return null;
            var type = input.GetType();
            var typeInfo = type.GetTypeInfo();

            var ele = doc.CreateElementNS(NAMESPACE, type.Name);

            if (type == typeof(Int32))
            {
                ele = doc.CreateElementNS(NAMESPACE, "int32");
                ele.InnerText = input.ToString();
            }
            else if (type == typeof(Int64))
            {
                ele = doc.CreateElementNS(NAMESPACE, "int64");
                ele.InnerText = input.ToString();
            }
            else if (type == typeof(UInt32))
            {
                ele = doc.CreateElementNS(NAMESPACE, "uint32");
                ele.InnerText = input.ToString();
            }
            else if (type == typeof(UInt64))
            {
                ele = doc.CreateElementNS(NAMESPACE, "uint64");
                ele.InnerText = input.ToString();
            }
            else if (type == typeof(float))
            {
                ele = doc.CreateElementNS(NAMESPACE, "float32");
                ele.InnerText = input.ToString();
            }
            else if (type == typeof(double))
            {
                ele = doc.CreateElementNS(NAMESPACE, "float64");
                ele.InnerText = input.ToString();
            }
            else if (type == typeof(byte[]))
            {
                ele = doc.CreateElementNS(NAMESPACE, "bytes");
                var value = input as byte[];
                if (value != null)
                {
                    ele.InnerText = System.Convert.ToBase64String(value);
                }
            }
            else if (type == typeof(Guid))
            {
                ele = doc.CreateElementNS(NAMESPACE, "uuid");
                ele.InnerText = ((Guid)input).ToString("B");
            }
            else if (type == typeof(string))
            {
                ele = doc.CreateElementNS(NAMESPACE, "string");
                var value = input as String;
                ele.InnerText = System.Net.WebUtility.HtmlEncode(value ?? "");
            }
            else if (type == typeof(bool))
            {
                ele = doc.CreateElementNS(NAMESPACE, "bool");
                ele.InnerText = input.ToString();
            }
            else if (type.GetTypeInfo().IsEnum)
            {
                ele = doc.CreateElementNS(NAMESPACE, type.Name);
                ele.InnerText = input.ToString();
            }
            else
            {
                ZZZ toConvert = (ZZZ)input;

                foreach (var p in typeInfo.DeclaredProperties.Where(x => toConvert.GetPropertysToSerilize.Contains(x.Name)))
                {
                    var pChild = doc.CreateElementNS(NAMESPACE, p.Name);
                    ele.AppendChild(pChild);

                    if (p.PropertyType == typeof(Int32))
                    {
                        pChild.InnerText = p.GetValue(toConvert).ToString();
                    }
                    else if (p.PropertyType == typeof(Int64))
                    {
                        pChild.InnerText = p.GetValue(toConvert).ToString();
                    }
                    else if (p.PropertyType == typeof(UInt32))
                    {
                        pChild.InnerText = p.GetValue(toConvert).ToString();
                    }
                    else if (p.PropertyType == typeof(UInt64))
                    {
                        pChild.InnerText = p.GetValue(toConvert).ToString();
                    }
                    else if (p.PropertyType == typeof(float))
                    {
                        pChild.InnerText = p.GetValue(toConvert).ToString();
                    }
                    else if (p.PropertyType == typeof(double))
                    {
                        pChild.InnerText = p.GetValue(toConvert).ToString();
                    }
                    else if (p.PropertyType == typeof(byte[]))
                    {
                        var value = p.GetValue(toConvert) as byte[];
                        if (value != null)
                        {
                            pChild.InnerText = System.Convert.ToBase64String(value);
                        }
                    }
                    else if (p.PropertyType == typeof(Guid))
                    {
                        pChild.InnerText = ((Guid)p.GetValue(toConvert)).ToString("B");
                    }
                    else if (p.PropertyType == typeof(string))
                    {
                        var value = p.GetValue(toConvert) as String;
                        pChild.InnerText = System.Net.WebUtility.HtmlEncode(value ?? "");
                    }
                    else if (p.PropertyType == typeof(bool))
                    {
                        pChild.InnerText = p.GetValue(toConvert).ToString();
                    }
                    else if (IsList(p.PropertyType))
                    {
                        foreach (var value in GetListValus(p.GetValue(toConvert)))
                        {
                            if (value == null)
                                throw new ArgumentException("Listen dürfen keine null werte beinhalten.");
                            var vElement = PersistRoot(value, doc);
                            pChild.AppendChild(vElement);
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
                            pChild.AppendChild(vElement);
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