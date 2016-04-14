using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class GuidExtensions
    {
        

        public static byte[] ToBigEndianBytes(this Guid netGuid)
        {
            byte[] bigEndian = new byte[16];
            byte[] littleEndian = netGuid.ToByteArray();
            for (int i = 8; i < 16; i++)
            {
                bigEndian[i] = littleEndian[i];
            }
            bigEndian[0] = littleEndian[3];
            bigEndian[1] = littleEndian[2];
            bigEndian[2] = littleEndian[1];
            bigEndian[3] = littleEndian[0];
            bigEndian[4] = littleEndian[5];
            bigEndian[5] = littleEndian[4];
            bigEndian[6] = littleEndian[7];
            bigEndian[7] = littleEndian[6];
            return bigEndian;
        }
    }
}
