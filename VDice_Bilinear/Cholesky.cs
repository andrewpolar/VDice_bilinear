﻿/*
Copyright 2013, Andrew Polar

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDice_Bilinear
{
    internal class Cholesky
    {
        //main method for Cholesky decomposition.
        //input         n  size of matrix
        //input/output  a  Symmetric positive def. matrix
        //output        p  vector of resulting diag of a
        //author:       <Vadum Kutsyy, kutsyy@hotmail.com>
        //Converted to c# by Andrew Polar
        private bool choldc1(int n, double[,] a, double[] p)
        {
            int i, j, k;
            double sum;

            for (i = 0; i < n; i++)
            {
                for (j = i; j < n; j++)
                {
                    sum = a[i, j];
                    for (k = i - 1; k >= 0; k--)
                    {
                        sum -= a[i, k] * a[j, k];
                    }
                    if (i == j)
                    {
                        if (sum <= 0)
                        {
                            Console.WriteLine("Matrix is not positive definite! Sum = {0:0.0000}", sum);
                            return false;
                        }
                        p[i] = Math.Sqrt(sum);
                    }
                    else
                    {
                        a[j, i] = sum / p[i];
                    }
                }
            }
            return true;
        }

        private bool choldc(int n, double[,] A, double[,] a)
        {
            int i, j;
            double[] p = new double[n];

            for (i = 0; i < n; i++)
                for (j = 0; j < n; j++)
                    a[i, j] = A[i, j];

            if (!choldc1(n, a, p))
            {
                return false;
            }

            for (i = 0; i < n; i++)
            {
                a[i, i] = p[i];
                for (j = i + 1; j < n; j++)
                {
                    a[i, j] = 0;
                }
            }

            p = null;
            return true;
        }

        ///////////////////////////////////////////////////////////////////
        //solution for M * x = y
        //M must be positive definite
        public bool CholeskySolution(double[,] M, double[] x, double[] y, int size)
        {
            double[,] T = new double[size, size];

            //find lower triangular
            if (!choldc(size, M, T))
            {
                return false;
            }
            //make it symmetric
            for (int i = 0; i < size; ++i)
                for (int j = i + 1; j < size; ++j)
                    T[i, j] = T[j, i];

            double[] v = new double[size];

            //solve lower triangular
            for (int i = 0; i < size; ++i)
            {
                double sum = 0.0;
                for (int j = 0; j < i; ++j)
                {
                    sum += T[i, j] * v[j];
                }
                v[i] = (y[i] - sum) / T[i, i];
            }

            //solve upper triangular
            for (int i = size - 1; i >= 0; --i)
            {
                double sum = 0.0;
                for (int j = size - 1; j > i; --j)
                {
                    sum += T[i, j] * x[j];
                }
                x[i] = (v[i] - sum) / T[i, i];
            }

            v = null;
            T = null;

            return true;
        }

        public void CholeskySolutionRectangular(double[,] matrix, double[] vector, double[] solution, bool regularization)
        {
            int nRows = matrix.GetLength(0);
            int nCols = matrix.GetLength(1);

            double[,] mtm = new double[nCols, nCols];
            for (int i = 0; i < nCols; ++i)
            {
                for (int j = 0; j < nCols; ++j)
                {
                    mtm[i, j] = 0.0;
                    for (int k = 0; k < nRows; ++k)
                    {
                        mtm[i, j] += matrix[k, i] * matrix[k, j];
                    }
                }
            }

            //regularization
            if (regularization)
            {
                double trace = 0.0;
                for (int i = 0; i < nCols; ++i)
                {
                    trace += mtm[i, i];
                }
                trace /= (double)(nCols);
                trace /= 1000.0;

                for (int i = 0; i < nCols; ++i)
                {
                    mtm[i, i] += trace;
                }
            }
            //end

            double[] mtv = new double[nCols];
            for (int i = 0; i < nCols; ++i)
            {
                mtv[i] = 0.0;
                for (int k = 0; k < nRows; ++k)
                {
                    mtv[i] += matrix[k, i] * vector[k];
                }
            }

            if (!CholeskySolution(mtm, solution, mtv, nCols))
            {
                Console.WriteLine("Failed to obtain solution of linear system");
                Environment.Exit(1);
            }
        }

        public void SelfTest()
        {
            const int size = 10;
            double[,] H = new double[size, size];

            for (int i = 0; i < size; ++i)
            {
                for (int j = 0; j < size; ++j)
                {
                    H[i, j] = 1.0 / (double)(i + j + 1);
                }
            }

            double[] x = new double[size];
            double[] y = new double[size];

            for (int i = 0; i < size; ++i)
            {
                x[i] = (double)(i + 1);
                y[i] = 0.0;
            }

            for (int i = 0; i < size; ++i)
            {
                for (int j = 0; j < size; ++j)
                {
                    y[i] += H[i, j] * x[j];
                }
            }
            for (int i = 0; i < size; ++i)
            {
                x[i] = 0.0;
            }

            Cholesky ch = new Cholesky();
            if (ch.CholeskySolution(H, x, y, size))
            {
                for (int i = 0; i < size; ++i)
                {
                    Console.WriteLine(x[i]);
                }
            }
            else
            {
                Console.WriteLine("Failed to obtain cholessky solution");
            }
        }
    }
}
