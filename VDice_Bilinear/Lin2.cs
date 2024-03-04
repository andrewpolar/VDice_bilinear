using System;
using System.Collections.Generic;
using System.Text;

namespace VDice_Bilinear
{
    internal class Lin2
    {
        public List<double[]> _inputs = new List<double[]>();
        public List<double> _target = new List<double>();
        public double[] _model = null;
        public double _last_residual;
        private double _targetMin;
        private double _targetMax;

        public Lin2(List<double[]> inputs, List<double> target)
        {
            _inputs = inputs;
            _target = target;

            if (inputs.Count != target.Count)
            {
                Console.WriteLine("Invalid training data");
                Environment.ExitCode = 0;
            }

            FindMinMax();
        }

        private void FindMinMax()
        {
            _targetMin = double.MaxValue;
            _targetMax = double.MinValue;
            for (int j = 0; j < _target.Count; ++j)
            {
                if (_target[j] < _targetMin) _targetMin = _target[j];
                if (_target[j] > _targetMax) _targetMax = _target[j];
            }
        }

        public double[] GetExtendedInput(double[] x)
        {
            double[] z = new double[8];
            z[0] = x[0];
            z[1] = x[1];
            z[2] = x[2];
            z[3] = x[0] * x[1];
            z[4] = x[0] * x[2];
            z[5] = x[1] * x[2];
            z[6] = x[0] * x[1] * x[2];
            z[7] = 1.0;
            return z;
        }

        public void BuildRepresentation()
        {
            int rows = _inputs.Count;
            int cols = 8;
            double[,] M = new double[rows, cols];
            double[] V = new double[rows];

            for (int i = 0; i < rows; ++i)
            {
                double[] z = GetExtendedInput(_inputs[i]);
                for (int j = 0; j < cols; ++j)
                {
                    M[i, j] = z[j];
                }
                V[i] = _target[i];
            }
            _model = new double[cols];

            Cholesky ch = new Cholesky();
            ch.CholeskySolutionRectangular(M, V, _model, false);
            GetRelativeResidual();
        }

        public void GetRelativeResidual()
        {
            double residual = 0.0;
            for (int i = 0; i < _inputs.Count; ++i)
            {
                double m = ComputeOutput(_inputs[i]);
                residual += (m - _target[i]) * (m - _target[i]);
            }
            residual /= _inputs.Count;
            residual = Math.Sqrt(residual);
            residual /= (_targetMax - _targetMin);
            _last_residual = residual;
        }

        public double ComputeOutput(double[] input)
        {
            double[] z = GetExtendedInput(input);
            double result = 0.0;
            for (int i = 0; i < z.Length; ++i)
            {
                result += z[i] * _model[i];
            }
            return result;
        }

        public void SortData()
        {
            List<double> error = new List<double>();
            for (int i = 0; i < _inputs.Count; ++i)
            {
                error.Add(_target[i] - ComputeOutput(_inputs[i]));
            }
            int[] indexes = new int[error.Count];
            for (int i = 0; i < indexes.Length; ++i)
            {
                indexes[i] = i;
            }
            Array.Sort(error.ToArray(), indexes);
            ResortData(indexes);
        }

        public void ResortData(int[] indexes)
        {
            int len = _inputs[0].Length;
            List<double[]> tmpInput = new List<double[]>();
            List<double> tmpTarget = new List<double>();
            foreach (int n in indexes)
            {
                double[] x = new double[len];
                for (int k = 0; k < len; ++k)
                {
                    x[k] = _inputs[n][k];
                }
                tmpInput.Add(x);
                tmpTarget.Add(_target[n]);
            }
            _inputs.Clear();
            _target.Clear();
            for (int i = 0; i < tmpInput.Count; ++i)
            {
                _inputs.Add(tmpInput[i]);
                _target.Add(tmpTarget[i]);
            }
        }
    }
}
