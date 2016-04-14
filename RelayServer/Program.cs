using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace RelayServer
{
    internal static class Program
    {
        public const int TIMEOUT = 60;

        public const uint LIST_PORT = Network.Relay.Protocol.PORT;

        /// <summary>
        /// Dauer in Minuten, in denen alte Clients entfernt werden sollen
        /// </summary>
        public const int CHECK_OLD_STUFF = 2;

        private static DateTime lastCheck = DateTime.Now;

        private static readonly Dictionary<Tuple<string, uint>, UInt32> IdLookup = new Dictionary<Tuple<string, uint>, uint>();
        private static readonly Dictionary<UInt32, Client> ClientLookup = new Dictionary<uint, Client>();
        private static readonly System.Security.Cryptography.RandomNumberGenerator r = System.Security.Cryptography.RandomNumberGenerator.Create();
        [STAThread]
        private static void Main(string[] args)
        {
            //WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            //bool hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);

            //if (hasAdministrativeRight)
            //{
            //}
            //else
            //{
            //    ProcessStartInfo processInfo = new ProcessStartInfo();
            //    processInfo.Verb = "runas";
            //    processInfo.FileName = Environment.CommandLine.Trim('"', ' '); // Application.ExecutablePath;
            //    try
            //    {
            //        Process.Start(processInfo);
            //    }
            //    catch (Win32Exception ex)
            //    {
            //        System.Environment.Exit(-1);
            //    }

            //    System.Environment.Exit(0);
            //}
            

             Console.WriteLine("Starte MessageLoop");
            var clientLoop = ClientMessageLoop();
             Console.WriteLine("Starte HTTP Listener");
            var listLoop = ListingMode();

            Console.WriteLine("Warte...");
            Task.WaitAll(clientLoop);

        }

        private static async Task ListingMode()
        {
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }
            // URI prefixes are required,
            // for example "http://contoso.com:8080/index/".

            // Create a listener.
            HttpListener listener = new HttpListener();
            // Add the prefixes.
            listener.Prefixes.Add("http://*:" + LIST_PORT + "/");
            try
            {
                listener.Start();
            }
            catch (Exception r)
            {
                Console.WriteLine(r);
                throw;
            }

            while (true)
            {
                byte[] buffer = new byte[0];
                HttpListenerResponse response = null;
                try
                {
                    var context = await listener.GetContextAsync();
                    var request = context.Request;

                    Console.WriteLine("Listening...");
                    // Obtain a response object.
                    response = context.Response;
                    // Construct a response.

                    var clients = ClientLookup.Values.Select(x => new System.Xml.Linq.XElement("Client",
                        new System.Xml.Linq.XElement("Id", x.ID),
                         System.Xml.Linq.XElement.Parse(Encoding.UTF8.GetString(x.Data))
                    )); //"<HTML><BODY> Hello world!</BODY></HTML>";

                    var responseXML = new System.Xml.Linq.XElement("Clients", clients);
                    string responseString = responseXML.ToString(System.Xml.Linq.SaveOptions.None);
                    response.ContentType = "text/xml";
                    buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                    // Get a response stream and write the response to it.

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);

                }
                finally
                {

                    try
                    {
                        response.ContentLength64 = buffer.Length;
                        using (System.IO.Stream output = response.OutputStream)
                            output.Write(buffer, 0, buffer.Length);
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine(ex);

                    }
                }
            }
        }

        private static async Task ClientMessageLoop()
        {
            var channel = new System.Collections.Concurrent.AwaitableConcurrentQueue<Network.Socket.MessageRecivedArgs>();

            using (var soc = Network.Socket.SocketFactory.CreateReliableDatagramsocket())
            {

                await soc.Bind(Network.Relay.Protocol.PORT);
                soc.MessageRecived += (xx, ev) => channel.Enqueue(ev);

                while (true)
                {
                    try
                    {

                        while (true)
                        {
                            var ev = await channel.DeQueue();

                            var host = ev.Host;
                            var port = ev.Port;
                            var data = ev.Data;
                            var sender = Tuple.Create(host, port);
                            var message = Network.Relay.Messages.Message.CreateMessageFromData(data);
                            switch (message.Type)
                            {
                                case Network.Relay.MessageType.Keepalive:
                                    {
                                        if (!IdLookup.ContainsKey(sender))
                                            break;
                                        var id = IdLookup[sender];
                                        var client = ClientLookup[id];
                                        client.Timeout = GenerateTimeout();
                                        Console.WriteLine("Recived KeepAlive from id:" + id);
                                    }
                                    break;

                                case Network.Relay.MessageType.Request:
                                    {
                                        var request = message as Network.Relay.Messages.Request;
                                        Console.WriteLine("Request from");
                                        Console.WriteLine(sender);

                                        var newId = GenerateID();
                                        IdLookup[sender] = newId;
                                        var client = new Client() { ID = newId, EndPoint = sender, Timeout = GenerateTimeout(), Data = request.Data };
                                        ClientLookup[newId] = client;
                                        var ips = await Dns.GetHostAddressesAsync(sender.Item1);
                                        var ip = ips.Any() ? ips.First().GetAddressBytes() : new byte[0];

                                        var response = new Network.Relay.Messages.Accept() { Address = ip, Id = newId, Timeout = TIMEOUT, Port = (UInt16)sender.Item2 };
                                        response.AddressFamily = response.Address.Length == 4 ? Network.AddressFamily.IPv4 : Network.AddressFamily.IPv6;
                                        var rawData = response.RawData;
                                        Console.WriteLine("Save under ID");
                                        Console.WriteLine(newId);

                                        await soc.Send(rawData, sender.Item1, sender.Item2);
                                    }
                                    break;

                                case Network.Relay.MessageType.Accept:
                                    {
                                        // Server sollte kein Accept bekommen Ignore
                                        Console.WriteLine("Server bekam Accept geschikt");
                                    }
                                    break;

                                case Network.Relay.MessageType.Send:
                                    {
                                        if (!IdLookup.ContainsKey(sender))
                                            break;
                                        var sourceId = IdLookup[sender];
                                        var sourceClient = ClientLookup[sourceId];
                                        sourceClient.Timeout = GenerateTimeout();
                                        var send = message as Network.Relay.Messages.Send;

                                        if (!ClientLookup.ContainsKey(send.TargetId))
                                            break;
                                        var targtClient = ClientLookup[send.TargetId];
                                        var targetIp = targtClient.EndPoint;

                                        var relay = new Network.Relay.Messages.Relay() { Data = send.Data, SourceId = sourceId };
                                        var rawData = relay.RawData;
                                        await soc.Send(rawData, targetIp.Item1, targetIp.Item2);
                                        Console.WriteLine("Recived send from id:\t" + sourceId + "\tto\t" + targtClient.ID);
                                    }
                                    break;

                                case Network.Relay.MessageType.Relay:
                                    {
                                        // Server sollte kein Accept bekommen Ignore
                                        Console.WriteLine("Server bekam Relay geschikt");
                                    }
                                    break;

                                default:
                                    break;
                            }

                            if (lastCheck.AddMinutes(CHECK_OLD_STUFF) < DateTime.Now)
                            {
                                var listToRemove = new List<UInt32>();
                                lastCheck = DateTime.Now;

                                var toRemove = ClientLookup.Values.Where(c => c.Timeout < DateTime.Now).ToArray();
                                foreach (var c in toRemove)
                                {
                                    IdLookup.Remove(c.EndPoint);
                                    ClientLookup.Remove(c.ID);
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("===============================================");
                        Console.WriteLine(ex.GetType().FullName);
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        Console.WriteLine("===============================================");
                    }
                }
            }
        }

        private static DateTime GenerateTimeout()
        {
            return DateTime.Now.AddSeconds(TIMEOUT * 3);// Erlaut das UDP Pakete verloren gehen können.
        }

        private static uint GenerateID()
        {
            for (int i = 0; i < 100; i++)
            {
                var b = new byte[4];
                r.GetBytes(b);
                var id = BitConverter.ToUInt32(b, 0);
                if (!ClientLookup.ContainsKey(id))
                    return id;
            }
            throw new Exception("No Free Id Found after 100 tries");
        }
    }

    internal class Client
    {
        public UInt32 ID { get; set; }

        public byte[] Data { get; set; }

        public Tuple<string, uint> EndPoint { get; set; }

        public DateTime Timeout { get; set; }
    }
}