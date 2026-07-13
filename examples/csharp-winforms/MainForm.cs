using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace LwOpenCVDnnPPOCRWin7Test
{
    public partial class MainForm : Form
    {
        private NativeOcr engine;
        private string imagePath;

        private Bitmap originalImage;
        private Bitmap displayImage;

        private string summaryText = string.Empty;
        private string fullText = string.Empty;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
#if PPOCR_X86_LOW_MEMORY
            // The x86 process has a much smaller address space, so keep one recognizer network.
            txtBatch.Text = "4";
            txtPredictors.Text = "1";
            txtThreads.Text = "2";
#endif
            string defaultImage = Path.Combine(
                Path.Combine(FindSolutionRoot(), "test-images"),
                "3.jpg");

            if (File.Exists(defaultImage))
            {
                LoadImage(defaultImage);
            }

            AppendLine("lw.OpenCVDNN.PPOCR Win7 Test 1.0.0.1");
            AppendLine("作者: 天天代码码天天,QQ:819069052");
            AppendLine("进程架构: " + (IntPtr.Size == 4 ? "x86（低内存模式）" : "x64"));
            AppendLine("OpenCV 5 DNN仅使用CPU，请选择模型后点击初始化。");
            AppendLine(string.Empty);
        }

        private void btnInit_Click(object sender, EventArgs e)
        {
            SetBusy(true);

            try
            {
                ReleaseEngine();

                ModelFiles model = GetSelectedModel();

                ValidateFile(model.Detector);
                ValidateFile(model.Recognizer);
                ValidateFile(model.Dictionary);

                if (model.UseClassifier)
                {
                    ValidateFile(model.Classifier);
                }

                PPOCRConfig config = new PPOCRConfig();

                config.det_model_path = model.Detector;
                config.rec_model_path = model.Recognizer;
                config.rec_dict_path = model.Dictionary;

                config.cls_model_path = model.UseClassifier
                    ? model.Classifier
                    : null;

                config.limit_side_len = ParseInt(txtLimit.Text, 960);
                config.det_db_thresh = ParseDouble(txtDetThresh.Text, 0.3);
                config.det_db_box_thresh = ParseDouble(txtBoxThresh.Text, 0.6);
                config.det_db_unclip_ratio = ParseDouble(txtUnclip.Text, 1.5);

                config.use_dilation = chkDilation.Checked ? 1 : 0;
                config.use_angle_cls = model.UseClassifier ? 1 : 0;

                config.cls_thresh = 0.9;
                config.cls_batch_num = 1;

                config.rec_batch_num = ParseInt(txtBatch.Text, 8);
                config.rec_img_h = 48;
                config.rec_img_w = 320;
                config.rec_predictor_num = ParseInt(txtPredictors.Text, 1);
                config.cpu_threads = ParseInt(txtThreads.Text, 0);

                Stopwatch watch = Stopwatch.StartNew();
                engine = new NativeOcr(config);
                watch.Stop();

                output.Clear();

                AppendLine("初始化成功");
                AppendLine(new string('=', 70));

                AppendLine("DLL版本: " + NativeOcr.Version);
                AppendLine("初始化耗时: " + watch.Elapsed.TotalMilliseconds.ToString("F1") + " ms");
                AppendLine("模型: " + model.Name);

                AppendLine(string.Empty);
                AppendLine("模型文件");
                AppendLine(new string('-', 70));

                AppendLine("det: " + model.Detector);
                AppendLine("rec: " + model.Recognizer);
                AppendLine("dict: " + model.Dictionary);

                if (model.UseClassifier)
                {
                    AppendLine("cls: " + model.Classifier);
                    AppendLine("方向分类: 已启用");
                }
                else
                {
                    AppendLine("cls: 未加载");
                    AppendLine("方向分类: 未启用");
                }

                AppendLine(string.Empty);
                AppendLine("检测参数");
                AppendLine(new string('-', 70));

                AppendLine("limit_side_len: " + config.limit_side_len);
                AppendLine("det_db_thresh: " + config.det_db_thresh.ToString(CultureInfo.InvariantCulture));
                AppendLine("det_db_box_thresh: " + config.det_db_box_thresh.ToString(CultureInfo.InvariantCulture));
                AppendLine("det_db_unclip_ratio: " + config.det_db_unclip_ratio.ToString(CultureInfo.InvariantCulture));
                AppendLine("use_dilation: " + config.use_dilation);

                AppendLine(string.Empty);
                AppendLine("方向分类参数");
                AppendLine(new string('-', 70));

                AppendLine("use_angle_cls: " + config.use_angle_cls);
                AppendLine("cls_thresh: " + config.cls_thresh.ToString(CultureInfo.InvariantCulture));
                AppendLine("cls_batch_num: " + config.cls_batch_num);

                AppendLine(string.Empty);
                AppendLine("识别参数");
                AppendLine(new string('-', 70));

                AppendLine("rec_batch_num: " + config.rec_batch_num);
                AppendLine("rec_img_h: " + config.rec_img_h);
                AppendLine("rec_img_w: " + config.rec_img_w);
                AppendLine("rec_predictor_num: " + config.rec_predictor_num);
                AppendLine("cpu_threads: " + config.cpu_threads);

                AppendLine(new string('=', 70));

                statusLabel.Text =
                    "初始化成功 | " +
                    model.Name +
                    " | CLS " +
                    (model.UseClassifier ? "ON" : "OFF") +
                    " | DLL " +
                    NativeOcr.Version;
            }
            catch (Exception ex)
            {
                ReleaseEngine();

                output.Text = "初始化失败\r\n" + ex;
                statusLabel.Text = "初始化失败: " + ex.Message;

                MessageBox.Show(
                    this,
                    ex.Message,
                    "初始化失败",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void btnDestroy_Click(object sender, EventArgs e)
        {
            ReleaseEngine();

            AppendLine("模型已释放");
            statusLabel.Text = "模型已释放";
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter =
                    "图片|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff|" +
                    "所有文件|*.*";
                dialog.Title = "选择待识别图片";

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    LoadImage(dialog.FileName);
                }
            }
        }

        private void btnRecognize_Click(object sender, EventArgs e)
        {
            if (engine == null)
            {
                MessageBox.Show(
                    this,
                    "请先初始化模型。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
            {
                MessageBox.Show(
                    this,
                    "请先选择图片。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            SetBusy(true);

            try
            {
                byte[] bytes = File.ReadAllBytes(imagePath);

                // 预热一次，不计入测速结果
                engine.Recognize(bytes);

                int loops = Decimal.ToInt32(numLoops.Value);
                double best = double.MaxValue;
                double total = 0;
                string json = string.Empty;

                StringBuilder timing = new StringBuilder();

                for (int i = 0; i < loops; i++)
                {
                    Stopwatch watch = Stopwatch.StartNew();
                    json = engine.Recognize(bytes);
                    watch.Stop();

                    double elapsed = watch.Elapsed.TotalMilliseconds;

                    if (elapsed < best)
                    {
                        best = elapsed;
                    }

                    total += elapsed;

                    timing.AppendLine(
                        "第" +
                        (i + 1) +
                        "次: " +
                        elapsed.ToString("F1") +
                        " ms");

                    Application.DoEvents();
                }

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializer.MaxJsonLength = int.MaxValue;

                OcrResponse response = serializer.Deserialize<OcrResponse>(json);

                if (response == null)
                {
                    throw new InvalidOperationException("OCR返回结果为空，无法解析JSON。");
                }

                if (response.results == null)
                {
                    response.results = new List<OcrItem>();
                }

                DrawResults(response.results);

                StringBuilder summary = new StringBuilder();

                summary.AppendLine("图片: " + imagePath);
                summary.AppendLine("循环次数: " + loops);
                summary.AppendLine(new string('-', 70));
                summary.Append(timing);
                summary.AppendLine("最快: " + best.ToString("F1") + " ms");
                summary.AppendLine("平均: " + (total / loops).ToString("F1") + " ms");
                summary.AppendLine("DLL内部OCR: " + response.elapsed_ms.ToString("F1") + " ms");
                summary.AppendLine("文本框数量: " + response.results.Count);
                summary.AppendLine(new string('-', 70));

                foreach (OcrItem item in response.results)
                {
                    summary.AppendLine(item.text + "  [" + item.score.ToString("F3") + "]");
                }

                summaryText = summary.ToString();

                fullText =
                    summaryText +
                    Environment.NewLine +
                    new string('=', 70) +
                    Environment.NewLine +
                    "完整JSON" +
                    Environment.NewLine +
                    new string('=', 70) +
                    Environment.NewLine +
                    json;

                output.Text = chkFullJson.Checked ? fullText : summaryText;

                statusLabel.Text =
                    "识别成功 | 最快 " +
                    best.ToString("F1") +
                    " ms | 平均 " +
                    (total / loops).ToString("F1") +
                    " ms";
            }
            catch (Exception ex)
            {
                output.Text = ex.ToString();
                statusLabel.Text = "识别失败: " + ex.Message;

                MessageBox.Show(
                    this,
                    ex.Message,
                    "识别失败",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void chkFullJson_CheckedChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(summaryText))
            {
                return;
            }

            output.Text = chkFullJson.Checked ? fullText : summaryText;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DisposeRuntimeObjects();
        }

        private void DrawResults(List<OcrItem> results)
        {
            if (originalImage == null)
            {
                return;
            }

            if (displayImage != null)
            {
                displayImage.Dispose();
                displayImage = null;
            }

            displayImage = new Bitmap(originalImage);

            using (Graphics graphics = Graphics.FromImage(displayImage))
            using (Pen pen = new Pen(Color.Red, 2F))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;

                foreach (OcrItem item in results)
                {
                    Point[] points =
                    {
                        new Point(item.x1, item.y1),
                        new Point(item.x2, item.y2),
                        new Point(item.x3, item.y3),
                        new Point(item.x4, item.y4)
                    };

                    graphics.DrawPolygon(pen, points);
                }
            }

            picture.Image = displayImage;
        }

        private void LoadImage(string path)
        {
            picture.Image = null;

            if (originalImage != null)
            {
                originalImage.Dispose();
                originalImage = null;
            }

            if (displayImage != null)
            {
                displayImage.Dispose();
                displayImage = null;
            }

            using (FileStream stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite))
            using (Image loaded = Image.FromStream(stream))
            {
                originalImage = new Bitmap(loaded);
            }

            displayImage = new Bitmap(originalImage);
            picture.Image = displayImage;
            imagePath = path;

            statusLabel.Text = "已选择: " + path;
        }

        private ModelFiles GetSelectedModel()
        {
            string root = GetModelDirectory();
            ModelFiles model = new ModelFiles();

            // V5和V6统一使用同一个方向分类模型。
            // 是否启用由界面中的chkAngle控制。
            model.Classifier = Path.Combine(root, "PP-OCRv5_mobile_cls_onnx.onnx");
            model.UseClassifier = chkAngle.Checked;

            if (rdoV5Mobile.Checked)
            {
                model.Name = "PP-OCRv5 mobile";
                model.Detector = Path.Combine(root, "PP-OCRv5_mobile_det_onnx.onnx");
                model.Recognizer = Path.Combine(root, "PP-OCRv5_mobile_rec_onnx.onnx");
                model.Dictionary = Path.Combine(root, "ppocrv5_dict.txt");
            }
            else if (rdoV5Server.Checked)
            {
                model.Name = "PP-OCRv5 server";
                model.Detector = Path.Combine(root, "PP-OCRv5_server_det_infer.onnx");
                model.Recognizer = Path.Combine(root, "PP-OCRv5_server_rec_infer.onnx");
                model.Dictionary = Path.Combine(root, "ppocrv5_dict.txt");
            }
            else if (rdoV6Small.Checked)
            {
                model.Name = "PP-OCRv6 small";
                model.Detector = Path.Combine(root, "PP-OCRv6_small_det.onnx");
                model.Recognizer = Path.Combine(root, "PP-OCRv6_small_rec.onnx");
                model.Dictionary = Path.Combine(root, "PP-OCRv6_small_rec_dict.txt");
            }
            else
            {
                model.Name = "PP-OCRv6 tiny";
                model.Detector = Path.Combine(root, "PP-OCRv6_tiny_det.onnx");
                model.Recognizer = Path.Combine(root, "PP-OCRv6_tiny_rec.onnx");
                model.Dictionary = Path.Combine(root, "PP-OCRv6_tiny_rec_dict.txt");
            }

            return model;
        }

        private static string GetModelDirectory()
        {
            string sourceModels = Path.Combine(
                Path.Combine(FindSolutionRoot(), "models"),
                "inference");

            if (Directory.Exists(sourceModels))
            {
                return sourceModels;
            }

            string besideExe = Path.Combine(Application.StartupPath, "inference");
            return besideExe;
        }

        private static string FindSolutionRoot()
        {
            DirectoryInfo directory = new DirectoryInfo(Application.StartupPath);

            while (directory != null)
            {
                string solutionFile = Path.Combine(
                    directory.FullName,
                    "lw.OpenCVDNN.PPOCR.Win7.sln");

                if (File.Exists(solutionFile))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            return Application.StartupPath;
        }

        private static void ValidateFile(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new FileNotFoundException("模型或字典路径为空。");
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("模型或字典不存在。", path);
            }
        }

        private static int ParseInt(string value, int fallback)
        {
            if (int.TryParse(value, out int result))
            {
                return result;
            }

            return fallback;
        }

        private static double ParseDouble(string value, double fallback)
        {
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }

            // 兼容部分中文系统中用户输入逗号小数点的情况。
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out result))
            {
                return result;
            }

            return fallback;
        }

        private void SetBusy(bool busy)
        {
            btnInit.Enabled = !busy;
            btnDestroy.Enabled = !busy;
            btnSelect.Enabled = !busy;
            btnRecognize.Enabled = !busy;

            UseWaitCursor = busy;
        }

        private void AppendLine(string value)
        {
            output.AppendText(value + Environment.NewLine);
        }

        private void ReleaseEngine()
        {
            if (engine == null)
            {
                return;
            }

            engine.Dispose();
            engine = null;
        }

        private void DisposeRuntimeObjects()
        {
            ReleaseEngine();

            picture.Image = null;

            if (displayImage != null)
            {
                displayImage.Dispose();
                displayImage = null;
            }

            if (originalImage != null)
            {
                originalImage.Dispose();
                originalImage = null;
            }
        }

        private sealed class ModelFiles
        {
            public string Name;
            public string Detector;
            public string Recognizer;
            public string Dictionary;
            public string Classifier;
            public bool UseClassifier;
        }
    }
}
