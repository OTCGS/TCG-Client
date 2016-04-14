using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPnPConsoleTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                DoWork().Wait();
            }
            catch (Exception e)
            {
                Log(e.ToString());
                Log(e.Message);
            }
            PressAnyKey();
        }

        private static async Task DoWork()
        {
            Log("Search for UPnP Nat");
            try
            {
                await Misc.UPnP.Nat.UPnPNatTraversal.SearchDevice();
            }
            catch (TimeoutException)
            {
                Log("No UPnP Nat found");
                return;
            }
            Log("UPnP Nat found");

            var ip = await Misc.UPnP.Nat.UPnPNatTraversal.GetExternalIP();
            Log("Your externl ip is {0}", ip);

            Log("Test Forwarding");
            Log("Forward UDP Port 49498 (extern) to 49499 (local)");

            var interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces().SelectMany(x => x.GetIPProperties().UnicastAddresses).Select(x => x.Address.ToString()).ToArray();
            string ownIp;
            Log("Please select your inteface to which you want to forward");

            for (int i = 0; i < interfaces.Length; i++)
            {
                Log("{0:D" + interfaces.Length.ToString().Length + "}:\t{1}", i + 1, interfaces[i]);
            }
            Log("Please Enter The Number");
            if (interfaces.Length > 9)
            {
                int? value = null;
                do
                {
                    try
                    {
                        value = int.Parse(Console.ReadLine());
                        if (value > interfaces.Length || value <= 0)
                            value = null;
                    }
                    catch (Exception)
                    {
                        Log("Please enter a valid Number");
                    }
                } while (value == null);
                ownIp = interfaces[value.Value - 1];
            }
            else
            {
                int? value = null;
                do
                {
                    try
                    {
                        value = int.Parse(Console.ReadKey(true).KeyChar.ToString());
                        if (value > interfaces.Length || value <= 0)
                            value = null;
                    }
                    catch (Exception)
                    {
                        Log("Please enter a valid Number");
                    }
                } while (value == null);
                ownIp = interfaces[value.Value - 1];
            }

            await Misc.UPnP.Nat.UPnPNatTraversal.ForwardPort(49498, 49499, ownIp, Misc.UPnP.Nat.ProtocolType.Udp, "Testforwarding");
            Log("Forwarded. Please Check Forwarding in your router settings");
            PressAnyKey();
            Log("Delete Forwarding Rule...");

            await Misc.UPnP.Nat.UPnPNatTraversal.DeleteForwardingRule(49498, Misc.UPnP.Nat.ProtocolType.Udp);
            Log("Deleted");
            Log("Test Finished");
        }

        private static void PressAnyKey()
        {
            Console.WriteLine("Press Any Key...");
            Console.ReadKey(false);
        }

        private static void Log(string p, params object[] par)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("{0}\t", DateTime.Now);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(p, par);
        }
    }
}