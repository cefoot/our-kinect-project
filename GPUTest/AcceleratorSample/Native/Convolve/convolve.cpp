// Copyright (c) Microsoft Corporation.   All rights reserved.
/*!
 * 
 * \file Convolve.cpp
 * 
 * Accelerator implementation of Convolution
 * 
 */
#include <stdlib.h>
#include <stdio.h>
#include <math.h>
#include "..\..\..\include\accelerator.h"
#include "..\..\..\include\DX9Target.h"
#include "..\..\..\include\MulticoreTarget.h"
#include "convolve.h"
 
using namespace ParallelArrays;
using namespace MicrosoftTargets;
using namespace std;


int main(int argc, char* argv[])
{
        // create an convolution object based on the user's input
        Convolve *convolve = new Convolve();

        DX9Target *tgtDX9 = CreateDX9Target();
        printf("Starting DX9 Convolution\r\n");
        convolve->Run(tgtDX9);
        tgtDX9->Delete();
        printf("Finished DX9 Convolution\r\n");

		// run on multicore
        MulticoreTarget *tgtMC = CreateMulticoreTarget(false);
        printf("Starting Mulicore Convolution\r\n");
        convolve->Run(tgtMC);
        tgtMC->Delete();
        printf("Finished Mulicore Convolution\r\n");

        delete convolve;
        return 0;
}
/*!
 * Run the convolution with the given target
 */
void Convolve::Run(Target *tgt)
{
    // initialize the data exactly once
    Init(32, 5);
    Float4* resultArray = (Float4 *)malloc(size * size * 4 * 4 * sizeof(float)); 

    ComputeWithAccelerator(tgt, resultArray);

    delete resultArray;

}
/*!
 * Construct based on user commandline arguments
 * -v means verify
 * -i is number of iterions
 * -s is the size of the random array--filter is always 5
 */
Convolve::Convolve()
{
    filter = NULL;
	data = NULL;
};
Convolve::~Convolve()
{
    if (filter != NULL)
        delete filter;
    filter = NULL;
    if (data != NULL)
        delete data;
    data = NULL;
};
/*!
 * Initial data to be a size  X size array.
 * Initial a random filter of size filtSize
 */
void Convolve::Init(int sizeIn, int filtSize)
{

    // initialize the input array
	size = sizeIn;
    GenerateRandomData();

    // initalize the filter
    filterSize = filtSize;
    GenerateFilter(filterSize);
};
/*!
 * Accelerator implementation of convolution with the output stored in resultArray
 * resultArray must be allocated and of size height X width
 */
void Convolve::ComputeWithAccelerator(Target *tgt, Float4 *resultArray) 
{

    // Convolve in X direction.
    Float4 zero = Float4(0);
    F4PA input = F4PA((Float4*)data, size, size);
    size_t dims[] = {size,size};
    F4PA smoothX = F4PA(zero,dims, 2);
    intptr_t counts[] = {0,0};
    int filterHalf = filterSize/2;
    Float4 scale;

    for (int i = -filterHalf; i <= filterHalf; i++) 
    {
        counts[0] = i;
        scale = Float4(filter[i + filterHalf]);
        smoothX += Shift(input, counts, 2) * scale;
    }

   // Convolve in Y direction.
    counts[0] = 0;
    F4PA result = F4PA(zero,dims, 2);
    for (int i = -filterHalf; i <= filterHalf; i++) 
    {
        counts[1] = i;
        scale = Float4(filter[filterHalf + i]);
        result += Shift(smoothX, counts, 2) * scale;
    }

    tgt->ToArray(result, resultArray, size, size, size * 4 * sizeof(float));
};
/*!
 * Generate a random filter of size k
 */
void Convolve::GenerateFilter(int k) 
{
    if (filter != NULL)
        delete filter;
    filter = (float *)malloc(k * sizeof(float));
    // Generate a Gaussian filter, truncated at 2*sigma.
    float sigma = 2 / 2.0;
    float sum = 0;
    for (int i = 0; i < k; i++) {
        filter[i] = exp(-(i-2)*(i-2) / (2*sigma*sigma));
        sum += filter[i];
    }
    for (int i = 0; i < k; i++) {
        filter[i] /= sum;
    }
}
 /*!
 * Fill the data buffer with random data
 */
void Convolve::GenerateRandomData()
{
    Float4 value;
    data = (Float4 *)malloc(size * size * 4 * 4 * sizeof(float)); 

    // initialize the input
    for (int i = 0; i < size; i++)
    {
        for (int j = 0; j < size; j++)
        {
            value = Float4(rand()/(float)RAND_MAX, rand()/(float)RAND_MAX, rand()/(float)RAND_MAX, rand()/(float)RAND_MAX);
            *(data + i * size  + j) = value;
        }
    }
}
/*!
 * Return the closest integer value to x that is between max and min
 */
int Convolve::Clamp(int x, int min, int max) 
{
        return (x < min)? min : (x > max)? max : x;
}
