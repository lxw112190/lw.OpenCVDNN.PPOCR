using System;
using System.Runtime.InteropServices;
using System.Text;

namespace LwOpenCVDnnPPOCRWin7Test
{
    // The field order and types must stay identical to ppocr_config_w in ppocr_api.h.
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct PPOCRConfig
    {
        public int struct_size;
        [MarshalAs(UnmanagedType.LPWStr)] public string det_model_path;
        [MarshalAs(UnmanagedType.LPWStr)] public string rec_model_path;
        [MarshalAs(UnmanagedType.LPWStr)] public string rec_dict_path;
        [MarshalAs(UnmanagedType.LPWStr)] public string cls_model_path;
        public int limit_side_len;
        public double det_db_thresh;
        public double det_db_box_thresh;
        public double det_db_unclip_ratio;
        public int use_dilation;
        public int use_angle_cls;
        public double cls_thresh;
        public int cls_batch_num;
        public int rec_batch_num;
        public int rec_img_h;
        public int rec_img_w;
        public int rec_predictor_num;

        // 0 lets OpenCV choose its default CPU thread count; a positive value sets it explicitly.
        public int cpu_threads;
    }

    /// <summary>
    /// Owns one native OCR engine. The class serializes recognition and disposal so the
    /// unmanaged handle cannot be destroyed while a native call is still using it.
    /// </summary>
    internal sealed class NativeOcr : IDisposable
    {
        private const string DllName = "lw.OpenCVDNN.PPOCR.dll";
        private readonly object syncRoot = new object();
        private IntPtr handle;
        private bool disposed;

        // All exports use the C ABI and __cdecl. Model paths and errors are UTF-16.
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern int ppocr_create_ex_w(ref PPOCRConfig config, out IntPtr engine,
            StringBuilder error, int errorCapacity);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern int ppocr_ocr_encoded(IntPtr engine, IntPtr imageBytes, int imageSize,
            out IntPtr utf8Json, out int jsonSize,
            StringBuilder error, int errorCapacity);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void ppocr_free(IntPtr memory);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void ppocr_destroy(IntPtr engine);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr ppocr_get_version();

        public static string Version
        {
            get { return Marshal.PtrToStringAnsi(ppocr_get_version()); }
        }

        public NativeOcr(PPOCRConfig config)
        {
            // struct_size lets the native DLL reject incompatible caller layouts safely.
            config.struct_size = Marshal.SizeOf(typeof(PPOCRConfig));
            StringBuilder error = new StringBuilder(512);
            int code = ppocr_create_ex_w(ref config, out handle, error, error.Capacity);
            if (code != 0)
                throw new InvalidOperationException("OCR初始化失败(" + code + "): " + error);
        }

        public string Recognize(byte[] encodedImage)
        {
            if (encodedImage == null || encodedImage.Length == 0)
                throw new ArgumentException("图片数据为空", "encodedImage");

            lock (syncRoot)
            {
                ThrowIfDisposed();

                // Pin only for the duration of the native call; the DLL does not retain this pointer.
                GCHandle pin = GCHandle.Alloc(encodedImage, GCHandleType.Pinned);
                IntPtr result = IntPtr.Zero;
                try
                {
                    int resultSize;
                    StringBuilder error = new StringBuilder(512);
                    int code = ppocr_ocr_encoded(handle, pin.AddrOfPinnedObject(), encodedImage.Length,
                        out result, out resultSize, error, error.Capacity);
                    if (code != 0)
                        throw new InvalidOperationException("OCR识别失败(" + code + "): " + error);
                    if (result == IntPtr.Zero || resultSize < 0)
                        throw new InvalidOperationException("OCR返回了无效结果");

                    byte[] utf8 = new byte[resultSize];
                    if (resultSize > 0) Marshal.Copy(result, utf8, 0, resultSize);
                    return Encoding.UTF8.GetString(utf8);
                }
                finally
                {
                    // JSON is allocated with CoTaskMemAlloc by the DLL and must use its paired free API.
                    if (result != IntPtr.Zero) ppocr_free(result);
                    pin.Free();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            lock (syncRoot)
            {
                if (disposed) return;
                if (handle != IntPtr.Zero)
                {
                    ppocr_destroy(handle);
                    handle = IntPtr.Zero;
                }
                disposed = true;
            }
        }

        private void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException("NativeOcr");
        }

        ~NativeOcr()
        {
            Dispose(false);
        }
    }
}
