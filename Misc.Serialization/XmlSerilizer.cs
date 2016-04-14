using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Misc.Serialization
{
    public class XmlSerilizer<T>
    {
        private readonly Dictionary<Type, Func<Type[], object>> genericFactoryLookup = new Dictionary<Type, Func<Type[], object>>();
        private readonly Dictionary<Type, Func<object>> factoryLookup = new Dictionary<Type, Func<object>>();
        private readonly Dictionary<Type, Func<string, object>> parserLookup = new Dictionary<Type, Func<string, object>>();
        private readonly Dictionary<Type, Func<object, string>> writerLookup = new Dictionary<Type, Func<object, string>>();

        private readonly Dictionary<Type, Func<object, IEnumerable<object>>> getDataFromObjectGenericLookup = new Dictionary<Type, Func<object, IEnumerable<object>>>();
        private readonly Dictionary<Type, Func<IEnumerable<object>, Type[], object>> getObjectFromDataGenericLookup = new Dictionary<Type, Func<IEnumerable<object>, Type[], object>>();
        private readonly Dictionary<Type, Func<Type[], Type>> getCollectionEncapsuledTypeGenericLookup = new Dictionary<Type, Func<Type[], Type>>();

        private readonly Dictionary<Type, Tuple<Type, Func<object, IEnumerable<object>>>> getDataFromObjectLookup = new Dictionary<Type, Tuple<Type, Func<object, IEnumerable<object>>>>();
        private readonly Dictionary<Type, Tuple<Type, Func<IEnumerable<object>, object>>> getObjectFromDataLookup = new Dictionary<Type, Tuple<Type, Func<IEnumerable<object>, object>>>();

        private readonly HashSet<Type> supressEnumerator = new HashSet<Type>();

        public XmlSerilizer()
        {
            // Add Stdandard Parser
            AddParser<Guid>(str => Guid.Parse(str), g => g.ToString());
            AddParser<byte[]>(str => Convert.FromBase64String(str), b => Convert.ToBase64String(b));
            AddParser<String>(str => str, str => str);
            AddParser<Int32>(str => int.Parse(str), i => i.ToString());
            AddParser<Int16>(str => Int16.Parse(str), i => i.ToString());
            AddParser<Int64>(str => Int64.Parse(str), i => i.ToString());
            AddParser<UInt32>(str => UInt32.Parse(str), i => i.ToString());
            AddParser<UInt16>(str => UInt16.Parse(str), i => i.ToString());
            AddParser<UInt64>(str => UInt64.Parse(str), i => i.ToString());

            AddGenericCollectionHandler<IList<dynamic>>(
                l => ((System.Collections.IEnumerable)l).OfType<object>(),
                (en, gtyp) =>
                {
                    var type = typeof(List<>).GetGenericTypeDefinition().MakeGenericType(gtyp).GetTypeInfo();
                    var con = type.DeclaredConstructors.First(x => x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType != typeof(int));
                    var methode = typeof(System.Linq.Enumerable).GetRuntimeMethod("OfType", new Type[] { typeof(System.Collections.IEnumerable) }).GetGenericMethodDefinition().MakeGenericMethod(gtyp);
                    var erg = methode.Invoke(null, new Object[] { en });
                    return con.Invoke(new Object[] { erg });
                },
                typpara => typpara[0]);
        }


        public void AddGenericFactoryMethod<E>(Func<Type[], E> factoryMethode)
        {
            genericFactoryLookup.Add(typeof(E).GetGenericTypeDefinition(), x => factoryMethode(x));
        }

        public void AddFactoryMethod<E>(Func<E> factoryMethode)
        {
            factoryLookup.Add(typeof(E), () => factoryMethode());
        }

        public void AddParser<E>(Func<string, E> parser, Func<E, string> writer)
        {
            parserLookup.Add(typeof(E), str => parser(str));
            writerLookup.Add(typeof(E), obj => writer((E)obj));
        }

        public void AddGenericCollectionHandler<E>(Func<object, IEnumerable<object>> getDataFromObject, Func<IEnumerable<Object>, Type[], dynamic> getObjectFromData, Func<Type[], Type> getEncapsuledTypeArgument)
        {
            this.getDataFromObjectGenericLookup.Add(typeof(E).GetGenericTypeDefinition(), e =>
            {
                var data = getDataFromObject(e);
                return data.OfType<object>();
            }
                );
            this.getObjectFromDataGenericLookup.Add(typeof(E).GetGenericTypeDefinition(), (data, typedef) => getObjectFromData(data, typedef));
            this.getCollectionEncapsuledTypeGenericLookup.Add(typeof(E).GetGenericTypeDefinition(), getEncapsuledTypeArgument);
        }

        public void AddCollectionHandler<E, R>(Func<E, IEnumerable<R>> getDataFromObject, Func<IEnumerable<R>, E> getObjectFromData)
        {
            this.getDataFromObjectLookup.Add(typeof(E), new Tuple<Type, Func<object, IEnumerable<object>>>(typeof(R), e => getDataFromObject((E)e).OfType<object>()));
            this.getObjectFromDataLookup.Add(typeof(E), new Tuple<Type, Func<IEnumerable<object>, object>>(typeof(R), (data) => getObjectFromData(data.OfType<R>())));
        }

        public T Deserilize(string xml)
        {
            var doc = XDocument.Parse(xml);
            var root = doc.Root;
            var type = typeof(T);
            T obj = (T)GenerateObject(type);

            var xmlAttriue = type.GetTypeInfo().GetCustomAttribute<XmlClassAttribute>();
            if (xmlAttriue != null)
                Deserilize(root, obj, type);
            else
                Deserilize(root, obj, type);
            return obj;
        }

        private Object GenerateObject(Type type)
        {
            if (factoryLookup.ContainsKey(type))
                return factoryLookup[type]();
            if (type.GenericTypeArguments.Length > 0 && genericFactoryLookup.ContainsKey(type.GetGenericTypeDefinition()))
                return genericFactoryLookup[type.GetGenericTypeDefinition()](type.GenericTypeArguments);

            var con = type.GetTypeInfo().DeclaredConstructors.FirstOrDefault(x => x.GetParameters().Length == 0);
            if (con != null)
                return con.Invoke(new Object[0]);

            throw new ArgumentException("Cannot Create Instance of " + type);
        }

        private void Deserilize(XElement element, object obj, Type type)
        {
            bool valueFound = false;
            var propertiesToCheck = this.GetProperties(type);
            foreach (var property in propertiesToCheck)
            {
                if (property.GetIndexParameters().Length > 0)
                    continue; // ignore indexer;

                var isIgnore = property.GetCustomAttribute<XmlIgnoreAttribute>() != null;
                var ele = property.GetCustomAttribute<XmlElementAttribute>();
                var attr = property.GetCustomAttribute<XmlAttributeAttribute>();
                var valueAttr = property.GetCustomAttribute<XmlValueAttribute>() != null;
                var en = property.PropertyType.GetTypeInfo().ImplementedInterfaces.FirstOrDefault(x => x.Name == typeof(IEnumerable<>).Name && x.GetTypeInfo().Module == typeof(IEnumerable<>).GetTypeInfo().Module && x.GetTypeInfo().Namespace == typeof(IEnumerable<>).GetTypeInfo().Namespace);

                if (isIgnore)
                    continue;
                if (valueAttr && valueFound)
                    throw new ArgumentException("Only one Property on Class may be Value");

                var setter = property.SetMethod;
                if (setter == null)
                {
                    var objProp = obj.GetType().GetRuntimeProperty(property.Name);
                    if (objProp != null)
                        setter = objProp.SetMethod;
                }

                if (setter == null)
                    throw new ArgumentException("property doesent have get, cant read Value. Consider Using XmlIgnore");
                valueFound = valueAttr;

                if (valueAttr)
                {
                    var data = element.Value;
                    var value = parserLookup[property.PropertyType](data);
                    setter.Invoke(obj, new Object[] { value });
                }
                else if (attr != null)
                {
                    var xAttribute = element.Attribute(XName.Get(attr.Name));
                    object value;
                    if (xAttribute != null)
                    {
                        var data = xAttribute.Value;
                        value = parserLookup[property.PropertyType](data);
                    }
                    else
                    {
                        value = GetDefaultValue(property.PropertyType);
                    }
                    setter.Invoke(obj, new Object[] { value });
                }

                else
                {
                    string name;
                    if (ele != null)
                        name = ele.Name;
                    else
                        name = property.Name;
                    var currentElement = element.Element(XName.Get(name));
                    if (currentElement == null)
                        continue;
                    if (parserLookup.ContainsKey(property.PropertyType))
                    {
                        var data = currentElement.Value;
                        var value = parserLookup[property.PropertyType](data);
                        setter.Invoke(obj, new Object[] { value });
                    }
                    else if (this.getObjectFromDataLookup.ContainsKey(property.PropertyType))
                    {
                        var tupel = getObjectFromDataLookup[property.PropertyType];

                        var typeArgument = tupel.Item1;
                        var xmlAttriue = typeArgument.GetTypeInfo().GetCustomAttribute<XmlClassAttribute>();

                        var arrayElement = currentElement;
                        string arrayElementName;
                        if (xmlAttriue != null)
                            arrayElementName = xmlAttriue.Name;
                        else
                            arrayElementName = typeArgument.Name;

                        var arrayElementContent = arrayElement.Elements(XName.Get(arrayElementName)).ToArray();

                        Array objects = Array.CreateInstance(typeArgument, arrayElementContent.Length);
                        for (int i = 0; i < objects.Length; i++)
                        {
                            var value = GenerateObject(typeArgument);
                            Deserilize(arrayElementContent[i], value, typeArgument);
                            objects.SetValue(value, i);
                        }
                        var data = tupel.Item2(objects.OfType<Object>());
                        setter.Invoke(obj, new Object[] { data });
                    }

                    else if (property.PropertyType.GetTypeInfo().IsGenericType && this.getObjectFromDataGenericLookup.ContainsKey(property.PropertyType.GetGenericTypeDefinition()))
                    {
                        var tupel = getObjectFromDataGenericLookup[property.PropertyType.GetGenericTypeDefinition()];

                        var typeArgument = getCollectionEncapsuledTypeGenericLookup[property.PropertyType.GetGenericTypeDefinition()](property.PropertyType.GenericTypeArguments);
                        var xmlAttriue = typeArgument.GetTypeInfo().GetCustomAttribute<XmlClassAttribute>();

                        var arrayElement = currentElement;
                        string arrayElementName;
                        if (xmlAttriue != null)
                            arrayElementName = xmlAttriue.Name;
                        else
                            arrayElementName = typeArgument.Name;

                        var arrayElementContent = arrayElement.Elements(XName.Get(arrayElementName)).ToArray();

                        Array objects = Array.CreateInstance(typeArgument, arrayElementContent.Length);
                        for (int i = 0; i < objects.Length; i++)
                        {
                            var value = GenerateObject(typeArgument);
                            Deserilize(arrayElementContent[i], value, typeArgument);
                            objects.SetValue(value, i);
                        }
                        var data = tupel(objects.OfType<Object>(), property.PropertyType.GenericTypeArguments);
                        setter.Invoke(obj, new Object[] { data });
                    }
                    else if (property.PropertyType.IsArray)
                    {
                        var typeArgument = property.PropertyType.GetElementType();
                        var xmlAttriue = typeArgument.GetTypeInfo().GetCustomAttribute<XmlClassAttribute>();

                        var arrayElement = currentElement;
                        string arrayElementName;
                        if (xmlAttriue != null)
                            arrayElementName = xmlAttriue.Name;
                        else
                            arrayElementName = typeArgument.Name;

                        var arrayElementContent = arrayElement.Elements(XName.Get(arrayElementName)).ToArray();

                        Array objects = Array.CreateInstance(typeArgument, arrayElementContent.Length);
                        for (int i = 0; i < objects.Length; i++)
                        {
                            var value = GenerateObject(typeArgument);
                            Deserilize(arrayElementContent[i], value, typeArgument);
                            objects.SetValue(value, i);
                        }
                        setter.Invoke(obj, new Object[] { objects });
                    }
                    else if (property.PropertyType.GetTypeInfo().IsEnum)
                    {
                        var value = Enum.Parse(property.PropertyType, currentElement.Value);
                        setter.Invoke(obj, new Object[] { value });
                    }
                    else
                    {
                        var value = GenerateObject(property.PropertyType);
                        Deserilize(currentElement, value, property.PropertyType);
                        setter.Invoke(obj, new Object[] { value });
                    }
                }
            }
        }

        private object GetDefaultValue(Type type)
        {
            if (type.GetTypeInfo().IsValueType)
                return Activator.CreateInstance(type);
            return null;
        }

        public string Serialize(T obj)
        {
            var doc = new XDocument();
            var type = typeof(T);
            using (var writer = doc.CreateWriter())
            {
                var xmlAttriue = type.GetTypeInfo().GetCustomAttribute<XmlClassAttribute>();
                if (xmlAttriue != null)
                    Serialize(writer, obj, type, xmlAttriue.Name);
                else
                    Serialize(writer, obj, type, type.Name);

                writer.Flush();
            }
            return doc.ToString();
        }

        private void Serialize(XmlWriter writer, object obj, Type type, string elementName, bool isValue = false)
        {
            if (obj == null)
                return;
            if (writerLookup.ContainsKey(type))
            {
                var data = writerLookup[type](obj);
                if (isValue)
                    writer.WriteValue(data);
                else
                    writer.WriteElementString(elementName, data);
            }
            else if (isValue)
                throw new ArgumentException("Types that are Values must be havev a Parser/Writer");

            else if (this.getDataFromObjectLookup.ContainsKey(type))
            {
                var tupel = getDataFromObjectLookup[type];

                var typeArgument = tupel.Item1;
                var xmlAttriue = typeArgument.GetTypeInfo().GetCustomAttribute<XmlClassAttribute>();

                writer.WriteStartElement(elementName);
                foreach (var item in tupel.Item2(obj))
                {
                    if (xmlAttriue != null)
                        Serialize(writer, item, typeArgument, xmlAttriue.Name);
                    else
                        Serialize(writer, item, typeArgument, typeArgument.Name);
                }
                writer.WriteEndElement();
            }
            else if (type.GetTypeInfo().IsGenericType && this.getDataFromObjectGenericLookup.ContainsKey(type.GetGenericTypeDefinition()))
            {
                var tupel = getDataFromObjectGenericLookup[type.GetGenericTypeDefinition()];

                var typeArgument = getCollectionEncapsuledTypeGenericLookup[type.GetGenericTypeDefinition()](type.GenericTypeArguments);
                var xmlAttriue = typeArgument.GetTypeInfo().GetCustomAttribute<XmlClassAttribute>();

                writer.WriteStartElement(elementName);
                foreach (var item in tupel(obj))
                {
                    if (xmlAttriue != null)
                        Serialize(writer, item, typeArgument, xmlAttriue.Name);
                    else
                        Serialize(writer, item, typeArgument, typeArgument.Name);
                }
                writer.WriteEndElement();
            }
            else if (type.IsArray)
            {
                var typeArgument = type.GetElementType();
                var xmlAttriue = typeArgument.GetTypeInfo().GetCustomAttribute<XmlClassAttribute>();

                writer.WriteStartElement(elementName);
                foreach (var item in obj as System.Collections.IEnumerable)
                {
                    if (xmlAttriue != null)
                        Serialize(writer, item, typeArgument, xmlAttriue.Name);
                    else
                        Serialize(writer, item, typeArgument, typeArgument.Name);
                }
                writer.WriteEndElement();
            }

            else if (type.GetTypeInfo().IsEnum)
            {
                writer.WriteElementString(elementName, obj.ToString());
            }
            else
            {
                var propertiesToCheck = GetProperties(type);
                writer.WriteStartElement(elementName);
                // Sortiern, sodass properties die als attribut geschrieben werden zuerst aufgerufen werden
                propertiesToCheck = propertiesToCheck.OrderBy(x => x.GetCustomAttribute<XmlAttributeAttribute>() == null);

                bool valueFound = false;
                foreach (var property in propertiesToCheck)
                {
                    if (property.GetIndexParameters().Length > 0)
                        continue; // ignore indexer;
                    var isIgnore = property.GetCustomAttribute<XmlIgnoreAttribute>() != null;
                    var element = property.GetCustomAttribute<XmlElementAttribute>();
                    var attr = property.GetCustomAttribute<XmlAttributeAttribute>();
                    var valueAttribute = property.GetCustomAttribute<XmlValueAttribute>() != null;
                    if (isIgnore)
                        continue;
                    if (valueAttribute && valueFound)
                        throw new ArgumentException("Only one Property on Class may be Value");

                    var getter = property.GetMethod;
                    if (getter == null)
                    {
                        var objProp = obj.GetType().GetRuntimeProperty(property.Name);
                        if (objProp != null)
                            getter = objProp.GetMethod;
                    }

                    if (getter == null)
                        throw new ArgumentException("property doesent have get, cant read Value. Consider Using XmlIgnore");
                    valueFound = valueAttribute;
                    if (attr != null)
                    {
                        var data = writerLookup[property.PropertyType](getter.Invoke(obj, new Object[0]));
                        writer.WriteAttributeString(attr.Name, data);
                    }
                    else if (element != null)
                    {
                        Serialize(writer, property.GetValue(obj), property.PropertyType, element.Name, valueAttribute);
                    }
                    else
                    {
                        Serialize(writer, property.GetValue(obj), property.PropertyType, property.Name, valueAttribute);
                    }
                }

                writer.WriteEndElement();
            }
        }

        private IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            if (!type.GetTypeInfo().IsInterface)
                foreach (var p in type.GetRuntimeProperties().Where(x => !IsStatic(x)))
                    yield return p;
            else
            {
                // Wenn es ein Interface ist, so müssen manuell die Properties der Interfaces die dieses "implementiert" gesucht werden.
                var interfaces = type.GetTypeInfo().ImplementedInterfaces;
                foreach (var i in interfaces.Concat(type))
                    foreach (var p in i.GetRuntimeProperties())
                        yield return p;
            }
        }

        private bool IsStatic(PropertyInfo arg)
        {
            if (arg.CanRead)
                return arg.GetMethod.IsStatic;
            else if (arg.CanWrite)
                return arg.SetMethod.IsStatic;
            throw new ArgumentException("This Property has no Writre nor set Method");
        }
    }
}