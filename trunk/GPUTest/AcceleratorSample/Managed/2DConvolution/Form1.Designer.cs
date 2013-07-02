namespace _2DConvolution
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
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.width15K = new System.Windows.Forms.RadioButton();
      this.width1K = new System.Windows.Forms.RadioButton();
      this.width500 = new System.Windows.Forms.RadioButton();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.iterativeTarget = new System.Windows.Forms.RadioButton();
      this.mcTarget = new System.Windows.Forms.RadioButton();
      this.dx9Target = new System.Windows.Forms.RadioButton();
      this.label1 = new System.Windows.Forms.Label();
      this.applyFilter = new System.Windows.Forms.Button();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // pictureBox1
      // 
      this.pictureBox1.Location = new System.Drawing.Point(253, 3);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(500, 500);
      this.pictureBox1.TabIndex = 0;
      this.pictureBox1.TabStop = false;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.width15K);
      this.groupBox1.Controls.Add(this.width1K);
      this.groupBox1.Controls.Add(this.width500);
      this.groupBox1.Location = new System.Drawing.Point(13, 13);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(140, 115);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Image Size";
      // 
      // width15K
      // 
      this.width15K.AutoSize = true;
      this.width15K.Location = new System.Drawing.Point(7, 76);
      this.width15K.Name = "width15K";
      this.width15K.Size = new System.Drawing.Size(107, 21);
      this.width15K.TabIndex = 0;
      this.width15K.Text = "1500 x 1500";
      this.width15K.UseVisualStyleBackColor = true;
      // 
      // width1K
      // 
      this.width1K.AutoSize = true;
      this.width1K.Location = new System.Drawing.Point(7, 49);
      this.width1K.Name = "width1K";
      this.width1K.Size = new System.Drawing.Size(107, 21);
      this.width1K.TabIndex = 0;
      this.width1K.Text = "1000 x 1000";
      this.width1K.UseVisualStyleBackColor = true;
      // 
      // width500
      // 
      this.width500.AutoSize = true;
      this.width500.Checked = true;
      this.width500.Location = new System.Drawing.Point(7, 22);
      this.width500.Name = "width500";
      this.width500.Size = new System.Drawing.Size(91, 21);
      this.width500.TabIndex = 0;
      this.width500.TabStop = true;
      this.width500.Text = "500 x 500";
      this.width500.UseVisualStyleBackColor = true;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.iterativeTarget);
      this.groupBox2.Controls.Add(this.mcTarget);
      this.groupBox2.Controls.Add(this.dx9Target);
      this.groupBox2.Location = new System.Drawing.Point(13, 134);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(140, 115);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Target";
      // 
      // iterativeTarget
      // 
      this.iterativeTarget.AutoSize = true;
      this.iterativeTarget.Location = new System.Drawing.Point(7, 76);
      this.iterativeTarget.Name = "iterativeTarget";
      this.iterativeTarget.Size = new System.Drawing.Size(79, 21);
      this.iterativeTarget.TabIndex = 0;
      this.iterativeTarget.Text = "Iterative";
      this.iterativeTarget.UseVisualStyleBackColor = true;
      // 
      // mcTarget
      // 
      this.mcTarget.AutoSize = true;
      this.mcTarget.Location = new System.Drawing.Point(7, 49);
      this.mcTarget.Name = "mcTarget";
      this.mcTarget.Size = new System.Drawing.Size(88, 21);
      this.mcTarget.TabIndex = 0;
      this.mcTarget.Text = "MultiCore";
      this.mcTarget.UseVisualStyleBackColor = true;
      // 
      // dx9Target
      // 
      this.dx9Target.AutoSize = true;
      this.dx9Target.Checked = true;
      this.dx9Target.Location = new System.Drawing.Point(7, 22);
      this.dx9Target.Name = "dx9Target";
      this.dx9Target.Size = new System.Drawing.Size(87, 21);
      this.dx9Target.TabIndex = 0;
      this.dx9Target.TabStop = true;
      this.dx9Target.Text = "DirectX 9";
      this.dx9Target.UseVisualStyleBackColor = true;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(20, 292);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(94, 17);
      this.label1.TabIndex = 2;
      this.label1.Text = "ElapsedTime:";
      // 
      // applyFilter
      // 
      this.applyFilter.Location = new System.Drawing.Point(23, 367);
      this.applyFilter.Name = "applyFilter";
      this.applyFilter.Size = new System.Drawing.Size(91, 27);
      this.applyFilter.TabIndex = 3;
      this.applyFilter.Text = "Apply Filter";
      this.applyFilter.UseVisualStyleBackColor = true;
      this.applyFilter.Click += new System.EventHandler(this.applyFilter_Click);
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1244, 643);
      this.Controls.Add(this.applyFilter);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.pictureBox1);
      this.Name = "Form1";
      this.Text = "Form1";
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.PictureBox pictureBox1;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.RadioButton width15K;
    private System.Windows.Forms.RadioButton width1K;
    private System.Windows.Forms.RadioButton width500;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.RadioButton iterativeTarget;
    private System.Windows.Forms.RadioButton mcTarget;
    private System.Windows.Forms.RadioButton dx9Target;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button applyFilter;
  }
}

