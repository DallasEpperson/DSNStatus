namespace DSNStatusTester
{
    partial class frmTester
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
            this.timePicker = new System.Windows.Forms.DateTimePicker();
            this.btnSpecified = new System.Windows.Forms.Button();
            this.btnNow = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // timePicker
            // 
            this.timePicker.CustomFormat = "yyyy.MM.dd - HH:mm:ss";
            this.timePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.timePicker.Location = new System.Drawing.Point(9, 10);
            this.timePicker.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.timePicker.Name = "timePicker";
            this.timePicker.Size = new System.Drawing.Size(131, 20);
            this.timePicker.TabIndex = 0;
            this.timePicker.TabStop = false;
            this.timePicker.Value = new System.DateTime(2014, 5, 8, 15, 12, 33, 0);
            // 
            // btnSpecified
            // 
            this.btnSpecified.Location = new System.Drawing.Point(143, 10);
            this.btnSpecified.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnSpecified.Name = "btnSpecified";
            this.btnSpecified.Size = new System.Drawing.Size(110, 20);
            this.btnSpecified.TabIndex = 1;
            this.btnSpecified.Text = "Get Specified";
            this.btnSpecified.UseVisualStyleBackColor = true;
            this.btnSpecified.Click += new System.EventHandler(this.btnSpecified_Click);
            // 
            // btnNow
            // 
            this.btnNow.Location = new System.Drawing.Point(104, 32);
            this.btnNow.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnNow.Name = "btnNow";
            this.btnNow.Size = new System.Drawing.Size(56, 19);
            this.btnNow.TabIndex = 0;
            this.btnNow.Text = "Get Now";
            this.btnNow.UseVisualStyleBackColor = true;
            this.btnNow.Click += new System.EventHandler(this.btnNow_Click);
            // 
            // frmTester
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(262, 60);
            this.Controls.Add(this.btnNow);
            this.Controls.Add(this.btnSpecified);
            this.Controls.Add(this.timePicker);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "frmTester";
            this.Text = "Deep Space Network Status Poller";
            this.Load += new System.EventHandler(this.frmTester_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DateTimePicker timePicker;
        private System.Windows.Forms.Button btnSpecified;
        private System.Windows.Forms.Button btnNow;
    }
}

