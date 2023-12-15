namespace Scanner
{
    partial class AddSignalForm
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
            SignalNameBox = new TextBox();
            AddButton = new Button();
            ContentPanel.SuspendLayout();
            SuspendLayout();
            // 
            // ContentPanel
            // 
            ContentPanel.AutoSize = true;
            ContentPanel.ColumnCount = 1;
            ContentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            ContentPanel.Controls.Add(SignalNameBox, 0, 0);
            ContentPanel.Controls.Add(AddButton, 0, 1);
            ContentPanel.Dock = DockStyle.Top;
            ContentPanel.Location = new Point(4, 4);
            ContentPanel.Margin = new Padding(0);
            ContentPanel.Name = "ContentPanel";
            ContentPanel.RowCount = 2;
            ContentPanel.RowStyles.Add(new RowStyle());
            ContentPanel.RowStyles.Add(new RowStyle());
            ContentPanel.Size = new Size(374, 82);
            ContentPanel.TabIndex = 0;
            // 
            // SignalNameBox
            // 
            SignalNameBox.Dock = DockStyle.Top;
            SignalNameBox.Location = new Point(3, 3);
            SignalNameBox.Name = "SignalNameBox";
            SignalNameBox.PlaceholderText = "Введите название сигнала";
            SignalNameBox.Size = new Size(368, 30);
            SignalNameBox.TabIndex = 0;
            // 
            // AddButton
            // 
            AddButton.AutoSize = true;
            AddButton.Dock = DockStyle.Top;
            AddButton.Location = new Point(3, 39);
            AddButton.Name = "AddButton";
            AddButton.Padding = new Padding(4);
            AddButton.Size = new Size(368, 40);
            AddButton.TabIndex = 1;
            AddButton.Text = "Добавить";
            AddButton.UseVisualStyleBackColor = true;
            AddButton.Click += AddButton_Click;
            // 
            // AddSignalForm
            // 
            AutoScaleDimensions = new SizeF(11F, 22F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(382, 92);
            Controls.Add(ContentPanel);
            Font = new Font("Times New Roman", 12F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AddSignalForm";
            Padding = new Padding(4);
            ShowIcon = false;
            Text = "Добавить сигнал";
            ContentPanel.ResumeLayout(false);
            ContentPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TableLayoutPanel ContentPanel;
        private TextBox SignalNameBox;
        private Button AddButton;
    }
}