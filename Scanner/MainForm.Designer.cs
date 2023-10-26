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
            Label label1;
            Label label2;
            Label label3;
            Label label4;
            Label label5;
            Label label6;
            Label label7;
            ContentPanel = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            DatabaseButton = new Button();
            DevicesBox = new ComboBox();
            FreqBox = new NumericUpDown();
            GainBox = new ComboBox();
            ControlButton = new Button();
            AdditionalPanel = new TableLayoutPanel();
            SignalNameBox = new TextBox();
            SamplerateBox = new NumericUpDown();
            HashBox = new TextBox();
            BandwidthBox = new NumericUpDown();
            tableLayoutPanel3 = new TableLayoutPanel();
            SpectrPlot = new ScottPlot.FormsPlot();
            SignalPlot = new ScottPlot.FormsPlot();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            label7 = new Label();
            ContentPanel.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)FreqBox).BeginInit();
            AdditionalPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)SamplerateBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)BandwidthBox).BeginInit();
            tableLayoutPanel3.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Top;
            label1.Location = new Point(3, 62);
            label1.Name = "label1";
            label1.Padding = new Padding(2);
            label1.Size = new Size(228, 26);
            label1.TabIndex = 0;
            label1.Text = "Устройство:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Dock = DockStyle.Top;
            label2.Location = new Point(3, 124);
            label2.Name = "label2";
            label2.Padding = new Padding(2);
            label2.Size = new Size(228, 26);
            label2.TabIndex = 2;
            label2.Text = "Частота (кГц):";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Dock = DockStyle.Top;
            label3.Location = new Point(3, 186);
            label3.Name = "label3";
            label3.Padding = new Padding(2);
            label3.Size = new Size(228, 26);
            label3.TabIndex = 4;
            label3.Text = "Усиление:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Dock = DockStyle.Top;
            label4.Location = new Point(3, 0);
            label4.Name = "label4";
            label4.Padding = new Padding(2);
            label4.Size = new Size(228, 26);
            label4.TabIndex = 5;
            label4.Text = "Samplerate:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Dock = DockStyle.Top;
            label5.Location = new Point(3, 62);
            label5.Name = "label5";
            label5.Padding = new Padding(2);
            label5.Size = new Size(228, 26);
            label5.TabIndex = 7;
            label5.Text = "Сигнал:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Dock = DockStyle.Top;
            label6.Location = new Point(3, 0);
            label6.Name = "label6";
            label6.Padding = new Padding(2);
            label6.Size = new Size(228, 26);
            label6.TabIndex = 8;
            label6.Text = "Ширина полосы:";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Dock = DockStyle.Top;
            label7.Location = new Point(3, 124);
            label7.Name = "label7";
            label7.Padding = new Padding(2);
            label7.Size = new Size(228, 26);
            label7.TabIndex = 9;
            label7.Text = "Сигнал:";
            // 
            // ContentPanel
            // 
            ContentPanel.ColumnCount = 2;
            ContentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            ContentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            ContentPanel.Controls.Add(tableLayoutPanel2, 1, 0);
            ContentPanel.Controls.Add(tableLayoutPanel3, 0, 0);
            ContentPanel.Dock = DockStyle.Fill;
            ContentPanel.Location = new Point(2, 2);
            ContentPanel.Margin = new Padding(0);
            ContentPanel.Name = "ContentPanel";
            ContentPanel.RowCount = 1;
            ContentPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            ContentPanel.Size = new Size(778, 549);
            ContentPanel.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.AutoSize = true;
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(DatabaseButton, 0, 11);
            tableLayoutPanel2.Controls.Add(label6, 0, 0);
            tableLayoutPanel2.Controls.Add(label3, 0, 7);
            tableLayoutPanel2.Controls.Add(label2, 0, 5);
            tableLayoutPanel2.Controls.Add(label1, 0, 3);
            tableLayoutPanel2.Controls.Add(DevicesBox, 0, 4);
            tableLayoutPanel2.Controls.Add(FreqBox, 0, 6);
            tableLayoutPanel2.Controls.Add(GainBox, 0, 8);
            tableLayoutPanel2.Controls.Add(ControlButton, 0, 10);
            tableLayoutPanel2.Controls.Add(AdditionalPanel, 0, 9);
            tableLayoutPanel2.Controls.Add(BandwidthBox, 0, 1);
            tableLayoutPanel2.Dock = DockStyle.Top;
            tableLayoutPanel2.Location = new Point(544, 0);
            tableLayoutPanel2.Margin = new Padding(0);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 12;
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.Size = new Size(234, 526);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // DatabaseButton
            // 
            DatabaseButton.AutoSize = true;
            DatabaseButton.Dock = DockStyle.Top;
            DatabaseButton.Location = new Point(3, 483);
            DatabaseButton.Name = "DatabaseButton";
            DatabaseButton.Padding = new Padding(4);
            DatabaseButton.Size = new Size(228, 40);
            DatabaseButton.TabIndex = 10;
            DatabaseButton.Text = "База сигналов";
            DatabaseButton.UseVisualStyleBackColor = true;
            DatabaseButton.Click += DatabaseButton_Click;
            // 
            // DevicesBox
            // 
            DevicesBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            DevicesBox.AutoCompleteSource = AutoCompleteSource.ListItems;
            DevicesBox.Dock = DockStyle.Top;
            DevicesBox.FormattingEnabled = true;
            DevicesBox.Location = new Point(3, 91);
            DevicesBox.Name = "DevicesBox";
            DevicesBox.Size = new Size(228, 30);
            DevicesBox.TabIndex = 3;
            // 
            // FreqBox
            // 
            FreqBox.Dock = DockStyle.Top;
            FreqBox.Increment = new decimal(new int[] { 1000, 0, 0, 0 });
            FreqBox.Location = new Point(3, 153);
            FreqBox.Maximum = new decimal(new int[] { 1600000, 0, 0, 0 });
            FreqBox.Minimum = new decimal(new int[] { 100000, 0, 0, 0 });
            FreqBox.Name = "FreqBox";
            FreqBox.Size = new Size(228, 30);
            FreqBox.TabIndex = 3;
            FreqBox.ThousandsSeparator = true;
            FreqBox.Value = new decimal(new int[] { 433950, 0, 0, 0 });
            // 
            // GainBox
            // 
            GainBox.Dock = DockStyle.Top;
            GainBox.FormattingEnabled = true;
            GainBox.Location = new Point(3, 215);
            GainBox.Name = "GainBox";
            GainBox.Size = new Size(228, 30);
            GainBox.TabIndex = 5;
            // 
            // ControlButton
            // 
            ControlButton.AutoSize = true;
            ControlButton.Dock = DockStyle.Top;
            ControlButton.Location = new Point(3, 437);
            ControlButton.Name = "ControlButton";
            ControlButton.Padding = new Padding(4);
            ControlButton.Size = new Size(228, 40);
            ControlButton.TabIndex = 6;
            ControlButton.Text = "Запуск";
            ControlButton.UseVisualStyleBackColor = true;
            // 
            // AdditionalPanel
            // 
            AdditionalPanel.AutoSize = true;
            AdditionalPanel.ColumnCount = 1;
            AdditionalPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            AdditionalPanel.Controls.Add(SignalNameBox, 0, 5);
            AdditionalPanel.Controls.Add(label7, 0, 4);
            AdditionalPanel.Controls.Add(label5, 0, 2);
            AdditionalPanel.Controls.Add(label4, 0, 0);
            AdditionalPanel.Controls.Add(SamplerateBox, 0, 1);
            AdditionalPanel.Controls.Add(HashBox, 0, 3);
            AdditionalPanel.Dock = DockStyle.Top;
            AdditionalPanel.Location = new Point(0, 248);
            AdditionalPanel.Margin = new Padding(0);
            AdditionalPanel.Name = "AdditionalPanel";
            AdditionalPanel.RowCount = 6;
            AdditionalPanel.RowStyles.Add(new RowStyle());
            AdditionalPanel.RowStyles.Add(new RowStyle());
            AdditionalPanel.RowStyles.Add(new RowStyle());
            AdditionalPanel.RowStyles.Add(new RowStyle());
            AdditionalPanel.RowStyles.Add(new RowStyle());
            AdditionalPanel.RowStyles.Add(new RowStyle());
            AdditionalPanel.Size = new Size(234, 186);
            AdditionalPanel.TabIndex = 7;
            // 
            // SignalNameBox
            // 
            SignalNameBox.Dock = DockStyle.Top;
            SignalNameBox.Location = new Point(3, 153);
            SignalNameBox.Name = "SignalNameBox";
            SignalNameBox.ReadOnly = true;
            SignalNameBox.Size = new Size(228, 30);
            SignalNameBox.TabIndex = 10;
            // 
            // SamplerateBox
            // 
            SamplerateBox.Dock = DockStyle.Top;
            SamplerateBox.Location = new Point(3, 29);
            SamplerateBox.Maximum = new decimal(new int[] { 1600000000, 0, 0, 0 });
            SamplerateBox.Name = "SamplerateBox";
            SamplerateBox.ReadOnly = true;
            SamplerateBox.Size = new Size(228, 30);
            SamplerateBox.TabIndex = 6;
            SamplerateBox.ThousandsSeparator = true;
            // 
            // HashBox
            // 
            HashBox.Dock = DockStyle.Top;
            HashBox.Location = new Point(3, 91);
            HashBox.Name = "HashBox";
            HashBox.ReadOnly = true;
            HashBox.Size = new Size(228, 30);
            HashBox.TabIndex = 8;
            // 
            // BandwidthBox
            // 
            BandwidthBox.Dock = DockStyle.Top;
            BandwidthBox.Location = new Point(3, 29);
            BandwidthBox.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            BandwidthBox.Minimum = new decimal(new int[] { 10000, 0, 0, 0 });
            BandwidthBox.Name = "BandwidthBox";
            BandwidthBox.Size = new Size(228, 30);
            BandwidthBox.TabIndex = 9;
            BandwidthBox.Value = new decimal(new int[] { 200000, 0, 0, 0 });
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 1;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Controls.Add(SpectrPlot, 0, 0);
            tableLayoutPanel3.Controls.Add(SignalPlot, 0, 1);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(0, 0);
            tableLayoutPanel3.Margin = new Padding(0);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 2;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel3.Size = new Size(544, 549);
            tableLayoutPanel3.TabIndex = 1;
            // 
            // SpectrPlot
            // 
            SpectrPlot.Dock = DockStyle.Fill;
            SpectrPlot.Location = new Point(0, 0);
            SpectrPlot.Margin = new Padding(0);
            SpectrPlot.Name = "SpectrPlot";
            SpectrPlot.Padding = new Padding(2);
            SpectrPlot.Size = new Size(544, 274);
            SpectrPlot.TabIndex = 0;
            // 
            // SignalPlot
            // 
            SignalPlot.Dock = DockStyle.Fill;
            SignalPlot.Location = new Point(0, 274);
            SignalPlot.Margin = new Padding(0);
            SignalPlot.Name = "SignalPlot";
            SignalPlot.Size = new Size(544, 275);
            SignalPlot.TabIndex = 1;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(11F, 22F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(782, 553);
            Controls.Add(ContentPanel);
            Font = new Font("Times New Roman", 12F, FontStyle.Regular, GraphicsUnit.Point);
            Margin = new Padding(4);
            MinimumSize = new Size(800, 600);
            Name = "MainForm";
            Padding = new Padding(2);
            Text = "Scanner";
            WindowState = FormWindowState.Maximized;
            FormClosing += MainForm_FormClosing;
            ContentPanel.ResumeLayout(false);
            ContentPanel.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)FreqBox).EndInit();
            AdditionalPanel.ResumeLayout(false);
            AdditionalPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)SamplerateBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)BandwidthBox).EndInit();
            tableLayoutPanel3.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel ContentPanel;
        private TableLayoutPanel tableLayoutPanel2;
        private ComboBox DevicesBox;
        private NumericUpDown FreqBox;
        private ComboBox GainBox;
        private TableLayoutPanel tableLayoutPanel3;
        private ScottPlot.FormsPlot SpectrPlot;
        private Button ControlButton;
        private TableLayoutPanel AdditionalPanel;
        private NumericUpDown SamplerateBox;
        private TextBox HashBox;
        private ScottPlot.FormsPlot SignalPlot;
        private NumericUpDown BandwidthBox;
        private TextBox SignalNameBox;
        private Button DatabaseButton;
    }
}