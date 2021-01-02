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
                                                    PredictionEngine<Simulation, CycleTimePrediction> predEngine,
                                                    int numberOfPredictions)
        {
            //Make a few prediction tests 
            // Make the provided number of predictions and compare with observed data from the test dataset
            var evalData = ReadSampleDataFromCsvFile(evalDataLocation, numberOfPredictions);

            for (int i = 0; i < numberOfPredictions; i++)
            {
                //Score
                var resultprediction = predEngine.Predict(evalData[i]);
                float time = evalData[i].Time;
                float actualValue = evalData[i].CycleTime;
                float estimate = resultprediction.CycleTime;
                //float estimate = resultprediction[i].PredictedCycleTime;
                float differenceAbs = estimate - actualValue;
                float differencePercent = ((estimate / actualValue) - 1) * 100;
               // float sumDifference += difference;

                Console.WriteLine($"Index: {i}");
                Console.WriteLine($"Time: {time}");
                Console.WriteLine($">> Difference in %: {differencePercent} % <<");
                Console.WriteLine($"Absolute Difference: {differenceAbs}");
                Common.ConsoleHelper.PrintRegressionPredictionVersusObserved(resultprediction.CycleTime.ToString(), 
                                                            evalData[i].CycleTime.ToString());
                //Common.ConsoleHelper.CalculateStandardDeviation(resultprediction.PredictedCycleTime.ToString());
            }

        }

        //This method is using regular .NET System.IO.File and LinQ to read just some sample data to test/predict with 
        public static List<Simulation> ReadSampleDataFromCsvFile(string dataLocation, int numberOfRecordsToRead)
        {
            return File.ReadLines(dataLocation)
                .Skip(1)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Split(';'))
                .Select(x => new Simulation()
                {
                    /*                    Time = int.Parse(x[0]),
                                        Lateness = float.Parse(x[1]),
                                        Assembly = float.Parse(x[2]),
                                        Total = int.Parse(x[3]),
                                        CycleTime = float.Parse(x[4]),
                                        Consumab = float.Parse(x[5]),
                                        Material = float.Parse(x[6]),
                                        InDueTotal = int.Parse(x[7])*/
                    Time = float.Parse(x[0], CultureInfo.InvariantCulture),
                    Lateness = float.Parse(x[1], CultureInfo.InvariantCulture),
                    Assembly = float.Parse(x[2], CultureInfo.InvariantCulture),
                    Total = float.Parse(x[3], CultureInfo.InvariantCulture),
                    CycleTime = float.Parse(x[4], CultureInfo.InvariantCulture),
                    Consumab = float.Parse(x[5], CultureInfo.InvariantCulture),
                    Material = float.Parse(x[6], CultureInfo.InvariantCulture),
                    InDueTotal = float.Parse(x[7], CultureInfo.InvariantCulture)
                })
                .Take(numberOfRecordsToRead)
                .ToList();
        }
    }
}
