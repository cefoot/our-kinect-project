using System;
using System.Windows.Forms;


namespace _2DConvolution
{
  public partial class Form1 : Form
  {
    Convolve convolver;
    int imageWidth, imageHeight;

    public Form1()
    {
      InitializeComponent();
      convolver = new Convolve(pictureBox1);
      convolver.Init();

      //Click handlers
      width500.Click += new EventHandler(width_Click);
      width1K.Click += new EventHandler(width_Click);
      width15K.Click += new EventHandler(width_Click);
      dx9Target.Click += new EventHandler(target_Click);
      mcTarget.Click+=new EventHandler(target_Click);
      iterativeTarget.Click+=new EventHandler(target_Click);
    }

    private void width_Click(object sender, EventArgs e)
    {
      if (width1K.Checked)
      {
        pictureBox1.Width = 1000;
        pictureBox1.Height = 1000;
      }
      else if (width15K.Checked)
      {
        pictureBox1.Width = 1500;
        pictureBox1.Height = 1500;
      }
      else
      {
        pictureBox1.Width = 500;
        pictureBox1.Height = 500;
      }
      convolver.Init();
    }

    private void target_Click(object sender, EventArgs e)
    {
      convolver.Init();
    }

    private void applyFilter_Click(object sender, EventArgs e)
    {
      long elapsedTime;
      TargetType evalTarget;

      if (dx9Target.Checked)
        evalTarget = TargetType.DX9;
      else if (mcTarget.Checked)
        evalTarget = TargetType.MC;
      else
        evalTarget = TargetType.Iterative;

      elapsedTime = convolver.Run(evalTarget);
      label1.Text = "Elapsed Time:" + elapsedTime.ToString();
    }
  }

  public enum TargetType
  {
    DX9,
    MC,
    Iterative
  }
}
