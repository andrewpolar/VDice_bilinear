﻿using System;
using System.Collections.Generic;
using System.Text;

namespace VDice_Bilinear
{
    internal class Static
    {
        public static double[] GetKNNSample(List<double[]> inputs, List<double> targets, double[] x, int N)
        {
            List<double> distance = new List<double>();
            for (int i = 0; i < inputs.Count; i++)
            {
                double d = 0.0;
                for (int j = 0; j < inputs[i].Length; j++)
                {
                    d += (inputs[i][j] - x[j]) * (inputs[i][j] - x[j]);

                }
                distance.Add(d);
            }
            double[] dd = distance.ToArray();
            int[] indexes = new int[dd.Length];
            for (int i = 0; i < dd.Length; i++)
            {
                indexes[i] = i;
            }

            Array.Sort(dd, indexes);
            List<double> result = new List<double>();
            int cnt = 0;
            while (true)
            {
                result.Add(targets[indexes[cnt]]);
                ++cnt;
                if (cnt >= N) break;
            }
            return result.ToArray();
        }

        public static double relativeDistance(double[] x, double[] y)
        {
            if (x.Length != y.Length)
            {
                Console.WriteLine("Static.relativeDistance wrong data passed");
                Environment.Exit(0);
            }
            double dist = 0.0;
            double norm1 = 0.0;
            double norm2 = 0.0;
            for (int i = 0; i < x.Length; ++i)
            {
                dist += (x[i] - y[i]) * (x[i] - y[i]);
                norm1 += x[i] * x[i];
                norm2 += y[i] * y[i];
            }
            dist = Math.Sqrt(dist);
            norm1 = Math.Sqrt(norm1);
            norm2 = Math.Sqrt(norm2);
            double norm = (norm1 + norm2) / 2.0;
            return dist / norm;
        }

        private static void MedianSplit(List<double> x, int depth, List<double> list)
        {
            if (0 == depth) return;

            x.Sort();
            int size = x.Count;
            double median = 0;
            if (0 == size % 2)
            {
                median = (x[size / 2 - 1] + x[size / 2]) / 2.0;
            }
            else median = x[size / 2];

            list.Add(median);

            List<double> left = new List<double>();
            List<double> right = new List<double>();
            for (int i = 0; i < x.Count; i++)
            {
                if (i < size / 2)
                {
                    left.Add(x[i]);
                }
                if (0 != size % 2)
                {
                    if (i > size / 2)
                    {
                        right.Add(x[i]);
                    }
                }
                else
                {
                    if (i >= size / 2)
                    {
                        right.Add(x[i]);
                    }
                }
            }

            MedianSplit(left, depth - 1, list);
            MedianSplit(right, depth - 1, list);
        }

        public static double Median_Statistic(List<double> x, List<double> y, int depth)
        {
            List<double> x_list = new List<double>();
            List<double> y_list = new List<double>();
            MedianSplit(x, depth, x_list);
            MedianSplit(y, depth, y_list);
            x_list.Sort();
            y_list.Sort();
            return relativeDistance(x_list.ToArray(), y_list.ToArray());
        }

        private static List<double> GetpValuesMedian(List<double> data, int subSampleSize, int depth)
        {
            if (subSampleSize * 2 > data.Count)
            {
                System.Console.WriteLine("Median pValues can be estimated for subsamples smaller than {0}", data.Count / 2);
                Environment.Exit(0);
            }

            List<double> result = new List<double>();
            List<double> sample = new List<double>();
            Random random = new Random();
            for (int K = 0; K < 100; ++K)
            {
                sample.Clear();
                for (int i = 0; i < subSampleSize; ++i)
                {
                    int pos = random.Next(data.Count);
                    sample.Add(data[pos]);
                }
                double cvm = Median_Statistic(sample, data, depth);
                result.Add(cvm);
            }
            return result;
        }

        public static double GetProbMedian(List<double> data, List<double> sample, int depth)
        {
            List<double> pValues = GetpValuesMedian(new List<double>(data), sample.Count, depth);
            pValues.Sort();
            double cvm = Median_Statistic(data, sample, depth);
            int counter = 0;
            for (int i = pValues.Count - 1; i >= 0; --i)
            {
                if (cvm > pValues[i])
                {
                    break;
                }
                ++counter;
            }
            return counter / 100.0;
        }

        public static double GetMean(double[] x)
        {
            double mean = 0.0;
            foreach (double v in x)
            {
                mean += v;
            }
            return mean / x.Length;
        }

        public static double GetSTD(double[] x, double mean)
        {
            double std = 0.0;
            foreach (double v in x)
            {
                std += (v - mean) * (v - mean);
            }
            std /= x.Length;
            return Math.Sqrt(std);
        }
    }
}
