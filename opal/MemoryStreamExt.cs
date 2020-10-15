using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Opal
{
    public static class MemoryStreamExt
    {
        public static uint ToMemoryBlock(this MemoryStream memoryStream,
            IntPtr[] block)
        {
            if (memoryStream == null) throw new ArgumentNullException();
            var array = memoryStream.ToArray();
            var buffer = Marshal.AllocCoTaskMem(array.Length);
            Marshal.Copy(memoryStream.ToArray(), 0, buffer, array.Length);
            block[0] = buffer;
            return (uint) array.Length;
        }
    }
}
