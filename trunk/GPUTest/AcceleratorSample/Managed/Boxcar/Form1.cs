using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

using Microsoft.ParallelArrays;
using PA = Microsoft.ParallelArrays.ParallelArrays;
using FPA = Microsoft.ParallelArrays.FloatParallelArray;

namespace Boxcar
{
  public partial class Form1 : Form
  {
    Size imageSize;
    BoxCarDemo boxDemo;
    Bitmap imageBitmap1;
    int numElements;
    int filterWidth;

    public Form1()
    {
      InitializeComponent();
      btnBoxcarApply.Click += new EventHandler(btnBoxcarApply_Click);
      this.Text = "Boxcar Demo";
      num4K.Click += new EventHandler(num_Click);
      num100K.Click += new EventHandler(num_Click);
      num1M.Click += new EventHandler(num_Click);
      targetMC.Click +=new EventHandler(num_Click);
      targetIterative.Click += new EventHandler(num_Click);
      filterWidth = 21;

      boxDemo = new BoxCarDemo(graph1);
      numElements = 4000;
      boxDemo.Init(numElements);
      imageSize = new Size(graph1.Width, graph1.Height);
      imageBitmap1 = boxDemo.CreateGraph(imageSize.Width, imageSize.Height, SeriesType.OriginalSeries);
      graph1.Image = imageBitmap1;
    }

    //update display of original series when anything changes
    void num_Click(object sender, EventArgs e)
    {
      if (num4K.Checked)
      {
        numElements = 4000;
        filterWidth = 25;
      }
      else if (num100K.Checked)
      {
        numElements = 100000;
        filterWidth = 151; //use a larger filter value for longer input arrays
      }
      else
      {
        numElements = 1000000;
        filterWidth = 151;
      }
      boxDemo.Init(numElements);
      imageSize = new Size(graph1.Width, graph1.Height);
      imageBitmap1 = boxDemo.CreateGraph(imageSize.Width, imageSize.Height, SeriesType.OriginalSeries);
      graph1.Image = imageBitmap1;
    }

    //Apply the selected filter to the starting time series
    private void btnBoxcarApply_Click(object sender, EventArgs e)
    {
      long elapsedTime;

      TargetType targType;

      if (targetMC.Checked)
        targType = TargetType.MC;
      else
        targType = TargetType.Iterative;

      //Initialize the form with the starting series
/*    boxDemo = new BoxCarDemo(graph1);
      boxDemo.Init(numElements);
      imageSize = new Size(graph1.Width, graph1.Height);
      imageBitmap1 = boxDemo.CreateGraph(imageSize.Width, imageSize.Height, SeriesType.OriginalSeries);
      graph1.Image = imageBitmap1;*/

      elapsedTime = boxDemo.BoxCarFilter(CalculateKernel(filterWidth), targType); 

      //Create a graph of the filtered series
      imageSize = new Size(graph2.Width, graph2.Height);
      graph2.Image = boxDemo.CreateGraph(imageSize.Width, imageSize.Height, SeriesType.FilteredSeries);
      time.Text = "Elapsed Time: " + elapsedTime.ToString();
      return;
    }

    //Calculate the kernel. This particular implementation calculates a simple boxcar filter
    private float[] CalculateKernel(int filterWidth)
    {
      float[] kernel = new float[filterWidth];

      for (int i = 0; i < filterWidth; i++)
      {
        kernel[i] = 1.0f / filterWidth;
      }
      return kernel;
    }
  }

  public class BoxCarDemo
  {
    private PictureBox imageBox;
    float[] originalSeries;
    float[] filteredSeries;
    DX9Target dxTarget;
    MulticoreTarget mcTarget;

    public BoxCarDemo(PictureBox pb)
    {
      imageBox = pb;
    }

