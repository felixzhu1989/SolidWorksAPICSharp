
namespace Chapter5AssemblyAutomation
{
    partial class FrmAssemblyAutomation
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
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnAddComp = new System.Windows.Forms.Button();
            this.AssyTest = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(33, 54);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "1. Select a face";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(33, 83);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(123, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "2. Click Add component.";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(33, 111);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(90, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "3. Exit application";
            // 
            // btnAddComp
            // 
            this.btnAddComp.Location = new System.Drawing.Point(36, 145);
            this.btnAddComp.Name = "btnAddComp";
            this.btnAddComp.Size = new System.Drawing.Size(141, 36);
            this.btnAddComp.TabIndex = 1;
            this.btnAddComp.Text = "Add component";
            this.btnAddComp.UseVisualStyleBackColor = true;
            this.btnAddComp.Click += new System.EventHandler(this.btnAddComp_Click);
            // 
            // AssyTest
            // 
            this.AssyTest.Location = new System.Drawing.Point(38, 208);
            this.AssyTest.Name = "AssyTest";
            this.AssyTest.Size = new System.Drawing.Size(138, 39);
            this.AssyTest.TabIndex = 2;
            this.AssyTest.Text = "Add and Mate Example";
            this.AssyTest.UseVisualStyleBackColor = true;
            this.AssyTest.Visible = false;
            this.AssyTest.Click += new System.EventHandler(this.AssyTest_Click);
            // 
            // FrmAssemblyAutomation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(260, 200);
            this.Controls.Add(this.AssyTest);
            this.Controls.Add(this.btnAddComp);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "FrmAssemblyAutomation";
            this.Text = "Assembly Automation";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnAddComp;
        private System.Windows.Forms.Button AssyTest;
    }
}

