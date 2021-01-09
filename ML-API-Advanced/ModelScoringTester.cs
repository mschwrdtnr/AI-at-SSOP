using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System;
using ML_API_Advanced.DataStuctures;

using Microsoft.ML;

namespace ML_API_Advanced
{
    public static class ModelScoringTester
    {
        public static void VisualizeSomePredictions(MLContext mlContext,
                                                    //string modelName, 
                                                    string evalDataLocation,
                                                    PredictionEngine<SimulationKpis, CycleTimePrediction> predEngine,
                                                    int numberOfPredictions)
        {
            //Make a few prediction tests 
            // Make the provided number of predictions and compare with observed data from the test dataset
            var evalData = ReadSampleDataFromCsvFile(evalDataLocation, numberOfPredictions);

            for (int i = 0; i < numberOfPredictions; i++)
            {
                //Score
                var resultprediction = predEngine.Predict(evalData[i]);
                //float time = evalData[i].Time;
                double actualValue = evalData[i].CycleTime_t1;
                double estimate = resultprediction.CycleTime;
                //float estimate = resultprediction[i].PredictedCycleTime;
                double differenceAbs = estimate - actualValue;
                double differencePercent = ((estimate / actualValue) - 1) * 100;
               // float sumDifference += difference;

                Console.WriteLine($"Index: {i}");
                //Console.WriteLine($"Time: {time}");
                Console.WriteLine($">> Difference in %: {differencePercent:F5} % <<");
                Console.WriteLine($"Absolute Difference: {differenceAbs:F5}");
                Common.ConsoleHelper.PrintRegressionPredictionVersusObserved(resultprediction.CycleTime.ToString(), 
                                                            evalData[i].CycleTime_t1.ToString());
                //Common.ConsoleHelper.CalculateStandardDeviation(resultprediction.PredictedCycleTime.ToString());
            }

        }

        //This method is using regular .NET System.IO.File and LinQ to read just some sample data to test/predict with 
        public static List<SimulationKpis> ReadSampleDataFromCsvFile(string dataLocation, int numberOfRecordsToRead)
        {
            return File.ReadLines(dataLocation)
                .Skip(1)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Split(';'))
                .Select(x => new SimulationKpis()
                {
                    /*                    Time = int.Parse(x[0]),
                                        Lateness = float.Parse(x[1]),
                                        Assembly = float.Parse(x[2]),
                                        Total = int.Parse(x[3]),
                                        CycleTime = float.Parse(x[4]),
                                        Consumab = float.Parse(x[5]),
                                        Material = float.Parse(x[6]),
                                        InDueTotal = int.Parse(x[7])*/
                    Lateness_t2 = float.Parse(x[0], CultureInfo.InvariantCulture),
                    Assembly_t2 = float.Parse(x[1], CultureInfo.InvariantCulture),
                    Total_t2 = float.Parse(x[2], CultureInfo.InvariantCulture),
                    CycleTime_t2 = float.Parse(x[3], CultureInfo.InvariantCulture),
                    Consumab_t2 = float.Parse(x[4], CultureInfo.InvariantCulture),
                    Material_t2 = float.Parse(x[5], CultureInfo.InvariantCulture),
                    InDueTotal_t2 = float.Parse(x[6], CultureInfo.InvariantCulture),
                    Lateness_t1 = float.Parse(x[7], CultureInfo.InvariantCulture),
                    Assembly_t1 = float.Parse(x[8], CultureInfo.InvariantCulture),
                    Total_t1 = float.Parse(x[9], CultureInfo.InvariantCulture),
                    CycleTime_t1 = float.Parse(x[10], CultureInfo.InvariantCulture),
                    Consumab_t1 = float.Parse(x[11], CultureInfo.InvariantCulture),
                    Material_t1 = float.Parse(x[12], CultureInfo.InvariantCulture),
                    InDueTotal_t1 = float.Parse(x[13], CultureInfo.InvariantCulture),
                    Lateness_t0 = float.Parse(x[14], CultureInfo.InvariantCulture),
                    Assembly_t0 = float.Parse(x[15], CultureInfo.InvariantCulture),
                    Total_t0 = float.Parse(x[16], CultureInfo.InvariantCulture),
                    Consumab_t0 = float.Parse(x[17], CultureInfo.InvariantCulture),
                    Material_t0 = float.Parse(x[18], CultureInfo.InvariantCulture),
                    InDueTotal_t0 = float.Parse(x[19], CultureInfo.InvariantCulture),
                    CycleTime_t0 = float.Parse(x[20], CultureInfo.InvariantCulture)
                })
                .Take(numberOfRecordsToRead)
                .ToList();
        }
    }
}
