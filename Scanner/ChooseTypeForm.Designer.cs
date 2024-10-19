namespace Scanner
{
    partial class ChooseTypeForm
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
            TcpButton = new Button();
            UsbButton = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(TcpButton, 0, 2);
            tableLayoutPanel1.Controls.Add(UsbButton, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(4, 4);
            tableLayoutPanel1.Margin = new Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Size = new Size(374, 445);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // TcpButton
            // 
            TcpButton.AutoSize = true;
            TcpButton.Dock = DockStyle.Top;
            TcpButton.Location = new Point(0, 222);
            TcpButton.Margin = new Padding(0);
            TcpButton.Name = "TcpButton";
            TcpButton.Padding = new Padding(4);
            TcpButton.Size = new Size(374, 40);
            TcpButton.TabIndex = 1;
            TcpButton.Text = "TCP RTL SDR";
            TcpButton.UseVisualStyleBackColor = true;
            // 
            // UsbButton
            // 
            UsbButton.AutoSize = true;
            UsbButton.Dock = DockStyle.Top;
            UsbButton.Location = new Point(0, 182);
            UsbButton.Margin = new Padding(0);
            UsbButton.Name = "UsbButton";
            UsbButton.Padding = new Padding(4);
            UsbButton.Size = new Size(374, 40);
            UsbButton.TabIndex = 0;
            UsbButton.Text = "USB RTL SDR";
            UsbButton.UseVisualStyleBackColor = true;
            // 
            // ChooseTypeForm
            // 
            AutoScaleDimensions = new SizeF(11F, 22F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(382, 453);
            Controls.Add(tableLayoutPanel1);
            Font = new Font("Times New Roman", 12F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ChooseTypeForm";
            Padding = new Padding(4);
            ShowIcon = false;
            Text = "Выбор типа источника";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button UsbButton;
        private Button TcpButton;
    }
}