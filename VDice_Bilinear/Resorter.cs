﻿using System;
using System.Collections.Generic;
using System.Text;

namespace VDice_Bilinear
{
    internal class Resorter
    {
        public static List<double[]> _inputs = null;
        public static List<double> _target = null;

        public bool ReadData(List<double[]> Inputs, List<double> Targets)
        {
            _inputs = Inputs;
            _target = Targets;
            return true;
        }

        public double Resort(int NBlocks)
        {
            double mean_relative_residual = 0.0; 
            int inputlen = _inputs[0].Length;
            int blocksize = _inputs.Count / NBlocks;
            if (0 != _inputs.Count % NBlocks)
            {
                blocksize += 1;
            }

            List<double[]> blockinput = new List<double[]>();
            List<double> blocktarget = new List<double>();
            int counter = 0;
            List<double[]> sortedinputs = new List<double[]>();
            List<double> sortedtarget = new List<double>();
            for (int j = 0; j < _inputs.Count; ++j)
            {
                double[] x = new double[inputlen];
                for (int k = 0; k < inputlen; ++k)
                {
                    x[k] = _inputs[j][k];
                }
                double t = _target[j];

                blockinput.Add(x);
                blocktarget.Add(t);

                if (++counter >= blocksize || j >= _inputs.Count - 1)
                {
                    Lin2 lin = new Lin2(blockinput, blocktarget);
                    lin.BuildRepresentation();
                    lin.SortData();
                    mean_relative_residual += lin._last_residual;
  
                    for (int k = 0; k < lin._inputs.Count; ++k)
                    {
                        double[] z = new double[inputlen];
                        for (int m = 0; m < inputlen; ++m)
                        {
                            z[m] = lin._inputs[k][m];
                        }
                        double t2 = lin._target[k];

                        sortedinputs.Add(z);
                        sortedtarget.Add(t2);
                    }

                    blockinput.Clear();
                    blocktarget.Clear();
                    counter = 0;
                }
            }
 
            _inputs.Clear();
            _target.Clear();

            for (int k = 0; k < sortedinputs.Count; ++k)
            {
                double[] z = new double[inputlen];
                for (int m = 0; m < inputlen; ++m)
                {
                    z[m] = sortedinputs[k][m];
                }
                double t2 = sortedtarget[k];

                _inputs.Add(z);
                _target.Add(t2);
            }

            return mean_relative_residual / NBlocks;
        }
    }
}
