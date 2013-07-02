namespace Boxcar
{
  partial class Form1
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
      this.graph1 = new System.Windows.Forms.PictureBox();
      this.btnBoxcarApply = new System.Windows.Forms.Button();
      this.graph2 = new System.Windows.Forms.PictureBox();
      this.label2 = new System.Windows.Forms.Label();
      this.time = new System.Windows.Forms.Label();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.targetIterative = new System.Windows.Forms.RadioButton();
      this.targetMC = new System.Windows.Forms.RadioButton();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.num1M = new System.Windows.Forms.RadioButton();
      this.num100K = new System.Windows.Forms.RadioButton();
      this.num4K = new System.Windows.Forms.RadioButton();
      ((System.ComponentModel.ISupportInitialize)(this.graph1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.graph2)).BeginInit();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // graph1
      // 
      this.graph1.BackColor = System.Drawing.SystemColors.Window;
      this.graph1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.graph1.Location = new System.Drawing.Point(33, 38);
      this.graph1.Margin = new System.Windows.Forms.Padding(4);
      this.graph1.Name = "graph1";
      this.graph1.Size = new System.Drawing.Size(1333, 123);
      this.graph1.TabIndex = 0;
      this.graph1.TabStop = false;
      // 
      // btnBoxcarApply
      // 
      this.btnBoxcarApply.Location = new System.Drawing.Point(409, 386);
      this.btnBoxcarApply.Margin = new System.Windows.Forms.Padding(4);
      this.btnBoxcarApply.Name = "btnBoxcarApply";
      this.btnBoxcarApply.Size = new System.Drawing.Size(100, 28);
      this.btnBoxcarApply.TabIndex = 3;
      this.btnBoxcarApply.Text = "Apply Filter";
      this.btnBoxcarApply.UseVisualStyleBackColor = true;
      // 
      // graph2
      // 
      this.graph2.BackColor = System.Drawing.SystemColors.Window;
      this.graph2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.graph2.Location = new System.Drawing.Point(33, 240);
      this.graph2.Margin = new System.Windows.Forms.Padding(4);
      this.graph2.Name = "graph2";
      this.graph2.Size = new System.Drawing.Size(1333, 123);
      this.graph2.TabIndex = 0;
      this.graph2.TabStop = false;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(33, 588);
      this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(0, 17);
      this.label2.TabIndex = 5;
      // 
      // time
      // 
      this.time.AutoSize = true;
      this.time.Location = new System.Drawing.Point(414, 435);
      this.time.Name = "time";
      this.time.Size = new System.Drawing.Size(98, 17);
      this.time.TabIndex = 6;
      this.time.Text = "Elapsed Time:";
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.targetIterative);
      this.groupBox1.Controls.Add(this.targetMC);
      this.groupBox1.Location = new System.Drawing.Point(239, 386);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(144, 124);
      this.groupBox1.TabIndex = 7;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Target";
      // 
      // targetIterative
      // 
      this.targetIterative.AutoSize = true;
      this.targetIterative.Checked = true;
      this.targetIterative.Location = new System.Drawing.Point(7, 62);
      this.targetIterative.Name = "targetIterative";
      this.targetIterative.Size = new System.Drawing.Size(79, 21);
      this.targetIterative.TabIndex = 2;
      this.targetIterative.TabStop = true;
      this.targetIterative.Text = "Iterative";
      this.targetIterative.UseVisualStyleBackColor = true;
      // 
      // targetMC
      // 
      this.targetMC.AutoSize = true;
      this.targetMC.Location = new System.Drawing.Point(7, 34);
      this.targetMC.Name = "targetMC";
      this.targetMC.Size = new System.Drawing.Size(93, 21);
      this.targetMC.TabIndex = 1;
      this.targetMC.Text = "Multi-Core";
      this.targetMC.UseVisualStyleBackColor = true;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.num1M);
      this.groupBox2.Controls.Add(this.num100K);
      this.groupBox2.Controls.Add(this.num4K);
      this.groupBox2.Location = new System.Drawing.Point(45, 386);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(157, 124);
      this.groupBox2.TabIndex = 9;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Number of Elements";
      // 
      // num1M
      // 
      this.num1M.AutoSize = true;
      this.num1M.Location = new System.Drawing.Point(7, 94);
      this.num1M.Name = "num1M";
      this.num1M.Size = new System.Drawing.Size(93, 21);
      this.num1M.TabIndex = 2;
      this.num1M.Text = "1,000,000";
      this.num1M.UseVisualStyleBackColor = true;
      // 
      // num100K
      // 
      this.num100K.AutoSize = true;
      this.num100K.Location = new System.Drawing.Point(7, 62);
      this.num100K.Name = "num100K";
      this.num100K.Size = new System.Drawing.Size(81, 21);
      this.num100K.TabIndex = 1;
      this.num100K.Text = "100,000";
      this.num100K.UseVisualStyleBackColor = true;
      // 
      // num4K
      // 
      this.num4K.AutoSize = true;
      this.num4K.Checked = true;
      this.num4K.Location = new System.Drawing.Point(7, 34);
      this.num4K.Name = "num4K";
      this.num4K.Size = new System.Drawing.Size(61, 21);
      this.num4K.TabIndex = 0;
      this.num4K.TabStop = true;
      this.num4K.Text = "4000";
      this.num4K.UseVisualStyleBackColor = true;
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1397, 535);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.time);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.btnBoxcarApply);
      this.Controls.Add(this.graph2);
      this.Controls.Add(this.graph1);
      this.Margin = new System.Windows.Forms.Padding(4);
      this.Name = "Form1";
      this.Text = "Form1";
      ((System.ComponentModel.ISupportInitialize)(this.graph1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.graph2)).EndInit();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.PictureBox graph1;
      private System.Windows.Forms.Button btnBoxcarApply;
      private System.Windows.Forms.PictureBox graph2;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label time;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.RadioButton targetMC;
    private System.Windows.Forms.RadioButton targetIterative;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.RadioButton num1M;
    private System.Windows.Forms.RadioButton num100K;
    private System.Windows.Forms.RadioButton num4K;
  }
}

