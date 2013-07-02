using System;
using System.Drawing;
using System.Windows.Forms;


using Microsoft.ParallelArrays;
using PA = Microsoft.ParallelArrays.ParallelArrays;
using FPA = Microsoft.ParallelArrays.FloatParallelArray;
using BPA = Microsoft.ParallelArrays.BoolParallelArray;

namespace Life
{
  class Life
  {
    int gridSize;
    int[] gridDimensions;
    float[,] currentState;
    int width, height;
    PictureBox imageBox;
    DX9Target dx9Targ;
    MulticoreTarget mcTarg;
    TargetType evalTarget;

    FPA zeroGrid;
    FPA oneGrid;
    FPA twoGrid;
    FPA threeGrid;

    public Life(int size, PictureBox box)
    {
      gridSize = size;
      gridDimensions = new int[] { gridSize, gridSize };
      imageBox = box;
    }

    public void Init(TargetType type)
    {
      evalTarget = type;
      dx9Targ = new DX9Target();
      mcTarg = new MulticoreTarget();

      currentState = new float[gridSize, gridSize];

      //Create initial grid.
      for (int i = 0; i < gridSize; i++)
      {
        currentState[0, i] = 1;
        currentState[i, 0] = 1;
        currentState[i, i] = 1;
      }

      
      imageBox.Image = ArrayToBitmap(currentState);

      //Create utility objects.
      zeroGrid = new FPA(0.0f, gridDimensions);
      oneGrid = new FPA(1.0f, gridDimensions);
      twoGrid = new FPA(2.0f, gridDimensions);
      threeGrid = new FPA(3.0f, gridDimensions);
    }

    //Utility methods
    FPA And(FPA a, FPA b) { return PA.Min(a, b); }
    FPA Or(FPA a, FPA b) { return PA.Max(a, b); }

    FPA Equals(FPA a, FPA b)
    {
      return PA.Cond(PA.CompareEqual(a,b), oneGrid, zeroGrid);
    }

    //Compute the next state and update the working grid
    public void ComputeNextState()
    {
      FPA nearestNeighbors = new FPA(0.0f, gridDimensions);
      FPA workingGrid = new FPA(currentState);

      //Determine the number of live nearest neighbors for each cell
      for (int gridY = -1; gridY <= 1; gridY++)
      {
        for (int gridX = -1; gridX <= 1; gridX++)
        {
          if(!(gridX == 0 && gridY == 0))
          {
            nearestNeighbors = nearestNeighbors + PA.Rotate(workingGrid, gridY, gridX);
          }
        }
      }

      //Update the grid, based on the nearest neighbor data
      workingGrid = Or(Equals(nearestNeighbors, threeGrid), And(Equals(nearestNeighbors, twoGrid), workingGrid));

      //Evaluate the results and update the image
      if(evalTarget == TargetType.DX9)
        dx9Targ.ToArray(workingGrid, out currentState);
      else
        mcTarg.ToArray(workingGrid, out currentState);

      imageBox.Image = ArrayToBitmap(currentState);
      GC.Collect();
    }

    //Converts a grid array to a blue and white bitmap.
    private Bitmap ArrayToBitmap(float[,] imageArray)
    {
      Color pixelColor;
      Bitmap workingBitmap = new Bitmap(gridSize / 2, gridSize / 2);

      for (int y = 0; y < gridSize/2; y++)
      {
        for (int x = 0; x < gridSize/2; x++)
        {
          if (imageArray[y, x] >= 0.5)
            pixelColor = Color.FromArgb(0, 0, 255);
          else
            pixelColor = Color.FromArgb(255, 255, 255);
          workingBitmap.SetPixel(x, y, pixelColor);
        }
      }
      return workingBitmap;
    }
  }
}
