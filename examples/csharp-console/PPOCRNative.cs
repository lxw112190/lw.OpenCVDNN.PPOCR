using System;
using System.Runtime.InteropServices;
using System.Text;

namespace LwPPOCRNet35
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct PPOCRRoi
    {
        public int struct_size;
        public int id;
        public int x1;
        public int y1;
        public int x2;
        public int y2;
        public int x3;
        public int y3;
        public int x4;
        public int y4;
        public int rotation;

        public static PPOCRRoi Rectangle(int id, int x, int y, int width, int height, int rotation)
        {
            if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException("width");
            PPOCRRoi roi = new PPOCRRoi();
            roi.struct_size = Marshal.SizeOf(typeof(PPOCRRoi));
            roi.id = id;
            roi.x1 = roi.x4 = x;
            roi.x2 = roi.x3 = checked(x + width - 1);
            roi.y1 = roi.y2 = y;
            roi.y3 = roi.y4 = checked(y + height - 1);
            roi.rotation = rotation;
            return roi;
        }
    }

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
        private static extern int ppocr_recognize_encoded(
            IntPtr engine, IntPtr imageBytes, int imageSize,
            out IntPtr utf8Json, out int jsonSize,
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder error, int errorCapacity);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ppocr_recognize_rois_encoded(
            IntPtr engine, IntPtr imageBytes, int imageSize,
            [In] PPOCRRoi[] rois, int roiCount,
            out IntPtr utf8Json, out int jsonSize,
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder error, int errorCapacity);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ppocr_recognize_encoded_batch(
            IntPtr engine, [In] IntPtr[] imageBytes, [In] int[] imageSizes, int imageCount,
            out IntPtr utf8Json, out int jsonSize,
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder error, int errorCapacity);

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

        public PPOCRNative(string recModel, string dictionary, int cpuThreads)
            : this(null, recModel, dictionary, cpuThreads)
        {
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

        public string RecognizeTextLine(byte[] encodedImage)
        {
            return CallRecognition(encodedImage, null);
        }

        public string RecognizeRegions(byte[] encodedImage, PPOCRRoi[] rois)
        {
            if (rois == null || rois.Length == 0)
                throw new ArgumentException("ROIs are empty", "rois");
            int size = Marshal.SizeOf(typeof(PPOCRRoi));
            for (int i = 0; i < rois.Length; i++) rois[i].struct_size = size;
            return CallRecognition(encodedImage, rois);
        }

        public string RecognizeTextLines(byte[][] encodedImages)
        {
            if (encodedImages == null || encodedImages.Length == 0)
                throw new ArgumentException("Images are empty", "encodedImages");
            GCHandle[] pins = new GCHandle[encodedImages.Length];
            IntPtr[] pointers = new IntPtr[encodedImages.Length];
            int[] sizes = new int[encodedImages.Length];
            IntPtr result = IntPtr.Zero;
            try
            {
                for (int i = 0; i < encodedImages.Length; i++)
                {
                    if (encodedImages[i] == null || encodedImages[i].Length == 0)
                        throw new ArgumentException("Batch contains an empty image", "encodedImages");
                    pins[i] = GCHandle.Alloc(encodedImages[i], GCHandleType.Pinned);
                    pointers[i] = pins[i].AddrOfPinnedObject();
                    sizes[i] = encodedImages[i].Length;
                }
                int resultSize;
                StringBuilder error = new StringBuilder(512);
                int code = ppocr_recognize_encoded_batch(handle, pointers, sizes, pointers.Length,
                    out result, out resultSize, error, error.Capacity);
                if (code != 0)
                    throw new InvalidOperationException("Batch recognition failed (" + code + "): " + error);
                byte[] utf8 = new byte[resultSize];
                if (resultSize > 0) Marshal.Copy(result, utf8, 0, resultSize);
                return Encoding.UTF8.GetString(utf8);
            }
            finally
            {
                if (result != IntPtr.Zero) ppocr_free(result);
                for (int i = 0; i < pins.Length; i++) if (pins[i].IsAllocated) pins[i].Free();
            }
        }

        private string CallRecognition(byte[] encodedImage, PPOCRRoi[] rois)
        {
            if (encodedImage == null || encodedImage.Length == 0)
                throw new ArgumentException("Image data is empty", "encodedImage");

            GCHandle pin = GCHandle.Alloc(encodedImage, GCHandleType.Pinned);
            IntPtr result = IntPtr.Zero;
            try
            {
                int resultSize;
                StringBuilder error = new StringBuilder(512);
                int code = rois == null
                    ? ppocr_recognize_encoded(handle, pin.AddrOfPinnedObject(), encodedImage.Length,
                        out result, out resultSize, error, error.Capacity)
                    : ppocr_recognize_rois_encoded(handle, pin.AddrOfPinnedObject(), encodedImage.Length,
                        rois, rois.Length, out result, out resultSize, error, error.Capacity);
                if (code != 0)
                    throw new InvalidOperationException("Recognition failed (" + code + "): " + error);
                byte[] utf8 = new byte[resultSize];
                if (resultSize > 0) Marshal.Copy(result, utf8, 0, resultSize);
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
