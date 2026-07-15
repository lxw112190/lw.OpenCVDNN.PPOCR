using System.Collections.Generic;

namespace LwOpenCVDnnPPOCRWin7Test
{
    public sealed class OcrResponse
    {
        public double elapsed_ms { get; set; }
        public List<OcrItem> results { get; set; }
    }

    public sealed class OcrItem
    {
        public int index { get; set; }
        public int id { get; set; }
        public int code { get; set; }
        public string msg { get; set; }
        public string text { get; set; }
        public float score { get; set; }
        public int x1 { get; set; }
        public int y1 { get; set; }
        public int x2 { get; set; }
        public int y2 { get; set; }
        public int x3 { get; set; }
        public int y3 { get; set; }
        public int x4 { get; set; }
        public int y4 { get; set; }
        public int rotation { get; set; }
    }
}
