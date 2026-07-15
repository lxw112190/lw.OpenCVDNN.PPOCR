namespace LwOpenCVDnnPPOCRWin7Test
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.GroupBox models;
        private System.Windows.Forms.GroupBox detection;
        private System.Windows.Forms.GroupBox recognition;

        private System.Windows.Forms.RadioButton rdoV5Mobile;
        private System.Windows.Forms.RadioButton rdoV5Server;
        private System.Windows.Forms.RadioButton rdoV6Tiny;
        private System.Windows.Forms.RadioButton rdoV6Small;
        private System.Windows.Forms.CheckBox chkAngle;

        private System.Windows.Forms.Label lblLimit;
        private System.Windows.Forms.Label lblDetThresh;
        private System.Windows.Forms.Label lblBoxThresh;
        private System.Windows.Forms.Label lblUnclip;

        private System.Windows.Forms.TextBox txtLimit;
        private System.Windows.Forms.TextBox txtDetThresh;
        private System.Windows.Forms.TextBox txtBoxThresh;
        private System.Windows.Forms.TextBox txtUnclip;
        private System.Windows.Forms.CheckBox chkDilation;

        private System.Windows.Forms.Label lblBatch;
        private System.Windows.Forms.Label lblPredictors;
        private System.Windows.Forms.Label lblThreads;
        private System.Windows.Forms.Label lblLoops;

        private System.Windows.Forms.TextBox txtBatch;
        private System.Windows.Forms.TextBox txtPredictors;
        private System.Windows.Forms.TextBox txtThreads;
        private System.Windows.Forms.NumericUpDown numLoops;

        private System.Windows.Forms.Button btnInit;
        private System.Windows.Forms.Button btnDestroy;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.Button btnRecognize;
        private System.Windows.Forms.Button btnRecognizeRegion;
        private System.Windows.Forms.Button btnClearRegion;
        private System.Windows.Forms.Label lblRegionHint;
        private System.Windows.Forms.CheckBox chkFullJson;

        private System.Windows.Forms.PictureBox picture;
        private System.Windows.Forms.RichTextBox output;

        private System.Windows.Forms.StatusStrip status;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeRuntimeObjects();

                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.models = new System.Windows.Forms.GroupBox();
            this.rdoV5Mobile = new System.Windows.Forms.RadioButton();
            this.rdoV5Server = new System.Windows.Forms.RadioButton();
            this.rdoV6Tiny = new System.Windows.Forms.RadioButton();
            this.rdoV6Small = new System.Windows.Forms.RadioButton();
            this.chkAngle = new System.Windows.Forms.CheckBox();
            this.detection = new System.Windows.Forms.GroupBox();
            this.lblLimit = new System.Windows.Forms.Label();
            this.lblDetThresh = new System.Windows.Forms.Label();
            this.lblBoxThresh = new System.Windows.Forms.Label();
            this.lblUnclip = new System.Windows.Forms.Label();
            this.txtLimit = new System.Windows.Forms.TextBox();
            this.txtDetThresh = new System.Windows.Forms.TextBox();
            this.txtBoxThresh = new System.Windows.Forms.TextBox();
            this.txtUnclip = new System.Windows.Forms.TextBox();
            this.chkDilation = new System.Windows.Forms.CheckBox();
            this.recognition = new System.Windows.Forms.GroupBox();
            this.lblBatch = new System.Windows.Forms.Label();
            this.lblPredictors = new System.Windows.Forms.Label();
            this.lblThreads = new System.Windows.Forms.Label();
            this.lblLoops = new System.Windows.Forms.Label();
            this.txtBatch = new System.Windows.Forms.TextBox();
            this.txtPredictors = new System.Windows.Forms.TextBox();
            this.txtThreads = new System.Windows.Forms.TextBox();
            this.numLoops = new System.Windows.Forms.NumericUpDown();
            this.btnInit = new System.Windows.Forms.Button();
            this.btnDestroy = new System.Windows.Forms.Button();
            this.btnSelect = new System.Windows.Forms.Button();
            this.btnRecognize = new System.Windows.Forms.Button();
            this.btnRecognizeRegion = new System.Windows.Forms.Button();
            this.btnClearRegion = new System.Windows.Forms.Button();
            this.lblRegionHint = new System.Windows.Forms.Label();
            this.chkFullJson = new System.Windows.Forms.CheckBox();
            this.picture = new System.Windows.Forms.PictureBox();
            this.output = new System.Windows.Forms.RichTextBox();
            this.status = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.models.SuspendLayout();
            this.detection.SuspendLayout();
            this.recognition.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numLoops)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picture)).BeginInit();
            this.status.SuspendLayout();
            this.SuspendLayout();
            // 
            // models
            // 
            this.models.Controls.Add(this.rdoV5Mobile);
            this.models.Controls.Add(this.rdoV5Server);
            this.models.Controls.Add(this.rdoV6Tiny);
            this.models.Controls.Add(this.rdoV6Small);
            this.models.Controls.Add(this.chkAngle);
            this.models.Location = new System.Drawing.Point(12, 12);
            this.models.Name = "models";
            this.models.Size = new System.Drawing.Size(232, 125);
            this.models.TabIndex = 0;
            this.models.TabStop = false;
            this.models.Text = "模型";
            // 
            // rdoV5Mobile
            // 
            this.rdoV5Mobile.Location = new System.Drawing.Point(14, 24);
            this.rdoV5Mobile.Name = "rdoV5Mobile";
            this.rdoV5Mobile.Size = new System.Drawing.Size(96, 20);
            this.rdoV5Mobile.TabIndex = 0;
            this.rdoV5Mobile.Text = "V5 mobile";
            this.rdoV5Mobile.UseVisualStyleBackColor = true;
            // 
            // rdoV5Server
            // 
            this.rdoV5Server.Location = new System.Drawing.Point(14, 52);
            this.rdoV5Server.Name = "rdoV5Server";
            this.rdoV5Server.Size = new System.Drawing.Size(96, 20);
            this.rdoV5Server.TabIndex = 1;
            this.rdoV5Server.Text = "V5 server";
            this.rdoV5Server.UseVisualStyleBackColor = true;
            // 
            // rdoV6Tiny
            // 
            this.rdoV6Tiny.Checked = true;
            this.rdoV6Tiny.Location = new System.Drawing.Point(116, 24);
            this.rdoV6Tiny.Name = "rdoV6Tiny";
            this.rdoV6Tiny.Size = new System.Drawing.Size(96, 20);
            this.rdoV6Tiny.TabIndex = 2;
            this.rdoV6Tiny.TabStop = true;
            this.rdoV6Tiny.Text = "V6 tiny";
            this.rdoV6Tiny.UseVisualStyleBackColor = true;
            // 
            // rdoV6Small
            // 
            this.rdoV6Small.Location = new System.Drawing.Point(116, 52);
            this.rdoV6Small.Name = "rdoV6Small";
            this.rdoV6Small.Size = new System.Drawing.Size(96, 20);
            this.rdoV6Small.TabIndex = 3;
            this.rdoV6Small.Text = "V6 small";
            this.rdoV6Small.UseVisualStyleBackColor = true;
            // 
            // chkAngle
            // 
            this.chkAngle.Location = new System.Drawing.Point(47, 92);
            this.chkAngle.Name = "chkAngle";
            this.chkAngle.Size = new System.Drawing.Size(96, 22);
            this.chkAngle.TabIndex = 4;
            this.chkAngle.Text = "启用方向分类";
            this.chkAngle.UseVisualStyleBackColor = true;
            // 
            // detection
            // 
            this.detection.Controls.Add(this.lblLimit);
            this.detection.Controls.Add(this.lblDetThresh);
            this.detection.Controls.Add(this.lblBoxThresh);
            this.detection.Controls.Add(this.lblUnclip);
            this.detection.Controls.Add(this.txtLimit);
            this.detection.Controls.Add(this.txtDetThresh);
            this.detection.Controls.Add(this.txtBoxThresh);
            this.detection.Controls.Add(this.txtUnclip);
            this.detection.Controls.Add(this.chkDilation);
            this.detection.Location = new System.Drawing.Point(252, 12);
            this.detection.Name = "detection";
            this.detection.Size = new System.Drawing.Size(278, 125);
            this.detection.TabIndex = 1;
            this.detection.TabStop = false;
            this.detection.Text = "检测参数";
            // 
            // lblLimit
            // 
            this.lblLimit.AutoSize = true;
            this.lblLimit.Location = new System.Drawing.Point(12, 28);
            this.lblLimit.Name = "lblLimit";
            this.lblLimit.Size = new System.Drawing.Size(29, 12);
            this.lblLimit.TabIndex = 0;
            this.lblLimit.Text = "边长";
            // 
            // lblDetThresh
            // 
            this.lblDetThresh.AutoSize = true;
            this.lblDetThresh.Location = new System.Drawing.Point(140, 28);
            this.lblDetThresh.Name = "lblDetThresh";
            this.lblDetThresh.Size = new System.Drawing.Size(53, 12);
            this.lblDetThresh.TabIndex = 1;
            this.lblDetThresh.Text = "像素阈值";
            // 
            // lblBoxThresh
            // 
            this.lblBoxThresh.AutoSize = true;
            this.lblBoxThresh.Location = new System.Drawing.Point(12, 62);
            this.lblBoxThresh.Name = "lblBoxThresh";
            this.lblBoxThresh.Size = new System.Drawing.Size(65, 12);
            this.lblBoxThresh.TabIndex = 2;
            this.lblBoxThresh.Text = "文本框阈值";
            // 
            // lblUnclip
            // 
            this.lblUnclip.AutoSize = true;
            this.lblUnclip.Location = new System.Drawing.Point(140, 62);
            this.lblUnclip.Name = "lblUnclip";
            this.lblUnclip.Size = new System.Drawing.Size(53, 12);
            this.lblUnclip.TabIndex = 3;
            this.lblUnclip.Text = "扩张比例";
            // 
            // txtLimit
            // 
            this.txtLimit.Location = new System.Drawing.Point(68, 24);
            this.txtLimit.Name = "txtLimit";
            this.txtLimit.Size = new System.Drawing.Size(58, 21);
            this.txtLimit.TabIndex = 0;
            this.txtLimit.Text = "960";
            // 
            // txtDetThresh
            // 
            this.txtDetThresh.Location = new System.Drawing.Point(208, 24);
            this.txtDetThresh.Name = "txtDetThresh";
            this.txtDetThresh.Size = new System.Drawing.Size(58, 21);
            this.txtDetThresh.TabIndex = 1;
            this.txtDetThresh.Text = "0.3";
            // 
            // txtBoxThresh
            // 
            this.txtBoxThresh.Location = new System.Drawing.Point(78, 58);
            this.txtBoxThresh.Name = "txtBoxThresh";
            this.txtBoxThresh.Size = new System.Drawing.Size(48, 21);
            this.txtBoxThresh.TabIndex = 2;
            this.txtBoxThresh.Text = "0.6";
            // 
            // txtUnclip
            // 
            this.txtUnclip.Location = new System.Drawing.Point(208, 58);
            this.txtUnclip.Name = "txtUnclip";
            this.txtUnclip.Size = new System.Drawing.Size(58, 21);
            this.txtUnclip.TabIndex = 3;
            this.txtUnclip.Text = "1.5";
            // 
            // chkDilation
            // 
            this.chkDilation.Location = new System.Drawing.Point(14, 92);
            this.chkDilation.Name = "chkDilation";
            this.chkDilation.Size = new System.Drawing.Size(110, 22);
            this.chkDilation.TabIndex = 4;
            this.chkDilation.Text = "启用膨胀";
            this.chkDilation.UseVisualStyleBackColor = true;
            // 
            // recognition
            // 
            this.recognition.Controls.Add(this.lblBatch);
            this.recognition.Controls.Add(this.lblPredictors);
            this.recognition.Controls.Add(this.lblThreads);
            this.recognition.Controls.Add(this.lblLoops);
            this.recognition.Controls.Add(this.txtBatch);
            this.recognition.Controls.Add(this.txtPredictors);
            this.recognition.Controls.Add(this.txtThreads);
            this.recognition.Controls.Add(this.numLoops);
            this.recognition.Location = new System.Drawing.Point(538, 12);
            this.recognition.Name = "recognition";
            this.recognition.Size = new System.Drawing.Size(280, 125);
            this.recognition.TabIndex = 2;
            this.recognition.TabStop = false;
            this.recognition.Text = "识别与测速";
            // 
            // lblBatch
            // 
            this.lblBatch.AutoSize = true;
            this.lblBatch.Location = new System.Drawing.Point(12, 28);
            this.lblBatch.Name = "lblBatch";
            this.lblBatch.Size = new System.Drawing.Size(29, 12);
            this.lblBatch.TabIndex = 0;
            this.lblBatch.Text = "批量";
            // 
            // lblPredictors
            // 
            this.lblPredictors.AutoSize = true;
            this.lblPredictors.Location = new System.Drawing.Point(140, 28);
            this.lblPredictors.Name = "lblPredictors";
            this.lblPredictors.Size = new System.Drawing.Size(41, 12);
            this.lblPredictors.TabIndex = 1;
            this.lblPredictors.Text = "预测器";
            // 
            // lblThreads
            // 
            this.lblThreads.AutoSize = true;
            this.lblThreads.Location = new System.Drawing.Point(12, 62);
            this.lblThreads.Name = "lblThreads";
            this.lblThreads.Size = new System.Drawing.Size(47, 12);
            this.lblThreads.TabIndex = 2;
            this.lblThreads.Text = "CPU线程";
            // 
            // lblLoops
            // 
            this.lblLoops.AutoSize = true;
            this.lblLoops.Location = new System.Drawing.Point(140, 62);
            this.lblLoops.Name = "lblLoops";
            this.lblLoops.Size = new System.Drawing.Size(53, 12);
            this.lblLoops.TabIndex = 3;
            this.lblLoops.Text = "循环次数";
            // 
            // txtBatch
            // 
            this.txtBatch.Location = new System.Drawing.Point(68, 24);
            this.txtBatch.Name = "txtBatch";
            this.txtBatch.Size = new System.Drawing.Size(56, 21);
            this.txtBatch.TabIndex = 0;
            this.txtBatch.Text = "8";
            // 
            // txtPredictors
            // 
            this.txtPredictors.Location = new System.Drawing.Point(208, 24);
            this.txtPredictors.Name = "txtPredictors";
            this.txtPredictors.Size = new System.Drawing.Size(56, 21);
            this.txtPredictors.TabIndex = 1;
            this.txtPredictors.Text = "4";
            // 
            // txtThreads
            // 
            this.txtThreads.Location = new System.Drawing.Point(68, 58);
            this.txtThreads.Name = "txtThreads";
            this.txtThreads.Size = new System.Drawing.Size(56, 21);
            this.txtThreads.TabIndex = 2;
            this.txtThreads.Text = "0";
            // 
            // numLoops
            // 
            this.numLoops.Location = new System.Drawing.Point(208, 58);
            this.numLoops.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numLoops.Name = "numLoops";
            this.numLoops.Size = new System.Drawing.Size(56, 21);
            this.numLoops.TabIndex = 3;
            this.numLoops.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // btnInit
            // 
            this.btnInit.Location = new System.Drawing.Point(832, 18);
            this.btnInit.Name = "btnInit";
            this.btnInit.Size = new System.Drawing.Size(126, 52);
            this.btnInit.TabIndex = 3;
            this.btnInit.Text = "初始化";
            this.btnInit.UseVisualStyleBackColor = true;
            this.btnInit.Click += new System.EventHandler(this.btnInit_Click);
            // 
            // btnDestroy
            // 
            this.btnDestroy.Location = new System.Drawing.Point(968, 18);
            this.btnDestroy.Name = "btnDestroy";
            this.btnDestroy.Size = new System.Drawing.Size(126, 52);
            this.btnDestroy.TabIndex = 4;
            this.btnDestroy.Text = "释放";
            this.btnDestroy.UseVisualStyleBackColor = true;
            this.btnDestroy.Click += new System.EventHandler(this.btnDestroy_Click);
            // 
            // btnSelect
            // 
            this.btnSelect.Location = new System.Drawing.Point(832, 80);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(126, 52);
            this.btnSelect.TabIndex = 5;
            this.btnSelect.Text = "选择图片";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // btnRecognize
            // 
            this.btnRecognize.Location = new System.Drawing.Point(968, 80);
            this.btnRecognize.Name = "btnRecognize";
            this.btnRecognize.Size = new System.Drawing.Size(126, 52);
            this.btnRecognize.TabIndex = 6;
            this.btnRecognize.Text = "识别/测速";
            this.btnRecognize.UseVisualStyleBackColor = true;
            this.btnRecognize.Click += new System.EventHandler(this.btnRecognize_Click);
            //
            // btnRecognizeRegion
            //
            this.btnRecognizeRegion.Location = new System.Drawing.Point(832, 142);
            this.btnRecognizeRegion.Name = "btnRecognizeRegion";
            this.btnRecognizeRegion.Size = new System.Drawing.Size(126, 25);
            this.btnRecognizeRegion.TabIndex = 7;
            this.btnRecognizeRegion.Text = "识别框选区域";
            this.btnRecognizeRegion.UseVisualStyleBackColor = true;
            this.btnRecognizeRegion.Click += new System.EventHandler(this.btnRecognizeRegion_Click);
            //
            // btnClearRegion
            //
            this.btnClearRegion.Location = new System.Drawing.Point(968, 142);
            this.btnClearRegion.Name = "btnClearRegion";
            this.btnClearRegion.Size = new System.Drawing.Size(126, 25);
            this.btnClearRegion.TabIndex = 8;
            this.btnClearRegion.Text = "清除框选";
            this.btnClearRegion.UseVisualStyleBackColor = true;
            this.btnClearRegion.Click += new System.EventHandler(this.btnClearRegion_Click);
            //
            // lblRegionHint
            //
            this.lblRegionHint.AutoSize = true;
            this.lblRegionHint.Location = new System.Drawing.Point(12, 149);
            this.lblRegionHint.Name = "lblRegionHint";
            this.lblRegionHint.Size = new System.Drawing.Size(335, 12);
            this.lblRegionHint.TabIndex = 9;
            this.lblRegionHint.Text = "鼠标左键拖动框选单行文字区域；右键或“清除框选”取消";
            // 
            // chkFullJson
            // 
            this.chkFullJson.Location = new System.Drawing.Point(552, 145);
            this.chkFullJson.Name = "chkFullJson";
            this.chkFullJson.Size = new System.Drawing.Size(130, 22);
            this.chkFullJson.TabIndex = 7;
            this.chkFullJson.Text = "显示完整JSON";
            this.chkFullJson.UseVisualStyleBackColor = true;
            this.chkFullJson.CheckedChanged += new System.EventHandler(this.chkFullJson_CheckedChanged);
            // 
            // picture
            // 
            this.picture.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.picture.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picture.Cursor = System.Windows.Forms.Cursors.Cross;
            this.picture.Location = new System.Drawing.Point(12, 173);
            this.picture.Name = "picture";
            this.picture.Size = new System.Drawing.Size(520, 606);
            this.picture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picture.TabIndex = 8;
            this.picture.TabStop = false;
            this.picture.Paint += new System.Windows.Forms.PaintEventHandler(this.picture_Paint);
            this.picture.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picture_MouseDown);
            this.picture.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picture_MouseMove);
            this.picture.MouseUp += new System.Windows.Forms.MouseEventHandler(this.picture_MouseUp);
            // 
            // output
            // 
            this.output.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.output.Font = new System.Drawing.Font("微软雅黑", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.output.Location = new System.Drawing.Point(552, 173);
            this.output.Name = "output";
            this.output.ReadOnly = true;
            this.output.Size = new System.Drawing.Size(542, 606);
            this.output.TabIndex = 9;
            this.output.Text = "";
            // 
            // status
            // 
            this.status.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.status.Location = new System.Drawing.Point(0, 790);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(1110, 22);
            this.status.TabIndex = 10;
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(347, 17);
            this.statusLabel.Text = "未初始化 | 版本 1.1.0.0 | 作者: 天天代码码天天,QQ:819069052";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1110, 812);
            this.Controls.Add(this.models);
            this.Controls.Add(this.detection);
            this.Controls.Add(this.recognition);
            this.Controls.Add(this.btnInit);
            this.Controls.Add(this.btnDestroy);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.btnRecognize);
            this.Controls.Add(this.btnRecognizeRegion);
            this.Controls.Add(this.btnClearRegion);
            this.Controls.Add(this.lblRegionHint);
            this.Controls.Add(this.chkFullJson);
            this.Controls.Add(this.picture);
            this.Controls.Add(this.output);
            this.Controls.Add(this.status);
            this.MinimumSize = new System.Drawing.Size(1126, 850);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "lw.OpenCVDNN.PPOCR Win7 Test - 1.1.0.0 （天天代码码天天，QQ：819069052）";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.models.ResumeLayout(false);
            this.detection.ResumeLayout(false);
            this.detection.PerformLayout();
            this.recognition.ResumeLayout(false);
            this.recognition.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numLoops)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picture)).EndInit();
            this.status.ResumeLayout(false);
            this.status.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
