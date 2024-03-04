using System;
using System.Collections.Generic;

namespace VDice_Bilinear
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DataHolder dh = new DataHolder(10, 1000);
            dh.MakeDataset();

            Resorter resorter = new Resorter();
            resorter.ReadData(dh._inputs, dh._target);
            Console.WriteLine("Resorting: relative residual error {0:0.0000}", resorter.Resort(1));
            Console.WriteLine("Resorting: relative residual error {0:0.0000}", resorter.Resort(2));
            Console.WriteLine("Resorting: relative residual error {0:0.0000}", resorter.Resort(4));
            Console.WriteLine("Resorting: relative residual error {0:0.0000}", resorter.Resort(8));
            Console.WriteLine("Resorting: relative residual error {0:0.0000}", resorter.Resort(16));
            Console.WriteLine("Resorting: relative residual error {0:0.0000}", resorter.Resort(32));
            Console.WriteLine("Resorting: relative residual error {0:0.0000}", resorter.Resort(64));
            Console.WriteLine();
  
            SlidingWindow slide = new SlidingWindow(dh._inputs, dh._target);
            slide.BuildModels(20, 50);

            Console.WriteLine();
            int NTests = 100;
            int passed_DDR = 0;
            int passed_kNN = 0;
            double RMSE_mean = 0.0;
            double RMSE_STD = 0.0;
            double meanMax = Double.MinValue;
            double meanMin = Double.MaxValue;
            double stdMax = Double.MinValue;
            double stdMin = Double.MaxValue;
            int counter = 0;
            while (true)
            {
                double[] randomInput = dh.MakeRandomInput();

                double[] DDRSample = slide.GetOutput(randomInput);
                double[] MonteCarloSample = dh.GetMonteCarloSample(randomInput, 4096);
                double[] kNN = Static.GetKNNSample(dh._inputs, dh._target, randomInput, DDRSample.Length);
 
                double MonteCarloMean = Static.GetMean(MonteCarloSample);
                double MonteCarloSTD = Static.GetSTD(MonteCarloSample, MonteCarloMean);

                double DDRMean = Static.GetMean(DDRSample);
                double DDRSTD = Static.GetSTD(DDRSample, DDRMean);

                if (MonteCarloMean > meanMax) meanMax = MonteCarloMean; 
                if (MonteCarloMean < meanMin) meanMin = MonteCarloMean;
                if (MonteCarloSTD > stdMax) stdMax = MonteCarloSTD;
                if (MonteCarloSTD < stdMin) stdMin = MonteCarloSTD;

                RMSE_mean += (DDRMean - MonteCarloMean) * (DDRMean - MonteCarloMean);
                RMSE_STD += (DDRSTD - MonteCarloSTD) * (DDRSTD - MonteCarloSTD);

                double p = Static.GetProbMedian(new List<double>(MonteCarloSample), new List<double>(DDRSample), 5);
                if (p > 0.005)
                {
                    ++passed_DDR;
                }

                double p1 = Static.GetProbMedian(new List<double>(MonteCarloSample), new List<double>(kNN), 5);
                if (p1 > 0.005)
                {
                    ++passed_kNN;
                }

                Console.Write("Test {0} \r", counter + 1);

                if (++counter >= NTests) break;
            }
            RMSE_mean /= NTests;
            RMSE_STD /= NTests;
            RMSE_mean = Math.Sqrt(RMSE_mean) / (meanMax - meanMin);
            RMSE_STD = Math.Sqrt(RMSE_STD) / (stdMax - stdMin);

            Console.WriteLine("Passed DDR goodness of fit tests {0} from {1} tests", passed_DDR, NTests);
            Console.WriteLine("Passed kNN goodness of fit tests {0} from {1} tests", passed_kNN, NTests);
            Console.WriteLine("Root means square error for means {0:0.0000}", RMSE_mean);
            Console.WriteLine("Root means square error for STDs {0:0.0000}", RMSE_STD);
        }
    }
}
