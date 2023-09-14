namespace Scanner
{
    partial class SamplerForm
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
            ContentPanel = new TableLayoutPanel();
            tableLayoutPanel1 = new TableLayoutPanel();
            SerializeButton = new Button();
            SamplesBox = new ListBox();
            ChooseFilesDialog = new OpenFileDialog();
            ContentPanel.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // ContentPanel
            // 
            ContentPanel.ColumnCount = 2;
            ContentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            ContentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            ContentPanel.Controls.Add(tableLayoutPanel1, 1, 0);
            ContentPanel.Controls.Add(SamplesBox, 0, 0);
            ContentPanel.Dock = DockStyle.Fill;
            ContentPanel.Location = new Point(0, 0);
            ContentPanel.Margin = new Padding(0);
            ContentPanel.Name = "ContentPanel";
            ContentPanel.RowCount = 1;
            ContentPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            ContentPanel.Size = new Size(782, 453);
            ContentPanel.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(SerializeButton, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(547, 0);
            tableLayoutPanel1.Margin = new Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(235, 453);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // SerializeButton
            // 
            SerializeButton.AutoSize = true;
            SerializeButton.Dock = DockStyle.Top;
            SerializeButton.Location = new Point(3, 3);
            SerializeButton.Name = "SerializeButton";
            SerializeButton.Padding = new Padding(4);
            SerializeButton.Size = new Size(229, 40);
            SerializeButton.TabIndex = 0;
            SerializeButton.Text = "Добавить в список";
            SerializeButton.UseVisualStyleBackColor = true;
            SerializeButton.Click += SerializeButton_Click;
            // 
            // SamplesBox
            // 
            SamplesBox.Dock = DockStyle.Fill;
            SamplesBox.FormattingEnabled = true;
            SamplesBox.ItemHeight = 22;
            SamplesBox.Location = new Point(3, 3);
            SamplesBox.Name = "SamplesBox";
            SamplesBox.Size = new Size(541, 447);
            SamplesBox.TabIndex = 1;
            // 
            // ChooseFilesDialog
            // 
            ChooseFilesDialog.Filter = "MP3|*.mp3|WAVE|*.wav";
            ChooseFilesDialog.Multiselect = true;
            ChooseFilesDialog.Title = "Выберите аудиофайлы";
            ChooseFilesDialog.FileOk += ChooseFilesDialog_FileOk;
            // 
            // SamplerForm
            // 
            AutoScaleDimensions = new SizeF(11F, 22F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(782, 453);
            Controls.Add(ContentPanel);
            Font = new Font("Times New Roman", 12F, FontStyle.Regular, GraphicsUnit.Point);
            Margin = new Padding(4);
            MinimumSize = new Size(800, 500);
            Name = "SamplerForm";
            Text = "SamplerForm";
            ContentPanel.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel ContentPanel;
        private TableLayoutPanel tableLayoutPanel1;
        private Button SerializeButton;
        private ListBox SamplesBox;
        private OpenFileDialog ChooseFilesDialog;
    }
}