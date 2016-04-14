using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace Misc.UPnP.Nat
{
    public static class UPnPNatTraversal
    {
        private static string _eventUrl;
        private static Task<string> _serviceUrl;
        private static TaskCompletionSource<string> taskComplet;

        static UPnPNatTraversal()
        {
            taskComplet = new System.Threading.Tasks.TaskCompletionSource<string>();
            _serviceUrl = taskComplet.Task;


        }

        public static async Task SearchDevice()
        {
            var listener = new Network.Socket.MessageRecivedEvent(async (sender, e) =>
         {
             var array = e.Data;
             var data = System.Text.UTF8Encoding.UTF8.GetString(array, 0, array.Length);
             var headers = ExtactHeaders(data).ToArray();
             taskComplet.SetResult(await GetServiceUrl(headers.First(x => x.Item1.Equals("Location", StringComparison.OrdinalIgnoreCase)).Item2));
         });
            var d = Network.Socket.SocketFactory.CreateDatagramsocket();
            try
            {
                d.MessageRecived += listener;
                await d.Bind();
                await d.JounMulticastGroup("239.255.255.250");

                var msg = "M-SEARCH * HTTP/1.1\r\n" +
                            "HOST: 239.255.255.250:1900\r\n" +
                            "ST:upnp:rootdevice\r\n" +
                            "MAN:\"ssdp:discover\"\r\n" +
                            "MX:3\r\n\r\n";
                var msgdata = System.Text.UTF8Encoding.UTF8.GetBytes(msg);
                await d.Send(msgdata, "239.255.255.250", 1900);
                await Task.WhenAny(_serviceUrl, Task.Delay(5000)).ContinueWith(x =>
                {
                    if (x.Result != _serviceUrl)
                        throw new TimeoutException("No Response from Gateway");
                });
            }
            finally
            {
                d.MessageRecived -= listener;
            }
        }

        private static IEnumerable<Tuple<string, string>> ExtactHeaders(string data)
        {
            var reg = new Regex("^(?<header>.*?):(?<content>.*)$", RegexOptions.Multiline);
            var matches = reg.Matches(data).Cast<Match>();
            return matches.Select(x => Tuple.Create(x.Groups["header"].Value, x.Groups["content"].Value));
        }

        private static async Task<string> GetServiceUrl(string resp)
        {
#if !DEBUG
            try
            {
#endif

            var req = WebRequest.Create(resp);
            var respp = await req.GetResponseAsync();
            var stream = respp.GetResponseStream();
            var xml = await SteamToText(stream);

            var desc = new System.Xml.XmlDocument();
            desc.LoadXml(xml);
            //var desc = await Misc.Xml.XmlFactory.LoadDocument(xml);

            var nsMgr = new System.Xml.XmlNamespaceManager(desc.NameTable);
            nsMgr.AddNamespace("tns", "urn:schemas-upnp-org:device-1-0");


            var typen = desc.SelectSingleNode("//tns:device/tns:deviceType/text()", nsMgr);
            if (!typen.Value.Contains("InternetGatewayDevice"))
                return null;
            var node = desc.SelectSingleNode("//tns:service[tns:serviceType=\"urn:schemas-upnp-org:service:WANIPConnection:1\"]/tns:controlURL/text()", nsMgr);
            if (node == null)
                return null;
            var eventnode = desc.SelectSingleNode("//tns:service[tns:serviceType=\"urn:schemas-upnp-org:service:WANIPConnection:1\"]/tns:eventSubURL/text()", nsMgr);
            _eventUrl = CombineUrls(resp, eventnode.Value);
            return CombineUrls(resp, node.Value);
#if !DEBUG
            }
            catch { return null; }
#endif
        }

        private static string CombineUrls(string resp, string p)
        {
            int n = resp.IndexOf("://");
            n = resp.IndexOf('/', n + 3);
            return resp.Substring(0, n) + p;
        }

        public static async Task ForwardPort(int externPort, int internPort, string internIp, ProtocolType protocol, string description = "")
        {
            if (internIp == null)
            {
                //TODO: Finde die Richtige eigene IP Adresse.
            }
            if (string.IsNullOrEmpty(await _serviceUrl))
                throw new Exception("No UPnP service available or Discover() has not been called");
            await SOAPRequest(await _serviceUrl, "<u:AddPortMapping xmlns:u=\"urn:schemas-upnp-org:service:WANIPConnection:1\">" +
               "<NewRemoteHost></NewRemoteHost><NewExternalPort>" + externPort.ToString() + "</NewExternalPort><NewProtocol>" + protocol.ToString().ToUpper() + "</NewProtocol>" +
               "<NewInternalPort>" + internPort.ToString() + "</NewInternalPort><NewInternalClient>" + internIp +
               "</NewInternalClient><NewEnabled>1</NewEnabled><NewPortMappingDescription>" + description +
           "</NewPortMappingDescription><NewLeaseDuration>0</NewLeaseDuration></u:AddPortMapping>", "AddPortMapping");
        }

        public static async Task DeleteForwardingRule(int externalPort, ProtocolType protocol)
        {
            if (string.IsNullOrEmpty(await _serviceUrl))
                throw new Exception("No UPnP service available or Discover() has not been called");
            await SOAPRequest(await _serviceUrl,
           "<u:DeletePortMapping xmlns:u=\"urn:schemas-upnp-org:service:WANIPConnection:1\">" +
           "<NewRemoteHost>" +
           "</NewRemoteHost>" +
           "<NewExternalPort>" + externalPort + "</NewExternalPort>" +
           "<NewProtocol>" + protocol.ToString().ToUpper() + "</NewProtocol>" +
           "</u:DeletePortMapping>", "DeletePortMapping");
        }

        public static async Task<string> GetExternalIP()
        {
            if (string.IsNullOrEmpty(await _serviceUrl))
                throw new Exception("No UPnP service available or Discover() has not been called");
            var xdoc = await SOAPRequest(await _serviceUrl, "<u:GetExternalIPAddress xmlns:u=\"urn:schemas-upnp-org:service:WANIPConnection:1\">" +
            "</u:GetExternalIPAddress>", "GetExternalIPAddress");

            var nsMgr = new System.Xml.XmlNamespaceManager(xdoc.NameTable);
            nsMgr.AddNamespace("tns", "urn:schemas-upnp-org:device-1-0");


            string IP = xdoc.SelectSingleNode("//NewExternalIPAddress/text()", nsMgr).Value;
            return IP;
        }

        private static async Task<XmlDocument> SOAPRequest(string url, string soap, string function)
        {
            string req = "<?xml version=\"1.0\"?>" +
            "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">" +
            "<s:Body>" +
            soap +
            "</s:Body>" +
            "</s:Envelope>";
            WebRequest r = HttpWebRequest.Create(url);
            r.Method = "POST";
            byte[] b = Encoding.UTF8.GetBytes(req);
            r.Headers["SOAPACTION"] = "\"urn:schemas-upnp-org:service:WANIPConnection:1#" + function + "\"";
            r.ContentType = "text/xml; charset=\"utf-8\"";
            //r.ContentLength = b.Length;
            (await r.GetRequestStreamAsync()).Write(b, 0, b.Length);
            WebResponse wres = await r.GetResponseAsync();
            var ress = wres.GetResponseStream();
            var text = await SteamToText(ress);

            var resp = new XmlDocument();
            resp.LoadXml(text);
            return resp;
        }

        private static async Task<string> SteamToText(System.IO.Stream ress)
        {
            using (var textstream = new System.IO.StreamReader(ress))
            {
                var text = await textstream.ReadToEndAsync();
                return text;
            }
        }
    }

    /// <summary>
    ///  Gibt die Protokolle an, die von der System.Net.Sockets.Socket-Klasse unterstützt werden
    /// </summary>
    public enum ProtocolType
    {
        /// <summary>
        /// Transmission Control Protocol.
        /// </summary>
        Tcp = 6,

        /// <summary>
        ///    User Datagram-Protokoll.
        /// </summary>
        Udp = 17,
    }
}