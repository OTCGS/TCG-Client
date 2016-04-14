using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Socket
{
    public interface IDatagramSocket : IDisposable
    {
        /// <summary>
        /// Liefert das unterliegende Soketobjekt zurück.
        /// </summary>
        Object OriginalSocket { get; }

        /// <summary>
        /// Der Port an den Der Socket gebunden ist. 0 falls noch nicht gebunden.
        /// </summary>
        UInt32 Port { get; }

        /// <summary>
        /// Sendet die Daten an den remoteHost an den spezifizierten Port.
        /// </summary>
        /// <remarks>
        /// Die Methode Bind muss vorher aufgerufen werden.
        /// </remarks>
        /// <param name="data">Die zu verschickenden Daten</param>
        /// <param name="remoteHost">Der Host zu dem gesendet wird</param>
        /// <param name="port">Der Port an den Gesendet wird</param>
        /// <returns></returns>
        Task Send(byte[] data, string remoteHost, uint port);

        /// <summary>
        /// Bindet den Socket an einen UDP Port.
        /// </summary>
        /// <param name="localPort">Die Portnummer</param>
        /// <returns></returns>
        Task Bind(uint localPort = 0);


        /// <summary>
        /// Wird gefeuert sobald ein Paket empfangen wird.
        /// </summary>
        event MessageRecivedEvent MessageRecived;
    }

    public delegate void MessageRecivedEvent(object sender, MessageRecivedArgs args);

    public class MessageRecivedArgs
    {
        public byte[] Data { get; set; }

        public string Host { get; set; }

        public uint Port { get; set; }
    }
}