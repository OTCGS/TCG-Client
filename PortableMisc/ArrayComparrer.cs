using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Misc.Portable
{
    public class ArrayComparrer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[] x, byte[] y)
        {
            return x.SequenceEqual(y);
        }

        public int GetHashCode(byte[] bArray)
        {
            {
                //http://bretm.home.comcast.net/~bretm/hash/6.html
                unchecked
                {
                    const int p = 16777619;
                    int hash = (int)2166136261;

                    for (int i = 0; i < bArray.Length; i++)
                        hash = (hash ^ bArray[i]) * p;

                    hash += hash << 13;
                    hash ^= hash >> 7;
                    hash += hash << 3;
                    hash ^= hash >> 17;
                    hash += hash << 5;
                    return hash;
                }
            }
        }
    }
}