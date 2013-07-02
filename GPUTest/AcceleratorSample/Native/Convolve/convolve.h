// Copyright (c) Microsoft Corporation.   All rights reserved.
/*!
 * 
 * \file convolve.h
 * 
 * Convolution for Accelerator v2
 * 
 */
using namespace ParallelArrays;
using namespace MicrosoftTargets;

/*!
 * Convolution Benchmark
 */
class Convolve
{
private:
    float *filter;  // the filter
    int filterSize; // length of the filter
	int size;		// edge size of square input data
	Float4 *data;

public:
    Convolve();
    ~Convolve();
    void Run(Target *tgt);
    /*!
     * Initial data to be a size  X size array.
     * Initial a random filter of size filtSize
     */
    void Init(int size, int filterSize);
    /*!
     * Release all resources
     */
    /*!
     * Accelerator implementation of convolution with the output stored in resultArray
     * resultArray must be allocated and of size  X size
     */
    void ComputeWithAccelerator(Target *tgt, Float4 *resultArray);
    /*!
     * Verify that the Accelerator implementation with the given target and base implementation give the same results
     */
    /*!
     * Generate a random filter of size k
     */
		void GenerateFilter(int k);
	 /*!
	 * Fill the data buffer with random data
	 */
	void GenerateRandomData();
    /*!
     * Return the closest integer value to x that is between max and min
     */
    int Clamp(int x, int min, int max);
};
