namespace Scanner
{
    partial class SelectSignalForm
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
            SignalsBox = new ListBox();
            AbbSignalButon = new Button();
            ContentPanel.SuspendLayout();
            SuspendLayout();
            // 
            // ContentPanel
            // 
            ContentPanel.ColumnCount = 1;
            ContentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            ContentPanel.Controls.Add(SignalsBox, 0, 0);
            ContentPanel.Controls.Add(AbbSignalButon, 0, 1);
            ContentPanel.Dock = DockStyle.Fill;
            ContentPanel.Location = new Point(4, 4);
            ContentPanel.Name = "ContentPanel";
            ContentPanel.RowCount = 2;
            ContentPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            ContentPanel.RowStyles.Add(new RowStyle());
            ContentPanel.Size = new Size(374, 445);
            ContentPanel.TabIndex = 0;
            // 
            // SignalsBox
            // 
            SignalsBox.Dock = DockStyle.Fill;
            SignalsBox.FormattingEnabled = true;
            SignalsBox.ItemHeight = 22;
            SignalsBox.Location = new Point(3, 3);
            SignalsBox.Name = "SignalsBox";
            SignalsBox.Size = new Size(368, 393);
            SignalsBox.TabIndex = 0;
            // 
            // AbbSignalButon
            // 
            AbbSignalButon.AutoSize = true;
            AbbSignalButon.Dock = DockStyle.Top;
            AbbSignalButon.Location = new Point(3, 402);
            AbbSignalButon.Name = "AbbSignalButon";
            AbbSignalButon.Padding = new Padding(4);
            AbbSignalButon.Size = new Size(368, 40);
            AbbSignalButon.TabIndex = 1;
            AbbSignalButon.Text = "Добавить сигнал";
            AbbSignalButon.UseVisualStyleBackColor = true;
            AbbSignalButon.Click += AbbSignalButon_Click;
            // 
            // SelectSignalForm
            // 
            AutoScaleDimensions = new SizeF(11F, 22F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(382, 453);
            Controls.Add(ContentPanel);
            Font = new Font("Times New Roman", 12F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SelectSignalForm";
            Padding = new Padding(4);
            ShowIcon = false;
            Text = "Выберите сигнал";
            FormClosed += SelectSignalForm_FormClosed;
            ContentPanel.ResumeLayout(false);
            ContentPanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel ContentPanel;
        private ListBox SignalsBox;
        private Button AbbSignalButon;
    }
}