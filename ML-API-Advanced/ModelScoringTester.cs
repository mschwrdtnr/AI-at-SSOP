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
                                                    string testDataLocation,
                                                    PredictionEngine<Simulation, CycleTimePrediction> predEngine,
                                                    int numberOfPredictions)
        {
            //Make a few prediction tests 
            // Make the provided number of predictions and compare with observed data from the test dataset
            var testData = ReadSampleDataFromCsvFile(testDataLocation, numberOfPredictions);

            for (int i = 0; i < numberOfPredictions; i++)
            {
                //Score
                var resultprediction = predEngine.Predict(testData[i]);
                float time = testData[i].Time;
                float actualValue = testData[i].CycleTime;
                float estimate = resultprediction.CycleTime;
                //float estimate = resultprediction[i].PredictedCycleTime;
                float differenceAbs = estimate - actualValue;
                float differencePercent = ((estimate / actualValue) - 1) * 100;
               // float sumDifference += difference;

                Console.WriteLine($"Index: {i}");
                Console.WriteLine($"Time: {time}");
                Console.WriteLine($"Absolute Difference: {differenceAbs}");
                Console.WriteLine($"Difference %: {differencePercent}%");
                Common.ConsoleHelper.PrintRegressionPredictionVersusObserved(resultprediction.CycleTime.ToString(), 
                                                            testData[i].CycleTime.ToString());
                //Common.ConsoleHelper.CalculateStandardDeviation(resultprediction.PredictedCycleTime.ToString());
            }

        }

        //This method is using regular .NET System.IO.File and LinQ to read just some sample data to test/predict with 
        public static List<Simulation> ReadSampleDataFromCsvFile(string dataLocation, int numberOfRecordsToRead)
        {
            return File.ReadLines(dataLocation)
                .Skip(1)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Split(','))
                .Select(x => new Simulation()
                {
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
