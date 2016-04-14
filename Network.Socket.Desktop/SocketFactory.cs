using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Socket
{
    public static partial class SocketFactory
    {
        private static readonly Desktop.DesktopSocketFactory fac = new Desktop.DesktopSocketFactory();
    }
}