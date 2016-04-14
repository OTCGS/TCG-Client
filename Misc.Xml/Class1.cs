using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Misc.Xml
{
    public interface IXmlDocument
    {
        IXmlNode SelectSingleNode(string path, string ns = null);

        string GetXml();
    }

    public interface IXmlNode
    {
        string Value { get; }

        string GetXml();
    }

    public abstract class XmlFactory
    {
        private static readonly TaskCompletionSource<XmlFactory> taskComp = new TaskCompletionSource<XmlFactory>();

        protected static Task<XmlFactory> Instance
        {
            get
            {
                if (DesignMode.Enabled)
                    return taskComp.Task;
                return Task.WhenAny<XmlFactory>(
                    Task.Run<XmlFactory>(async () =>
                    {
                        await Task.Delay(10000);
                        throw new TimeoutException("XmlFactory Not Set");
                    }),
                    taskComp.Task).Unwrap();
            }
        }

        protected void SetFactory(XmlFactory factory)
        {
            taskComp.SetResult(factory);
        }

        protected abstract Task<IXmlDocument> PrivatLoadDocument(string xml);

        public static async Task<IXmlDocument> LoadDocument(string xml)
        {
            var fac = await Instance;
            return await fac.PrivatLoadDocument(xml);
        }
    }
}