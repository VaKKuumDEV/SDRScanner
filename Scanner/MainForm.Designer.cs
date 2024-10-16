namespace Scanner
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            TableLayoutPanel tableLayoutPanel4;
            Label label9;
            TableLayoutPanel tableLayoutPanel5;
            Label label10;
            TableLayoutPanel tableLayoutPanel6;
            Label label1;
            TableLayoutPanel tableLayoutPanel10;
            TableLayoutPanel tableLayoutPanel1;
            Label label3;
            DevicesBox = new ComboBox();
            FreqBox = new NumericUpDown();
            GainBox = new ComboBox();
            ControlButton = new Button();
            bandwidthBox = new NumericUpDown();
            ContentPanel = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            tableLayoutPanel3 = new TableLayoutPanel();
            NoisePlot = new ScottPlot.WinForms.FormsPlot();
            SpectrPlot = new ScottPlot.WinForms.FormsPlot();
            tableLayoutPanel4 = new TableLayoutPanel();
            label9 = new Label();
            tableLayoutPanel5 = new TableLayoutPanel();
            label10 = new Label();
            tableLayoutPanel6 = new TableLayoutPanel();
            label1 = new Label();
            tableLayoutPanel10 = new TableLayoutPanel();
            tableLayoutPanel1 = new TableLayoutPanel();
            label3 = new Label();
            tableLayoutPanel4.SuspendLayout();
            tableLayoutPanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)FreqBox).BeginInit();
            tableLayoutPanel6.SuspendLayout();
            tableLayoutPanel10.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)bandwidthBox).BeginInit();
            ContentPanel.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.AutoSize = true;
            tableLayoutPanel4.ColumnCount = 1;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.Controls.Add(DevicesBox, 0, 1);
            tableLayoutPanel4.Controls.Add(label9, 0, 0);
            tableLayoutPanel4.Dock = DockStyle.Top;
            tableLayoutPanel4.Location = new Point(0, 0);
            tableLayoutPanel4.Margin = new Padding(0);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 2;
            tableLayoutPanel4.RowStyles.Add(new RowStyle());
            tableLayoutPanel4.RowStyles.Add(new RowStyle());
            tableLayoutPanel4.Size = new Size(308, 62);
            tableLayoutPanel4.TabIndex = 12;
            // 
            // DevicesBox
            // 
            DevicesBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            DevicesBox.AutoCompleteSource = AutoCompleteSource.ListItems;
            DevicesBox.Dock = DockStyle.Top;
            DevicesBox.FormattingEnabled = true;
            DevicesBox.Location = new Point(3, 29);
            DevicesBox.Name = "DevicesBox";
            DevicesBox.Size = new Size(302, 30);
            DevicesBox.TabIndex = 0;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Dock = DockStyle.Top;
            label9.Location = new Point(3, 0);
            label9.Name = "label9";
            label9.Padding = new Padding(2);
            label9.Size = new Size(302, 26);
            label9.TabIndex = 1;
            label9.Text = "Устройство:";
            // 
            // tableLayoutPanel5
            // 
            tableLayoutPanel5.AutoSize = true;
            tableLayoutPanel5.ColumnCount = 1;
            tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel5.Controls.Add(FreqBox, 0, 1);
            tableLayoutPanel5.Controls.Add(label10, 0, 0);
            tableLayoutPanel5.Dock = DockStyle.Top;
            tableLayoutPanel5.Location = new Point(0, 124);
            tableLayoutPanel5.Margin = new Padding(0);
            tableLayoutPanel5.Name = "tableLayoutPanel5";
            tableLayoutPanel5.RowCount = 2;
            tableLayoutPanel5.RowStyles.Add(new RowStyle());
            tableLayoutPanel5.RowStyles.Add(new RowStyle());
            tableLayoutPanel5.Size = new Size(308, 62);
            tableLayoutPanel5.TabIndex = 13;
            // 
            // FreqBox
            // 
            FreqBox.Dock = DockStyle.Top;
            FreqBox.Increment = new decimal(new int[] { 1000, 0, 0, 0 });
            FreqBox.Location = new Point(3, 29);
            FreqBox.Maximum = new decimal(new int[] { 1600000, 0, 0, 0 });
            FreqBox.Minimum = new decimal(new int[] { 100000, 0, 0, 0 });
            FreqBox.Name = "FreqBox";
            FreqBox.Size = new Size(302, 30);
            FreqBox.TabIndex = 2;
            FreqBox.ThousandsSeparator = true;
            FreqBox.Value = new decimal(new int[] { 433950, 0, 0, 0 });
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Dock = DockStyle.Top;
            label10.Location = new Point(3, 0);
            label10.Name = "label10";
            label10.Padding = new Padding(2);
            label10.Size = new Size(302, 26);
            label10.TabIndex = 3;
            label10.Text = "Частота (кГц):";
            // 
            // tableLayoutPanel6
            // 
            tableLayoutPanel6.AutoSize = true;
            tableLayoutPanel6.ColumnCount = 1;
            tableLayoutPanel6.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel6.Controls.Add(GainBox, 0, 1);
            tableLayoutPanel6.Controls.Add(label1, 0, 0);
            tableLayoutPanel6.Dock = DockStyle.Top;
            tableLayoutPanel6.Location = new Point(0, 186);
            tableLayoutPanel6.Margin = new Padding(0);
            tableLayoutPanel6.Name = "tableLayoutPanel6";
            tableLayoutPanel6.RowCount = 2;
            tableLayoutPanel6.RowStyles.Add(new RowStyle());
            tableLayoutPanel6.RowStyles.Add(new RowStyle());
            tableLayoutPanel6.Size = new Size(308, 62);
            tableLayoutPanel6.TabIndex = 14;
            // 
            // GainBox
            // 
            GainBox.Dock = DockStyle.Top;
            GainBox.FormattingEnabled = true;
            GainBox.Location = new Point(3, 29);
            GainBox.Name = "GainBox";
            GainBox.Size = new Size(302, 30);
            GainBox.TabIndex = 3;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Top;
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Padding = new Padding(2);
            label1.Size = new Size(302, 26);
            label1.TabIndex = 5;
            label1.Text = "Усиление:";
            // 
            // tableLayoutPanel10
            // 
            tableLayoutPanel10.AutoSize = true;
            tableLayoutPanel10.ColumnCount = 1;
            tableLayoutPanel10.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel10.Controls.Add(ControlButton, 0, 0);
            tableLayoutPanel10.Dock = DockStyle.Top;
            tableLayoutPanel10.Location = new Point(0, 248);
            tableLayoutPanel10.Margin = new Padding(0);
            tableLayoutPanel10.Name = "tableLayoutPanel10";
            tableLayoutPanel10.RowCount = 3;
            tableLayoutPanel10.RowStyles.Add(new RowStyle());
            tableLayoutPanel10.RowStyles.Add(new RowStyle());
            tableLayoutPanel10.RowStyles.Add(new RowStyle());
            tableLayoutPanel10.Size = new Size(308, 46);
            tableLayoutPanel10.TabIndex = 18;
            // 
            // ControlButton
            // 
            ControlButton.AutoSize = true;
            ControlButton.Dock = DockStyle.Top;
            ControlButton.Location = new Point(3, 3);
            ControlButton.Name = "ControlButton";
            ControlButton.Padding = new Padding(4);
            ControlButton.Size = new Size(302, 40);
            ControlButton.TabIndex = 7;
            ControlButton.Text = "Запуск";
            ControlButton.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(bandwidthBox, 0, 1);
            tableLayoutPanel1.Controls.Add(label3, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Top;
            tableLayoutPanel1.Location = new Point(0, 62);
            tableLayoutPanel1.Margin = new Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(308, 62);
            tableLayoutPanel1.TabIndex = 20;
            // 
            // bandwidthBox
            // 
            bandwidthBox.Dock = DockStyle.Top;
            bandwidthBox.Increment = new decimal(new int[] { 1000, 0, 0, 0 });
            bandwidthBox.Location = new Point(3, 29);
            bandwidthBox.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            bandwidthBox.Minimum = new decimal(new int[] { 10000, 0, 0, 0 });
            bandwidthBox.Name = "bandwidthBox";
            bandwidthBox.Size = new Size(302, 30);
            bandwidthBox.TabIndex = 1;
            bandwidthBox.ThousandsSeparator = true;
            bandwidthBox.Value = new decimal(new int[] { 75000, 0, 0, 0 });
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Dock = DockStyle.Top;
            label3.Location = new Point(3, 0);
            label3.Name = "label3";
            label3.Padding = new Padding(2);
            label3.Size = new Size(302, 26);
            label3.TabIndex = 3;
            label3.Text = "Ширина окна (кГц):";
            // 
            // ContentPanel
            // 
            ContentPanel.ColumnCount = 2;
            ContentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            ContentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            ContentPanel.Controls.Add(tableLayoutPanel2, 1, 0);
            ContentPanel.Controls.Add(tableLayoutPanel3, 0, 0);
            ContentPanel.Dock = DockStyle.Fill;
            ContentPanel.Location = new Point(2, 2);
            ContentPanel.Margin = new Padding(0);
            ContentPanel.Name = "ContentPanel";
            ContentPanel.RowCount = 1;
            ContentPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            ContentPanel.Size = new Size(878, 649);
            ContentPanel.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.AutoSize = true;
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(tableLayoutPanel1, 0, 1);
            tableLayoutPanel2.Controls.Add(tableLayoutPanel4, 0, 0);
            tableLayoutPanel2.Controls.Add(tableLayoutPanel5, 0, 2);
            tableLayoutPanel2.Controls.Add(tableLayoutPanel6, 0, 3);
            tableLayoutPanel2.Controls.Add(tableLayoutPanel10, 0, 4);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(570, 0);
            tableLayoutPanel2.Margin = new Padding(0);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 5;
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel2.Size = new Size(308, 649);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 1;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Controls.Add(NoisePlot, 0, 1);
            tableLayoutPanel3.Controls.Add(SpectrPlot, 0, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(0, 0);
            tableLayoutPanel3.Margin = new Padding(0);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 2;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel3.Size = new Size(570, 649);
            tableLayoutPanel3.TabIndex = 1;
            // 
            // NoisePlot
            // 
            NoisePlot.DisplayScale = 1.25F;
            NoisePlot.Dock = DockStyle.Fill;
            NoisePlot.Location = new Point(0, 324);
            NoisePlot.Margin = new Padding(0);
            NoisePlot.Name = "NoisePlot";
            NoisePlot.Padding = new Padding(2);
            NoisePlot.Size = new Size(570, 325);
            NoisePlot.TabIndex = 1;
            // 
            // SpectrPlot
            // 
            SpectrPlot.DisplayScale = 1.25F;
            SpectrPlot.Dock = DockStyle.Fill;
            SpectrPlot.Location = new Point(0, 0);
            SpectrPlot.Margin = new Padding(0);
            SpectrPlot.Name = "SpectrPlot";
            SpectrPlot.Padding = new Padding(2);
            SpectrPlot.Size = new Size(570, 324);
            SpectrPlot.TabIndex = 0;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(11F, 22F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(882, 653);
            Controls.Add(ContentPanel);
            Font = new Font("Times New Roman", 12F);
            Margin = new Padding(4);
            MinimumSize = new Size(800, 600);
            Name = "MainForm";
            Padding = new Padding(2);
            Text = "Scanner";
            WindowState = FormWindowState.Maximized;
            FormClosing += MainForm_FormClosing;
            tableLayoutPanel4.ResumeLayout(false);
            tableLayoutPanel4.PerformLayout();
            tableLayoutPanel5.ResumeLayout(false);
            tableLayoutPanel5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)FreqBox).EndInit();
            tableLayoutPanel6.ResumeLayout(false);
            tableLayoutPanel6.PerformLayout();
            tableLayoutPanel10.ResumeLayout(false);
            tableLayoutPanel10.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)bandwidthBox).EndInit();
            ContentPanel.ResumeLayout(false);
            ContentPanel.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel ContentPanel;
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel3;
        private ScottPlot.WinForms.FormsPlot SpectrPlot;
        private ComboBox DevicesBox;
        private NumericUpDown FreqBox;
        private ComboBox GainBox;
        private TableLayoutPanel tableLayoutPanel10;
        private Button ControlButton;
        private ScottPlot.WinForms.FormsPlot NoisePlot;
        private NumericUpDown bandwidthBox;
    }
}