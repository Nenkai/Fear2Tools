using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fear2Tools
{
    public static class Utils
    {
        public static string ReadNullTerminatedString(byte[] buffer, int offset)
        {
            int end = offset;
            while (end < buffer.Length && buffer[end] != 0)
            {
                end++;
            }

            unsafe
            {
                fixed (byte* pAscii = buffer)
                {
                    return new string((sbyte*)pAscii, offset, end - offset);
                }
            }
        }

        public static string ReadNullTerminatedWString(byte[] buffer, int offset)
        {
            int end = offset;
            while (end < buffer.Length && buffer[end] != 0)
            {
                end++;
            }

            unsafe
            {
                fixed (byte* pAscii = buffer)
                {
                    return new string((sbyte*)pAscii, offset, end - offset, Encoding.Unicode);
                }
            }
        }
    }
}
