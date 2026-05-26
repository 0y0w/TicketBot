using System;

namespace TicketBot
{
    partial class Form1
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.txtActivityId = new System.Windows.Forms.TextBox();
            this.txtPreference = new System.Windows.Forms.TextBox();
            this.txtQuantity = new System.Windows.Forms.TextBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.rtbLog = new System.Windows.Forms.RichTextBox();
            this.txtBlacklist = new System.Windows.Forms.TextBox();
            this.btnPause = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbPlatform = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // txtActivityId
            // 
            this.txtActivityId.Location = new System.Drawing.Point(21, 65);
            this.txtActivityId.Name = "txtActivityId";
            this.txtActivityId.Size = new System.Drawing.Size(90, 22);
            this.txtActivityId.TabIndex = 0;
            // 
            // txtPreference
            // 
            this.txtPreference.Location = new System.Drawing.Point(21, 165);
            this.txtPreference.Name = "txtPreference";
            this.txtPreference.Size = new System.Drawing.Size(186, 22);
            this.txtPreference.TabIndex = 1;
            // 
            // txtQuantity
            // 
            this.txtQuantity.Location = new System.Drawing.Point(21, 115);
            this.txtQuantity.Name = "txtQuantity";
            this.txtQuantity.Size = new System.Drawing.Size(90, 22);
            this.txtQuantity.TabIndex = 1;
            // 
            // btnStart
            // 
            this.btnStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.btnStart.Location = new System.Drawing.Point(21, 327);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(107, 33);
            this.btnStart.TabIndex = 2;
            this.btnStart.Text = "start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.btnStop.Location = new System.Drawing.Point(134, 327);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(107, 33);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // rtbLog
            // 
            this.rtbLog.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.rtbLog.Location = new System.Drawing.Point(21, 366);
            this.rtbLog.Name = "rtbLog";
            this.rtbLog.ReadOnly = true;
            this.rtbLog.Size = new System.Drawing.Size(320, 44);
            this.rtbLog.TabIndex = 3;
            this.rtbLog.Text = "";
            // 
            // txtBlacklist
            // 
            this.txtBlacklist.Location = new System.Drawing.Point(21, 215);
            this.txtBlacklist.Name = "txtBlacklist";
            this.txtBlacklist.Size = new System.Drawing.Size(186, 22);
            this.txtBlacklist.TabIndex = 1;
            // 
            // btnPause
            // 
            this.btnPause.Location = new System.Drawing.Point(21, 243);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(90, 27);
            this.btnPause.TabIndex = 4;
            this.btnPause.Text = "開始搶票";
            this.btnPause.UseVisualStyleBackColor = true;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 5;
            this.label1.Text = "活動代碼";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 100);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "購票數量";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 150);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "選區白名單";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(19, 200);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 8;
            this.label4.Text = "選區黑名單";
            // 
            // cmbPlatform
            // 
            this.cmbPlatform.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPlatform.FormattingEnabled = true;
            this.cmbPlatform.ItemHeight = 12;
            this.cmbPlatform.Items.AddRange(new object[] {
            "拓元購票 TixCraft",
            "KKTix"});
            this.cmbPlatform.Location = new System.Drawing.Point(21, 12);
            this.cmbPlatform.Name = "cmbPlatform";
            this.cmbPlatform.Size = new System.Drawing.Size(135, 20);
            this.cmbPlatform.TabIndex = 9;
            this.cmbPlatform.SelectedIndexChanged += new System.EventHandler(this.cmbPlatform_SelectedIndexChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(364, 427);
            this.Controls.Add(this.cmbPlatform);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnPause);
            this.Controls.Add(this.rtbLog);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.txtQuantity);
            this.Controls.Add(this.txtBlacklist);
            this.Controls.Add(this.txtPreference);
            this.Controls.Add(this.txtActivityId);
            this.Name = "Form1";
            this.Text = "ticket bot";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.TextBox txtActivityId;
        public System.Windows.Forms.TextBox txtPreference;
        public System.Windows.Forms.TextBox txtQuantity;
        public System.Windows.Forms.Button btnStart;
        public System.Windows.Forms.Button btnStop;
        public System.Windows.Forms.RichTextBox rtbLog;
        public System.Windows.Forms.TextBox txtBlacklist;
        public System.Windows.Forms.Button btnPause;
        public System.Windows.Forms.ComboBox cmbPlatform;
    }

}

