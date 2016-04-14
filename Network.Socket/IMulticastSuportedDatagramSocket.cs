using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Socket
{
    public interface IMulticastSuportedDatagramSocket :IDatagramSocket
    {
        Task JounMulticastGroup(string ip);

    }
}
