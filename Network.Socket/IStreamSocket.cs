using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Socket
{
    public interface ITcpSocket
    {
        /// <summary>
        /// Liefert das unterliegende Soketobjekt zurück.
        /// </summary>
        Object OriginalSocket { get; }

        /// <summary>
        /// Der Port an den Der Socket gebunden ist. 0 falls noch nicht gebunden.
        /// </summary>
        UInt16 LocalPort { get; }

        UInt16 RemotePort { get; }

        String RemoteHost { get; }

        Task Send(byte[] data);

        Task Connect(string remoteHost, int port);

        void Close();

        /// <summary>
        /// Wird gefeuert sobald ein Paket empfangen wird.
        /// </summary>
        event MessageRecivedEvent MessageRecived;
    }

    public interface ITcpSocketListener
    {
        /// <summary>
        /// Liefert das unterliegende Soketobjekt zurück.
        /// </summary>
        Object OriginalSocket { get; }

        /// <summary>
        /// Der Port an den Der Socket gebunden ist. 0 falls noch nicht gebunden.
        /// </summary>
        UInt16 Port { get; }

        /// <summary>
        /// Bindet den Socket an einen UDP Port.
        /// </summary>
        /// <param name="localPort">Die Portnummer</param>
        /// <returns></returns>
        Task Bind(int localPort = 0);

        /// <summary>
        /// Wird gefeuert sobald ein Paket empfangen wird.
        /// </summary>
        event TcPSocetRecivedEvent SocketRecived;
    }

    public delegate void TcPSocetRecivedEvent(object sender, TcPSocetRecivedArgs args);

    public class TcPSocetRecivedArgs
    {
        public ITcpSocket Socket { get; set; }
    }
}