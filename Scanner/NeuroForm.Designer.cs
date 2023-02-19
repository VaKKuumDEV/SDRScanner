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
            System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
            System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
            this.SignalPlot = new ScottPlot.FormsPlot();
            this.PreparedSignalPlot = new ScottPlot.FormsPlot();
            this.OpenSignalButton = new System.Windows.Forms.Button();
            this.ContentPanel = new System.Windows.Forms.TableLayoutPanel();
            this.OpenSignalDialog = new System.Windows.Forms.OpenFileDialog();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            this.ContentPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(this.SignalPlot, 0, 0);
            tableLayoutPanel1.Controls.Add(this.PreparedSignalPlot, 0, 1);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.Size = new System.Drawing.Size(403, 453);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // SignalPlot
            // 
            this.SignalPlot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SignalPlot.Location = new System.Drawing.Point(0, 0);
            this.SignalPlot.Margin = new System.Windows.Forms.Padding(0);
            this.SignalPlot.Name = "SignalPlot";
            this.SignalPlot.Size = new System.Drawing.Size(403, 226);
            this.SignalPlot.TabIndex = 0;
            // 
            // PreparedSignalPlot
            // 
            this.PreparedSignalPlot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PreparedSignalPlot.Location = new System.Drawing.Point(0, 226);
            this.PreparedSignalPlot.Margin = new System.Windows.Forms.Padding(0);
            this.PreparedSignalPlot.Name = "PreparedSignalPlot";
            this.PreparedSignalPlot.Size = new System.Drawing.Size(403, 227);
            this.PreparedSignalPlot.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.AutoSize = true;
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(this.OpenSignalButton, 0, 0);
            tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Top;
            tableLayoutPanel2.Location = new System.Drawing.Point(403, 0);
            tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            tableLayoutPanel2.Size = new System.Drawing.Size(173, 35);
            tableLayoutPanel2.TabIndex = 1;
            // 
            // OpenSignalButton
            // 
            this.OpenSignalButton.AutoSize = true;
            this.OpenSignalButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.OpenSignalButton.Location = new System.Drawing.Point(3, 3);
            this.OpenSignalButton.Name = "OpenSignalButton";
            this.OpenSignalButton.Size = new System.Drawing.Size(167, 29);
            this.OpenSignalButton.TabIndex = 0;
            this.OpenSignalButton.Text = "Выделить сигнал";
            this.OpenSignalButton.UseVisualStyleBackColor = true;
            this.OpenSignalButton.Click += new System.EventHandler(this.OpenSignalButton_Click);
            // 
            // ContentPanel
            // 
            this.ContentPanel.ColumnCount = 2;
            this.ContentPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.ContentPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.ContentPanel.Controls.Add(tableLayoutPanel1, 0, 0);
            this.ContentPanel.Controls.Add(tableLayoutPanel2, 1, 0);
            this.ContentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ContentPanel.Location = new System.Drawing.Point(4, 4);
            this.ContentPanel.Margin = new System.Windows.Forms.Padding(0);
            this.ContentPanel.Name = "ContentPanel";
            this.ContentPanel.RowCount = 1;
            this.ContentPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ContentPanel.Size = new System.Drawing.Size(576, 453);
            this.ContentPanel.TabIndex = 0;
            // 
            // OpenSignalDialog
            // 
            this.OpenSignalDialog.Title = "Выберите файл аудиозаписи";
            this.OpenSignalDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.OpenSignalDialog_FileOk);
            // 
            // NeuroForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 461);
            this.Controls.Add(this.ContentPanel);
            this.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(600, 500);
            this.Name = "NeuroForm";
            this.Padding = new System.Windows.Forms.Padding(4);
            this.Text = "NeuroForm";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            this.ContentPanel.ResumeLayout(false);
            this.ContentPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private TableLayoutPanel ContentPanel;
        private ScottPlot.FormsPlot SignalPlot;
        private ScottPlot.FormsPlot PreparedSignalPlot;
        private Button OpenSignalButton;
        private OpenFileDialog OpenSignalDialog;
    }
}