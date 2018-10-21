using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PokerServer.Core
{
    static class ConvertExtensions
    {
        public static T ToObject<T>(this byte[] data) where T: struct
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        }

        public static byte[] ToBytes<T>(this T obj)
        {
            int size = Marshal.SizeOf(obj);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(obj, ptr, false);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }
    }
}
