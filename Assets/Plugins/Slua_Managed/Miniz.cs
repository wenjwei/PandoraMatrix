using System;
using System.Runtime.InteropServices;

namespace com.tencent.pandora
{
    public class Miniz
    {
        private static byte[] _sharedBuffer = new byte[1024];
#if UNITY_IPHONE && !UNITY_EDITOR
	const string LUADLL = "__Internal";
	
#else
        const string LUADLL = "pandora";
#endif

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        static extern int PandoraNet_Compress(int rawDataLen, [MarshalAs(UnmanagedType.LPStr)] string buf, out IntPtr data);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        static extern int PandoraNet_UnCompress(int encodedCompressedDataLen, IntPtr buf, int bufsize, out IntPtr data);

        // Free memory alloced by pandora inner
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        static extern void PandoraNet_Free(IntPtr p);

        public static byte[] Compress(string rawData, out int length)
        {
            IntPtr data = IntPtr.Zero;
            int ret = PandoraNet_Compress(rawData.Length, rawData, out data);
            if (ret > 0)
            {
                length = ret;
                byte[] bs = GetSharedBuffer(length);
                Marshal.Copy(data, bs, 0, length);
                PandoraNet_Free(data);
                return bs;
            }
            else
            {
                length = 0;
                return null;
            }
        }

        private static byte[] GetSharedBuffer(int length)
        {
            if(_sharedBuffer.Length < length)
            {
                int len = _sharedBuffer.Length;
                while(len < length)
                {
                    len = len * 2;
                }
                _sharedBuffer = new byte[len];
                return _sharedBuffer;
            }
            return _sharedBuffer;
        }

        public static byte[] Compress(string rawData)
        {
            IntPtr data = IntPtr.Zero;
            int ret = PandoraNet_Compress(rawData.Length, rawData, out data);
            if (ret > 0)
            {
                byte[] bs = new byte[ret];
                Marshal.Copy(data, bs, 0, ret);
                PandoraNet_Free(data);
                return bs;
            }
            else
            {
                return null;
            }
        }

        public static string UnCompressToString(byte[] encodedCompressedData, int srclen)
        {
            int len;
            IntPtr data = uncompress(encodedCompressedData, srclen, out len);

            if (data != IntPtr.Zero)
            {
                string str = Marshal.PtrToStringAnsi(data);
                PandoraNet_Free(data);
                return str;
            }
            else
            {
                return null;
            }
        }

        static IntPtr uncompress(byte[] srcdata, int srclen, out int len)
        {
            IntPtr data = IntPtr.Zero;
            len = 0;
            var h = GCHandle.Alloc(srcdata, GCHandleType.Pinned);
            IntPtr p = h.AddrOfPinnedObject();

            int bufsize = 1024;
            const int MaxBufSize = 10 * 1024 * 1024;//解压后最大10M

            int ret;
            do
            {
                if (data != IntPtr.Zero)
                    PandoraNet_Free(data);
                data = IntPtr.Zero;
                ret = PandoraNet_UnCompress(srclen, p, bufsize, out data);
                bufsize += bufsize;

                if (bufsize > MaxBufSize)
                    break;
            } while (ret == -2);

            if (h.IsAllocated)
                h.Free();
            if (ret > 0)
            {
                len = ret;
                return data;
            }
            else
            {
                return IntPtr.Zero;
            }
        }

        public static byte[] UnCompressToBytes(byte[] encodedCompressedData, int srclen)
        {
            int len;
            IntPtr data = uncompress(encodedCompressedData, srclen, out len);

            if (data != IntPtr.Zero)
            {
                byte[] bs = new byte[len];
                Marshal.Copy(data, bs, 0, len);
                PandoraNet_Free(data);
                return bs;
            }
            else
            {
                return null;
            }
        }

    }
}