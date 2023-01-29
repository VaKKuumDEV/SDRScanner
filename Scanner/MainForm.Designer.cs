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
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label4;
            this.ContentPanel = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.DevicesBox = new System.Windows.Forms.ComboBox();
            this.FreqBox = new System.Windows.Forms.NumericUpDown();
            this.GainBox = new System.Windows.Forms.ComboBox();
            this.ControlButton = new System.Windows.Forms.Button();
            this.AdditionalPanel = new System.Windows.Forms.TableLayoutPanel();
            this.SamplerateBox = new System.Windows.Forms.NumericUpDown();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.SpectrPlot = new ScottPlot.FormsPlot();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            this.ContentPanel.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FreqBox)).BeginInit();
            this.AdditionalPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SamplerateBox)).BeginInit();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = System.Windows.Forms.DockStyle.Top;
            label1.Location = new System.Drawing.Point(3, 0);
            label1.Name = "label1";
            label1.Padding = new System.Windows.Forms.Padding(2);
            label1.Size = new System.Drawing.Size(168, 23);
            label1.TabIndex = 0;
            label1.Text = "Устройство:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Dock = System.Windows.Forms.DockStyle.Top;
            label2.Location = new System.Drawing.Point(3, 56);
            label2.Name = "label2";
            label2.Padding = new System.Windows.Forms.Padding(2);
            label2.Size = new System.Drawing.Size(168, 23);
            label2.TabIndex = 2;
            label2.Text = "Рабочая частота:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Dock = System.Windows.Forms.DockStyle.Top;
            label3.Location = new System.Drawing.Point(3, 111);
            label3.Name = "label3";
            label3.Padding = new System.Windows.Forms.Padding(2);
            label3.Size = new System.Drawing.Size(168, 23);
            label3.TabIndex = 4;
            label3.Text = "Усиление:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Dock = System.Windows.Forms.DockStyle.Top;
            label4.Location = new System.Drawing.Point(3, 0);
            label4.Name = "label4";
            label4.Padding = new System.Windows.Forms.Padding(2);
            label4.Size = new System.Drawing.Size(168, 23);
            label4.TabIndex = 5;
            label4.Text = "Samplerate:";
            // 
            // ContentPanel
            // 
            this.ContentPanel.ColumnCount = 2;
            this.ContentPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.ContentPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.ContentPanel.Controls.Add(this.tableLayoutPanel2, 1, 0);
            this.ContentPanel.Controls.Add(this.tableLayoutPanel3, 0, 0);
            this.ContentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ContentPanel.Location = new System.Drawing.Point(2, 2);
            this.ContentPanel.Margin = new System.Windows.Forms.Padding(0);
            this.ContentPanel.Name = "ContentPanel";
            this.ContentPanel.RowCount = 1;
            this.ContentPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ContentPanel.Size = new System.Drawing.Size(580, 457);
            this.ContentPanel.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(label3, 0, 4);
            this.tableLayoutPanel2.Controls.Add(label2, 0, 2);
            this.tableLayoutPanel2.Controls.Add(label1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.DevicesBox, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.FreqBox, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.GainBox, 0, 5);
            this.tableLayoutPanel2.Controls.Add(this.ControlButton, 0, 7);
            this.tableLayoutPanel2.Controls.Add(this.AdditionalPanel, 0, 6);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(406, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 8;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(174, 265);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // DevicesBox
            // 
            this.DevicesBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.DevicesBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.DevicesBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.DevicesBox.FormattingEnabled = true;
            this.DevicesBox.Location = new System.Drawing.Point(3, 26);
            this.DevicesBox.Name = "DevicesBox";
            this.DevicesBox.Size = new System.Drawing.Size(168, 27);
            this.DevicesBox.TabIndex = 1;
            // 
            // FreqBox
            // 
            this.FreqBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.FreqBox.Increment = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.FreqBox.Location = new System.Drawing.Point(3, 82);
            this.FreqBox.Maximum = new decimal(new int[] {
            1600000000,
            0,
            0,
            0});
            this.FreqBox.Minimum = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            this.FreqBox.Name = "FreqBox";
            this.FreqBox.Size = new System.Drawing.Size(168, 26);
            this.FreqBox.TabIndex = 3;
            this.FreqBox.ThousandsSeparator = true;
            this.FreqBox.Value = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            // 
            // GainBox
            // 
            this.GainBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.GainBox.FormattingEnabled = true;
            this.GainBox.Location = new System.Drawing.Point(3, 137);
            this.GainBox.Name = "GainBox";
            this.GainBox.Size = new System.Drawing.Size(168, 27);
            this.GainBox.TabIndex = 5;
            // 
            // ControlButton
            // 
            this.ControlButton.AutoSize = true;
            this.ControlButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.ControlButton.Location = new System.Drawing.Point(3, 225);
            this.ControlButton.Name = "ControlButton";
            this.ControlButton.Padding = new System.Windows.Forms.Padding(4);
            this.ControlButton.Size = new System.Drawing.Size(168, 37);
            this.ControlButton.TabIndex = 6;
            this.ControlButton.Text = "Запуск";
            this.ControlButton.UseVisualStyleBackColor = true;
            // 
            // AdditionalPanel
            // 
            this.AdditionalPanel.AutoSize = true;
            this.AdditionalPanel.ColumnCount = 1;
            this.AdditionalPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.AdditionalPanel.Controls.Add(label4, 0, 0);
            this.AdditionalPanel.Controls.Add(this.SamplerateBox, 0, 1);
            this.AdditionalPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.AdditionalPanel.Location = new System.Drawing.Point(0, 167);
            this.AdditionalPanel.Margin = new System.Windows.Forms.Padding(0);
            this.AdditionalPanel.Name = "AdditionalPanel";
            this.AdditionalPanel.RowCount = 2;
            this.AdditionalPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.AdditionalPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.AdditionalPanel.Size = new System.Drawing.Size(174, 55);
            this.AdditionalPanel.TabIndex = 7;
            // 
            // SamplerateBox
            // 
            this.SamplerateBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.SamplerateBox.Location = new System.Drawing.Point(3, 26);
            this.SamplerateBox.Maximum = new decimal(new int[] {
            1600000000,
            0,
            0,
            0});
            this.SamplerateBox.Name = "SamplerateBox";
            this.SamplerateBox.ReadOnly = true;
            this.SamplerateBox.Size = new System.Drawing.Size(168, 26);
            this.SamplerateBox.TabIndex = 6;
            this.SamplerateBox.ThousandsSeparator = true;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.SpectrPlot, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(406, 457);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // SpectrPlot
            // 
            this.SpectrPlot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SpectrPlot.Location = new System.Drawing.Point(0, 0);
            this.SpectrPlot.Margin = new System.Windows.Forms.Padding(0);
            this.SpectrPlot.Name = "SpectrPlot";
            this.SpectrPlot.Padding = new System.Windows.Forms.Padding(2);
            this.SpectrPlot.Size = new System.Drawing.Size(406, 182);
            this.SpectrPlot.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 461);
            this.Controls.Add(this.ContentPanel);
            this.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(600, 500);
            this.Name = "MainForm";
            this.Padding = new System.Windows.Forms.Padding(2);
            this.Text = "Scanner";
            this.ContentPanel.ResumeLayout(false);
            this.ContentPanel.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FreqBox)).EndInit();
            this.AdditionalPanel.ResumeLayout(false);
            this.AdditionalPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SamplerateBox)).EndInit();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.ResumeLayout(false);

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
    }
}