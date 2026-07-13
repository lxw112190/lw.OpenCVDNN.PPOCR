using System;
using System.Runtime.InteropServices;
using System.Text;

namespace LwPPOCRNet35
{
    internal sealed class PPOCRNative : IDisposable
    {
        private const string DllName = "lw.OpenCVDNN.PPOCR.dll";
        private IntPtr handle;

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern int ppocr_create_w(
            string detModel,
            string recModel,
            string dictionary,
            int cpuThreads,
            out IntPtr engine,
            StringBuilder error,
            int errorCapacity);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ppocr_ocr_encoded(
            IntPtr engine,
            IntPtr imageBytes,
            int imageSize,
            out IntPtr utf8Json,
            out int jsonSize,
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder error,
            int errorCapacity);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void ppocr_free(IntPtr memory);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void ppocr_destroy(IntPtr engine);

        public PPOCRNative(string detModel, string recModel, string dictionary, int cpuThreads)
        {
            StringBuilder error = new StringBuilder(512);
            int code = ppocr_create_w(detModel, recModel, dictionary, cpuThreads, out handle, error, error.Capacity);
            if (code != 0)
            {
                throw new InvalidOperationException("OCR initialization failed (" + code + "): " + error);
            }
        }

        public string Recognize(byte[] encodedImage)
        {
            if (encodedImage == null || encodedImage.Length == 0)
            {
                throw new ArgumentException("Image data is empty", "encodedImage");
            }

            GCHandle pin = GCHandle.Alloc(encodedImage, GCHandleType.Pinned);
            IntPtr result = IntPtr.Zero;
            try
            {
                int resultSize;
                StringBuilder error = new StringBuilder(512);
                int code = ppocr_ocr_encoded(
                    handle,
                    pin.AddrOfPinnedObject(),
                    encodedImage.Length,
                    out result,
                    out resultSize,
                    error,
                    error.Capacity);
                if (code != 0)
                {
                    throw new InvalidOperationException("OCR failed (" + code + "): " + error);
                }

                byte[] utf8 = new byte[resultSize];
                Marshal.Copy(result, utf8, 0, resultSize);
                return Encoding.UTF8.GetString(utf8);
            }
            finally
            {
                if (result != IntPtr.Zero) ppocr_free(result);
                pin.Free();
            }
        }

        public void Dispose()
        {
            if (handle != IntPtr.Zero)
            {
                ppocr_destroy(handle);
                handle = IntPtr.Zero;
            }
            GC.SuppressFinalize(this);
        }

        ~PPOCRNative()
        {
            Dispose();
        }
    }
}

