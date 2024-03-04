﻿using System;
using System.Collections.Generic;
using System.Text;

namespace VDice_Bilinear
{
    internal class SlidingWindow
    {
        private List<double[]> _inputs = null;
        private List<double> _target = null;
        List<Lin2> _lin2_list = null;
        private Random _rnd = new Random();

        public SlidingWindow(List<double[]> inputs, List<double> target)
        {
            _inputs = inputs;
            _target = target;
        }

        public void BuildModels(int SortedNBlocks, int nWantedModels)
        {
            int blockLength = _inputs.Count / SortedNBlocks;
            int shiftSize = (_inputs.Count - blockLength) / (nWantedModels - 1);
            List<int> A = new List<int>();
            List<int> B = new List<int>();
            int currentA = 0;
            int currentB = blockLength - 1;
            A.Add(currentA);
            B.Add(currentB);
            for (int i = 1; i < nWantedModels; ++i)
            {
                currentA += shiftSize;
                currentB += shiftSize;
                A.Add(currentA);
                if (_inputs.Count - 1 - currentB < shiftSize)
                {
                    currentB = _inputs.Count - 1;
                }
                B.Add(currentB);
                if (currentB >= _inputs.Count - 1)
                {
                    break;
                }
            }

            int models = A.Count;
            List<double[]> x = new List<double[]>();
            List<double> y = new List<double>();
            _lin2_list = new List<Lin2>();
            int nxsize = _inputs[0].Length;
            for (int i = 0; i < models; ++i)
            {
                x.Clear();
                y.Clear();
                for (int k = A[i]; k <= B[i]; ++k)
                {
                    double[] currentx = new double[nxsize];
                    for (int j = 0; j < nxsize; ++j)
                    {
                        currentx[j] = _inputs[k][j];
                    }
                    x.Add(currentx);
                    y.Add(_target[k]);
                }

                Lin2 lin2 = new Lin2(x, y);
                lin2.BuildRepresentation();
                _lin2_list.Add(lin2);
                Console.WriteLine("Sliding window error {0:0.0000}", lin2._last_residual);
            }
        }

        public double[] GetOutput(double[] x)
        {
            List<double> output = new List<double>();
            for (int i = 1; i < _lin2_list.Count - 1; ++i)
            {
                output.Add(_lin2_list[i].ComputeOutput(x));
            }
            return output.ToArray();
        }

        public double[] GetOutputCloud(double[] x, int N, double noise)
        {
            List<double> output = new List<double>();
            for (int i = 0; i < _lin2_list.Count; ++i)
            {
                output.Add(_lin2_list[i].ComputeOutput(x));
            }
            for (int k = 0; k < N; ++k)
            {
                double[] z = new double[x.Length];
                for (int j = 0; j < x.Length; ++j) 
                {
                    double n = (_rnd.Next(100) - 50.0) / 100.0 * noise;
                    z[j] = x[j] * (1.0 + n);
                }
                for (int i = 0; i < _lin2_list.Count; ++i)
                {
                    output.Add(_lin2_list[i].ComputeOutput(z));
                }
            }
            return output.ToArray();
        }
    }
}