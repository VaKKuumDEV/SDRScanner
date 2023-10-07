namespace Scanner
{
    partial class NeuroForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            TableLayoutPanel tableLayoutPanel1;
            TableLayoutPanel tableLayoutPanel3;
            Label label1;
            TableLayoutPanel tableLayoutPanel2;
            TableLayoutPanel tableLayoutPanel4;
            SignalPlot = new ScottPlot.FormsPlot();
            PreparedSignalPlot = new ScottPlot.FormsPlot();
            HashTextbox = new TextBox();
            RecognizeButton = new Button();
            SignalsBaseButton = new Button();
            OpenSignalButton = new Button();
            HashListbox = new ListBox();
            ForwardButton = new Button();
            NextButton = new Button();
            ListingLabel = new Label();
            ContentPanel = new TableLayoutPanel();
            OpenSignalDialog = new OpenFileDialog();
            RecognizeAllButton = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel3 = new TableLayoutPanel();
            label1 = new Label();
            tableLayoutPanel2 = new TableLayoutPanel();
            tableLayoutPanel4 = new TableLayoutPanel();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel4.SuspendLayout();
            ContentPanel.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(SignalPlot, 0, 0);
            tableLayoutPanel1.Controls.Add(PreparedSignalPlot, 0, 1);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel3, 0, 2);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(541, 453);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // SignalPlot
            // 
            SignalPlot.Dock = DockStyle.Fill;
            SignalPlot.Location = new Point(0, 0);
            SignalPlot.Margin = new Padding(0);
            SignalPlot.Name = "SignalPlot";
            SignalPlot.Size = new Size(541, 208);
            SignalPlot.TabIndex = 0;
            // 
            // PreparedSignalPlot
            // 
            PreparedSignalPlot.Dock = DockStyle.Fill;
            PreparedSignalPlot.Location = new Point(0, 208);
            PreparedSignalPlot.Margin = new Padding(0);
            PreparedSignalPlot.Name = "PreparedSignalPlot";
            PreparedSignalPlot.Size = new Size(541, 208);
            PreparedSignalPlot.TabIndex = 1;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.AutoSize = true;
            tableLayoutPanel3.ColumnCount = 2;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Controls.Add(label1, 0, 0);
            tableLayoutPanel3.Controls.Add(HashTextbox, 1, 0);
            tableLayoutPanel3.Dock = DockStyle.Top;
            tableLayoutPanel3.Location = new Point(0, 416);
            tableLayoutPanel3.Margin = new Padding(0);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 1;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Size = new Size(541, 36);
            tableLayoutPanel3.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(0, 0);
            label1.Margin = new Padding(0);
            label1.Name = "label1";
            label1.Padding = new Padding(4, 4, 32, 4);
            label1.Size = new Size(89, 30);
            label1.TabIndex = 0;
            label1.Text = "Хэш:";
            label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // HashTextbox
            // 
            HashTextbox.Dock = DockStyle.Top;
            HashTextbox.Location = new Point(92, 3);
            HashTextbox.Name = "HashTextbox";
            HashTextbox.ReadOnly = true;
            HashTextbox.Size = new Size(446, 30);
            HashTextbox.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(RecognizeAllButton, 0, 3);
            tableLayoutPanel2.Controls.Add(RecognizeButton, 0, 2);
            tableLayoutPanel2.Controls.Add(SignalsBaseButton, 0, 1);
            tableLayoutPanel2.Controls.Add(OpenSignalButton, 0, 0);
            tableLayoutPanel2.Controls.Add(HashListbox, 0, 4);
            tableLayoutPanel2.Controls.Add(tableLayoutPanel4, 0, 5);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(541, 0);
            tableLayoutPanel2.Margin = new Padding(0);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 6;
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.Size = new Size(233, 453);
            tableLayoutPanel2.TabIndex = 1;
            // 
            // RecognizeButton
            // 
            RecognizeButton.AutoSize = true;
            RecognizeButton.Dock = DockStyle.Top;
            RecognizeButton.Location = new Point(3, 79);
            RecognizeButton.Name = "RecognizeButton";
            RecognizeButton.Size = new Size(227, 32);
            RecognizeButton.TabIndex = 4;
            RecognizeButton.Text = "Распознать сэмпл";
            RecognizeButton.UseVisualStyleBackColor = true;
            RecognizeButton.Click += RecognizeButton_Click;
            // 
            // SignalsBaseButton
            // 
            SignalsBaseButton.AutoSize = true;
            SignalsBaseButton.Dock = DockStyle.Top;
            SignalsBaseButton.Location = new Point(3, 41);
            SignalsBaseButton.Name = "SignalsBaseButton";
            SignalsBaseButton.Size = new Size(227, 32);
            SignalsBaseButton.TabIndex = 3;
            SignalsBaseButton.Text = "База сигналов";
            SignalsBaseButton.UseVisualStyleBackColor = true;
            SignalsBaseButton.Click += SignalsBaseButton_Click;
            // 
            // OpenSignalButton
            // 
            OpenSignalButton.AutoSize = true;
            OpenSignalButton.Dock = DockStyle.Top;
            OpenSignalButton.Location = new Point(3, 3);
            OpenSignalButton.Name = "OpenSignalButton";
            OpenSignalButton.Size = new Size(227, 32);
            OpenSignalButton.TabIndex = 0;
            OpenSignalButton.Text = "Выделить сигнал";
            OpenSignalButton.UseVisualStyleBackColor = true;
            OpenSignalButton.Click += OpenSignalButton_Click;
            // 
            // HashListbox
            // 
            HashListbox.Dock = DockStyle.Fill;
            HashListbox.FormattingEnabled = true;
            HashListbox.ItemHeight = 22;
            HashListbox.Location = new Point(3, 155);
            HashListbox.Name = "HashListbox";
            HashListbox.Size = new Size(227, 253);
            HashListbox.TabIndex = 1;
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.AutoSize = true;
            tableLayoutPanel4.ColumnCount = 3;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel4.Controls.Add(ForwardButton, 0, 0);
            tableLayoutPanel4.Controls.Add(NextButton, 2, 0);
            tableLayoutPanel4.Controls.Add(ListingLabel, 1, 0);
            tableLayoutPanel4.Dock = DockStyle.Top;
            tableLayoutPanel4.Location = new Point(0, 411);
            tableLayoutPanel4.Margin = new Padding(0);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 1;
            tableLayoutPanel4.RowStyles.Add(new RowStyle());
            tableLayoutPanel4.Size = new Size(233, 42);
            tableLayoutPanel4.TabIndex = 2;
            // 
            // ForwardButton
            // 
            ForwardButton.AutoSize = true;
            ForwardButton.Location = new Point(3, 3);
            ForwardButton.Name = "ForwardButton";
            ForwardButton.Padding = new Padding(2);
            ForwardButton.Size = new Size(36, 36);
            ForwardButton.TabIndex = 0;
            ForwardButton.Text = "<";
            ForwardButton.UseVisualStyleBackColor = true;
            ForwardButton.Click += ForwardButton_Click;
            // 
            // NextButton
            // 
            NextButton.AutoSize = true;
            NextButton.Location = new Point(194, 3);
            NextButton.Name = "NextButton";
            NextButton.Padding = new Padding(2);
            NextButton.Size = new Size(36, 36);
            NextButton.TabIndex = 1;
            NextButton.Text = ">";
            NextButton.UseVisualStyleBackColor = true;
            NextButton.Click += NextButton_Click;
            // 
            // ListingLabel
            // 
            ListingLabel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            ListingLabel.AutoSize = true;
            ListingLabel.Location = new Point(42, 0);
            ListingLabel.Margin = new Padding(0);
            ListingLabel.Name = "ListingLabel";
            ListingLabel.Padding = new Padding(6);
            ListingLabel.Size = new Size(149, 42);
            ListingLabel.TabIndex = 2;
            ListingLabel.Text = "0/0";
            ListingLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // ContentPanel
            // 
            ContentPanel.ColumnCount = 2;
            ContentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            ContentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            ContentPanel.Controls.Add(tableLayoutPanel1, 0, 0);
            ContentPanel.Controls.Add(tableLayoutPanel2, 1, 0);
            ContentPanel.Dock = DockStyle.Fill;
            ContentPanel.Location = new Point(4, 4);
            ContentPanel.Margin = new Padding(0);
            ContentPanel.Name = "ContentPanel";
            ContentPanel.RowCount = 1;
            ContentPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            ContentPanel.Size = new Size(774, 453);
            ContentPanel.TabIndex = 0;
            // 
            // OpenSignalDialog
            // 
            OpenSignalDialog.Title = "Выберите файл аудиозаписи";
            OpenSignalDialog.FileOk += OpenSignalDialog_FileOk;
            // 
            // RecognizeAllButton
            // 
            RecognizeAllButton.AutoSize = true;
            RecognizeAllButton.Dock = DockStyle.Top;
            RecognizeAllButton.Location = new Point(3, 117);
            RecognizeAllButton.Name = "RecognizeAllButton";
            RecognizeAllButton.Size = new Size(227, 32);
            RecognizeAllButton.TabIndex = 5;
            RecognizeAllButton.Text = "Распознать полностью";
            RecognizeAllButton.UseVisualStyleBackColor = true;
            RecognizeAllButton.Click += RecognizeAllButton_Click;
            // 
            // NeuroForm
            // 
            AutoScaleDimensions = new SizeF(11F, 22F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(782, 461);
            Controls.Add(ContentPanel);
            Font = new Font("Times New Roman", 12F, FontStyle.Regular, GraphicsUnit.Point);
            Margin = new Padding(4);
            MinimumSize = new Size(800, 500);
            Name = "NeuroForm";
            Padding = new Padding(4);
            Text = "NeuroForm";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            tableLayoutPanel4.ResumeLayout(false);
            tableLayoutPanel4.PerformLayout();
            ContentPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel ContentPanel;
        private ScottPlot.FormsPlot SignalPlot;
        private ScottPlot.FormsPlot PreparedSignalPlot;
        private Button OpenSignalButton;
        private OpenFileDialog OpenSignalDialog;
        private ListBox HashListbox;
        private TextBox HashTextbox;
        private Button ForwardButton;
        private Button NextButton;
        private Label ListingLabel;
        private Button SignalsBaseButton;
        private Button RecognizeButton;
        private Button RecognizeAllButton;
    }
}