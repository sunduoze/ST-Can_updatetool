namespace CJQTest
{
    partial class CTool
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.btnMeter97 = new System.Windows.Forms.Button();
            this.btnMeter2007 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnMeter97
            // 
            this.btnMeter97.Location = new System.Drawing.Point(54, 36);
            this.btnMeter97.Name = "btnMeter97";
            this.btnMeter97.Size = new System.Drawing.Size(112, 23);
            this.btnMeter97.TabIndex = 0;
            this.btnMeter97.Text = "国标97模拟表柜";
            this.btnMeter97.UseVisualStyleBackColor = true;
            this.btnMeter97.Click += new System.EventHandler(this.btnMeter97_Click);
            // 
            // btnMeter2007
            // 
            this.btnMeter2007.Location = new System.Drawing.Point(54, 116);
            this.btnMeter2007.Name = "btnMeter2007";
            this.btnMeter2007.Size = new System.Drawing.Size(112, 23);
            this.btnMeter2007.TabIndex = 1;
            this.btnMeter2007.Text = "国标2007模拟表柜";
            this.btnMeter2007.UseVisualStyleBackColor = true;
            this.btnMeter2007.Click += new System.EventHandler(this.btnMeter2007_Click);
            // 
            // CTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Controls.Add(this.btnMeter2007);
            this.Controls.Add(this.btnMeter97);
            this.Name = "CTool";
            this.Text = "工具";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnMeter97;
        private System.Windows.Forms.Button btnMeter2007;
    }
}