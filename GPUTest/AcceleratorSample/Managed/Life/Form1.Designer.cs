namespace Life
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
      this.startButton = new System.Windows.Forms.Button();
      this.stopButton = new System.Windows.Forms.Button();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.dx9Target = new System.Windows.Forms.RadioButton();
      this.mcTarget = new System.Windows.Forms.RadioButton();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // pictureBox1
      // 
      this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.pictureBox1.Location = new System.Drawing.Point(17, 16);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(512, 512);
      this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBox1.TabIndex = 0;
      this.pictureBox1.TabStop = false;
      // 
      // startButton
      // 
      this.startButton.Location = new System.Drawing.Point(423, 544);
      this.startButton.Name = "startButton";
      this.startButton.Size = new System.Drawing.Size(87, 25);
      this.startButton.TabIndex = 1;
      this.startButton.Text = "Start";
      this.startButton.UseVisualStyleBackColor = true;
      this.startButton.Click += new System.EventHandler(this.startButton_Click);
      // 
      // stopButton
      // 
      this.stopButton.Location = new System.Drawing.Point(423, 584);
      this.stopButton.Name = "stopButton";
      this.stopButton.Size = new System.Drawing.Size(87, 27);
      this.stopButton.TabIndex = 2;
      this.stopButton.Text = "Stop";
      this.stopButton.UseVisualStyleBackColor = true;
      this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.dx9Target);
      this.groupBox1.Controls.Add(this.mcTarget);
      this.groupBox1.Location = new System.Drawing.Point(17, 544);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(101, 73);
      this.groupBox1.TabIndex = 3;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Targets";
      // 
      // dx9Target
      // 
      this.dx9Target.AutoSize = true;
      this.dx9Target.Checked = true;
      this.dx9Target.Location = new System.Drawing.Point(7, 21);
      this.dx9Target.Name = "dx9Target";
      this.dx9Target.Size = new System.Drawing.Size(87, 21);
      this.dx9Target.TabIndex = 0;
      this.dx9Target.TabStop = true;
      this.dx9Target.Text = "DirectX 9";
      this.dx9Target.UseVisualStyleBackColor = true;
      // 
      // mcTarget
      // 
      this.mcTarget.AutoSize = true;
      this.mcTarget.Location = new System.Drawing.Point(6, 48);
      this.mcTarget.Name = "mcTarget";
      this.mcTarget.Size = new System.Drawing.Size(86, 21);
      this.mcTarget.TabIndex = 0;
      this.mcTarget.TabStop = true;
      this.mcTarget.Text = "Multicore";
      this.mcTarget.UseVisualStyleBackColor = true;
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(579, 616);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.stopButton);
      this.Controls.Add(this.startButton);
      this.Controls.Add(this.pictureBox1);
      this.Name = "Form1";
      this.Text = "Form1";
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.PictureBox pictureBox1;
    private System.Windows.Forms.Button startButton;
    private System.Windows.Forms.Button stopButton;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.RadioButton dx9Target;
    private System.Windows.Forms.RadioButton mcTarget;
  }
}