    //generate the base time series
    public void Init(int numElements)
    {
      Random ranGen = new Random();
      int count = 0;
      float lastValue = 0;

      originalSeries = new float[numElements];
      filteredSeries = new float[numElements];

      for (int i = 0; i < originalSeries.Length; i++)
      {
        if (count != 0) //force the frequency to be relatively low.
        {
          originalSeries[i] = lastValue;
          if (count++ == (int) numElements/80)
            count = 0;
          continue;
        }
        else if (ranGen.Next(0, 100) < 75)
          originalSeries[i] = 0;
        else
          originalSeries[i] = 1;
        lastValue = originalSeries[i];
        count++;
      }
    }

    //Apply the filter to the starting time series
    public long BoxCarFilter(float [] kernel, TargetType target)
    {
      FPA fpTimeSeries = new FPA(originalSeries);
      FPA fpResult = new FPA(0, fpTimeSeries.Shape);
      int halfWidth = kernel.Length/2;
      long elapsedTime;
      dxTarget = new DX9Target();
      mcTarget = new MulticoreTarget();
      Stopwatch stopWatch = new Stopwatch();

      if (target == TargetType.MC)
      {
        stopWatch.Start();
        for (int i = 0; i < kernel.Length; i++)
        {
          fpResult += PA.Shift(fpTimeSeries, i) * kernel[i];
        }
        mcTarget.ToArray(fpResult, out filteredSeries);
        stopWatch.Stop();
      }
      else //iterative, by default
      {
        stopWatch.Start();
        for (int i = halfWidth; i < (filteredSeries.Length - halfWidth); i++)
        {
          filteredSeries[i] = 0;
          for (int j = -halfWidth; j < halfWidth + 1; j++)
          {
            filteredSeries[i] += originalSeries[i + j] / (kernel.Length);
          }
        }
        stopWatch.Stop();
      }
      elapsedTime = stopWatch.ElapsedTicks;
      return elapsedTime;
    }

    //Create a graph of either series
    public Bitmap CreateGraph(int width, int height, SeriesType type)
    {
      float [] Series;
      float[] workingArray = new float[4000]; 
      
      if (type == SeriesType.OriginalSeries)
        Series = originalSeries;
      else
        Series = filteredSeries;

      workingArray = CreateWorkingArray(Series);

      float xScale = (float)width / (float)(workingArray.Length - 1);
      int i;

      Bitmap graphBitmap = new Bitmap(width, height);
      float delta =  0.75f * height;
      float baseline = 0.1f * height;
      float lastX, lastY, nextX, nextY;
      Pen graphPen = new Pen(Brushes.Black);

      Graphics bmGraphic = Graphics.FromImage(graphBitmap);
      bmGraphic.FillRectangle(new SolidBrush(Color.White),
                              0, 0, 
                              graphBitmap.Width, 
                              graphBitmap.Height);

      lastX = 0;
      lastY = workingArray[0] + baseline;
      for (i = 1; i < workingArray.Length; i++)
      {
        nextX = i * xScale;
        nextY = (workingArray[i] * delta) + baseline;

        bmGraphic.DrawLine(graphPen,
                           lastX,
                           lastY,
                           i * xScale,
                           nextY);
        lastX = i * xScale;
        lastY = (workingArray[i] * delta) + baseline;
      }
      bmGraphic.Dispose();
      return graphBitmap;
    }

    float[] CreateWorkingArray(float[] initialSeries)
    {
      int k = 0;
      int count = 0;
      int increment = initialSeries.Length / 4000;
      float[] workingArray = new float[4000];

      for (int i = 0; i < initialSeries.Length; i++)
      {
        if (count == 0)
        {
          workingArray[k] = initialSeries[i];
          k++;
        }
        count++;
        if (count == increment)
          count = 0;
      }
      return workingArray;
    }

    public void Cleanup()
    {
      //
    }
  }
  public enum SeriesType
  {
    OriginalSeries,
    FilteredSeries,
  }

  public enum TargetType
  {
    DX9,
    MC,
    Iterative
  }
}