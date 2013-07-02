using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

using Microsoft.ParallelArrays;
using A = Microsoft.ParallelArrays.ParallelArrays;
using FPA = Microsoft.ParallelArrays.FloatParallelArray;

//-----------------------------------------------------------------------------
namespace _2DConvolution
{
  class Convolve
  {
    Bitmap bmOriginal;
    PictureBox imageBox;
    float[,] imageArray;
    int height, width;

    public Convolve(PictureBox box)
    {
      //Add a target type
      imageBox = box;
    }

    public void Init()
    {
      height = imageBox.Height;
      width = imageBox.Width;
      imageArray = CreateTestArray();
      imageBox.Image = bmOriginal;
    }

    public long Run(TargetType targ)
    {
      int filterLength = 11; //must be odd. Originally 5 elements
      int halfWidth = (filterLength - 1) / 2;

      float[] filter = new float[filterLength];
      float sigma = 4f; //Make configurable?
      float sum = 0;
      float[,] filteredArray = new float[height, width];
      DX9Target dxTarget;
      MulticoreTarget mcTarget;
      Stopwatch stopWatch = new Stopwatch();
      int colIndex, rowIndex; //column and row iteration indices
      int filterIndex; //iteration index

      dxTarget = new DX9Target();
      mcTarget = new MulticoreTarget();

      // Generate a Gaussian filter, truncated at 2*sigma.
      for (int i = 0; i < filterLength; i++)
      {
        filter[i] = (float)Math.Exp(-(i - halfWidth) * (i - halfWidth) / (2 * sigma * sigma));
        sum += filter[i];
      }
      for (int i = 0; i < filterLength; i++)
      {
        filter[i] /= sum;
      }

      if (targ != TargetType.Iterative) //Accelerator targets
      {
        stopWatch.Start();
        FPA data = new FPA(imageArray);

        // Convolve in X direction.
        FPA smoothX = new FPA(0, data.Shape);
        for (colIndex = 0; colIndex < filterLength; colIndex++)
        {
          smoothX += A.Shift(data, 0, colIndex - halfWidth) * filter[colIndex];
        }

        // Convolve in Y direction.
        FPA result = new FPA(0, data.Shape);
        for (rowIndex = 0; rowIndex < filterLength; rowIndex++)
        {
          result += A.Shift(smoothX, rowIndex - halfWidth, 0) * filter[rowIndex];
        }

        if (targ == TargetType.MC)
          mcTarget.ToArray(result, out filteredArray);
        else
          dxTarget.ToArray(result, out filteredArray);

        stopWatch.Stop();
      }
      else
      {
        stopWatch.Start();
        float[,] filteredArrayX = new float[height, width];

        //x convolution
        for (colIndex = 0; colIndex < width; colIndex++)
        {
          for (rowIndex = 0; rowIndex < height; rowIndex++)
          {
            if (rowIndex >= halfWidth && rowIndex < (width - halfWidth))  //non-endpoint calculation
            {
              for (filterIndex = 0; filterIndex < filterLength; filterIndex++)
              {
                filteredArrayX[rowIndex, colIndex] += imageArray[rowIndex + filterIndex - halfWidth, colIndex] * filter[filterIndex];
              }
            }
            else if (rowIndex < halfWidth) //beginning of series
            {
              for (filterIndex = 0; filterIndex < rowIndex + halfWidth; filterIndex++)
              {
                filteredArrayX[rowIndex, colIndex] += imageArray[filterIndex, colIndex] * filter[filterIndex + halfWidth - rowIndex];
              }
            }
            else //end of series
            {
              for (filterIndex = -halfWidth; rowIndex + filterIndex < width; filterIndex++)
              {
                filteredArrayX[rowIndex, colIndex] += imageArray[rowIndex + filterIndex, colIndex] * filter[filterIndex + halfWidth];
              }
            }
          }
        }

        //y convolution
        for (rowIndex = 0; rowIndex < height; rowIndex++)
        {
          for (colIndex = 0; colIndex < width; colIndex++) 
          {
            if (colIndex >= halfWidth && colIndex < (width - halfWidth))  //non-endpoint calculation
            {
              for (filterIndex = 0; filterIndex < filterLength; filterIndex++)
              {
                filteredArray[rowIndex, colIndex] += filteredArrayX[rowIndex, colIndex + filterIndex - halfWidth] * filter[filterIndex];
              }
            }
            else if (colIndex < halfWidth) //beginning of series
            {
              for (filterIndex = 0; filterIndex < colIndex + halfWidth; filterIndex++)
              {
                filteredArray[rowIndex, colIndex] += filteredArrayX[rowIndex, filterIndex + halfWidth - colIndex] * filter[filterIndex];
              }
            }
            else //end of series
            {
              for (filterIndex = -halfWidth; colIndex + filterIndex < width; filterIndex++)
              {
                filteredArray[rowIndex, colIndex] += filteredArrayX[rowIndex, colIndex + filterIndex] * filter[filterIndex + halfWidth];
              }
            }
          }
        }
        stopWatch.Stop();
      }

      imageBox.Image = ArrayToBitmap(filteredArray);
      return stopWatch.ElapsedTicks;
    }

    //Creates a B/W bitmap and converts it to a float[ , ] array
    public float[,] CreateTestArray()
    {
      bmOriginal = new Bitmap(width, height);
      Graphics g = Graphics.FromImage(bmOriginal);
      g.FillRectangle(new SolidBrush(Color.White), 0, 0, bmOriginal.Width, bmOriginal.Height);
      Random rand = new Random();
      for (int i = 0; i < 300; i++)
      {
        int x = rand.Next(bmOriginal.Width);
        int y = rand.Next(bmOriginal.Width);
        int radius = rand.Next(bmOriginal.Width / 30);
        g.FillEllipse(new SolidBrush(Color.Black), x - radius, y - radius, radius * 2, radius * 2);
      }
      float[,] result = new float[height, width];

      for (int rowIndex = 0; rowIndex < height; rowIndex++)
      {
        for (int colIndex = 0; colIndex < width; colIndex++)
        {
          Color c = bmOriginal.GetPixel(colIndex, rowIndex);
          result[rowIndex, colIndex] = (c.R + c.G + c.B) / 3f;
        }
      }
      return result;
    }

    Bitmap ArrayToBitmap(float[,] imageArray)
    {
      Bitmap bm = new Bitmap(width,height);
      for (int y = 0; y < height; y++)
      {
        for (int x = 0; x < width; x++)
        {
          int c = (int) imageArray[y, x]; 
          Color pixelColor = Color.FromArgb(c, c, c);
          bm.SetPixel(x,y,pixelColor);
        }
      }
      return bm;
    }
  }
}

//-----------------------------------------------------------------------------
