using System;
using System.Windows.Forms;
using System.ComponentModel;

namespace Life
{
  public partial class Form1 : Form
  {
    Life lifeDemo;
    private Timer updateTimer;
    IContainer timerComponents;

    public Form1()
    {
      InitializeComponent();
      lifeDemo = new Life(pictureBox1.Width, pictureBox1);
      this.Text = "Game of Life";
    }

    private void startButton_Click(object sender, EventArgs e)
    {
      TargetType evalTarget;
      if (dx9Target.Checked)
        evalTarget = TargetType.DX9;
      else
        evalTarget = TargetType.MC;
      lifeDemo.Init(evalTarget);
      timerComponents = new Container();
      updateTimer = new Timer(this.timerComponents);
      updateTimer.Enabled = true;
      updateTimer.Interval = 500;
      updateTimer.Tick += new EventHandler(updateTimer_Tick);
    }

    void updateTimer_Tick(object sender, EventArgs e)
    {
      lifeDemo.ComputeNextState();
    }

    private void stopButton_Click(object sender, EventArgs e)
    {
      updateTimer.Stop();
    }
  }
  public enum TargetType
  {
    DX9,
    MC,
    Iterative
  }
}
