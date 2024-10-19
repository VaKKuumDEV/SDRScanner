namespace Scanner
{
    partial class RtlSdrForm
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
            TableLayoutPanel tableLayoutPanel4;
            Label label9;
            TableLayoutPanel tableLayoutPanel6;
            Label label1;
            TableLayoutPanel tableLayoutPanel10;
            DevicesBox = new ComboBox();
            GainBox = new ComboBox();
            ControlButton = new Button();
            tableLayoutPanel2 = new TableLayoutPanel();
            tableLayoutPanel4 = new TableLayoutPanel();
            label9 = new Label();
            tableLayoutPanel6 = new TableLayoutPanel();
            label1 = new Label();
            tableLayoutPanel10 = new TableLayoutPanel();
            tableLayoutPanel4.SuspendLayout();
            tableLayoutPanel6.SuspendLayout();
            tableLayoutPanel10.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
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
            tableLayoutPanel4.Size = new Size(374, 62);
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
            DevicesBox.Size = new Size(368, 30);
            DevicesBox.TabIndex = 0;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Dock = DockStyle.Top;
            label9.Location = new Point(3, 0);
            label9.Name = "label9";
            label9.Padding = new Padding(2);
            label9.Size = new Size(368, 26);
            label9.TabIndex = 1;
            label9.Text = "Устройство:";
            // 
            // tableLayoutPanel6
            // 
            tableLayoutPanel6.AutoSize = true;
            tableLayoutPanel6.ColumnCount = 1;
            tableLayoutPanel6.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel6.Controls.Add(GainBox, 0, 1);
            tableLayoutPanel6.Controls.Add(label1, 0, 0);
            tableLayoutPanel6.Dock = DockStyle.Top;
            tableLayoutPanel6.Location = new Point(0, 62);
            tableLayoutPanel6.Margin = new Padding(0);
            tableLayoutPanel6.Name = "tableLayoutPanel6";
            tableLayoutPanel6.RowCount = 2;
            tableLayoutPanel6.RowStyles.Add(new RowStyle());
            tableLayoutPanel6.RowStyles.Add(new RowStyle());
            tableLayoutPanel6.Size = new Size(374, 62);
            tableLayoutPanel6.TabIndex = 14;
            // 
            // GainBox
            // 
            GainBox.Dock = DockStyle.Top;
            GainBox.FormattingEnabled = true;
            GainBox.Location = new Point(3, 29);
            GainBox.Name = "GainBox";
            GainBox.Size = new Size(368, 30);
            GainBox.TabIndex = 3;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Top;
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Padding = new Padding(2);
            label1.Size = new Size(368, 26);
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
            tableLayoutPanel10.Location = new Point(0, 399);
            tableLayoutPanel10.Margin = new Padding(0);
            tableLayoutPanel10.Name = "tableLayoutPanel10";
            tableLayoutPanel10.RowCount = 1;
            tableLayoutPanel10.RowStyles.Add(new RowStyle());
            tableLayoutPanel10.RowStyles.Add(new RowStyle());
            tableLayoutPanel10.RowStyles.Add(new RowStyle());
            tableLayoutPanel10.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel10.Size = new Size(374, 46);
            tableLayoutPanel10.TabIndex = 18;
            // 
            // ControlButton
            // 
            ControlButton.AutoSize = true;
            ControlButton.Dock = DockStyle.Top;
            ControlButton.Location = new Point(3, 3);
            ControlButton.Name = "ControlButton";
            ControlButton.Padding = new Padding(4);
            ControlButton.Size = new Size(368, 40);
            ControlButton.TabIndex = 5;
            ControlButton.Text = "Добавить";
            ControlButton.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.AutoSize = true;
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(tableLayoutPanel4, 0, 0);
            tableLayoutPanel2.Controls.Add(tableLayoutPanel6, 0, 1);
            tableLayoutPanel2.Controls.Add(tableLayoutPanel10, 0, 3);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(4, 4);
            tableLayoutPanel2.Margin = new Padding(0);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 4;
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.Size = new Size(374, 445);
            tableLayoutPanel2.TabIndex = 1;
            // 
            // RtlSdrForm
            // 
            AutoScaleDimensions = new SizeF(11F, 22F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(382, 453);
            Controls.Add(tableLayoutPanel2);
            Font = new Font("Times New Roman", 12F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(6);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "RtlSdrForm";
            Padding = new Padding(4);
            ShowIcon = false;
            Text = "Выбор USB RTL";
            tableLayoutPanel4.ResumeLayout(false);
            tableLayoutPanel4.PerformLayout();
            tableLayoutPanel6.ResumeLayout(false);
            tableLayoutPanel6.PerformLayout();
            tableLayoutPanel10.ResumeLayout(false);
            tableLayoutPanel10.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel2;
        private ComboBox DevicesBox;
        private ComboBox GainBox;
        private Button ControlButton;
    }
}