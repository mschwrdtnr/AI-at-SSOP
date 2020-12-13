﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ML_API_Advanced.DataStuctures;
using Common;
using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
using PLplot;

namespace ML_API_Advanced
{
    internal static class Program
    {

        private static string rootDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../"));
        private static string ModelPath = Path.Combine(rootDir, "MLModel.zip");
        private static string TrainDataPath = "C:/Users/axsw/source/repos/ML-API-Advanced/ML-API-Advanced/Data/CycleTime_train.csv";
        private static string TestDataPath = "C:/Users/axsw/source/repos/ML-API-Advanced/ML-API-Advanced/Data/CycleTime_eval.csv";

        private static MLContext mlContext = new MLContext();

        private static IDataView trainDataView = mlContext.Data.LoadFromTextFile<Simulation>(TrainDataPath, hasHeader: true, separatorChar: ',');
        private static IDataView testDataView = mlContext.Data.LoadFromTextFile<Simulation>(TestDataPath, hasHeader: true, separatorChar: ',');

        private static string LabelColumnName = "CycleTime";
        private static uint ExperimentTime = 10;

        static void Main(string[] args) //If args[0] == "svg" a vector-based chart will be created instead a .png chart
        {


            // STEP 1: Common data loading configuration

            // Create, train, evaluate and save a model
            //BuildTrainEvaluateAndSaveModel(mlContext);

            // Run an AutoML experiment on the dataset.
            var experimentResult = RunAutoMLExperiment(mlContext);

            // Evaluate the model and print metrics.
            EvaluateModel(mlContext, experimentResult.BestRun.Model, experimentResult.BestRun.TrainerName);

            // Save / persist the best model to a.ZIP file.
            SaveModel(mlContext, experimentResult.BestRun.Model);

            // Make a single test prediction loading the model from .ZIP file
            TestSinglePrediction(mlContext);

            // Paint regression distribution chart for a number of elements read from a Test DataSet file
            // PlotRegressionChart(mlContext, TestDataPath, 100, args);

            Console.WriteLine("Press any key to exit..");
            Console.ReadLine();
        }

        private static ExperimentResult<RegressionMetrics> RunAutoMLExperiment(MLContext mlContext)
        {
            // STEP 1: Common data loading configuration
            /*            IDataView trainingDataView = mlContext.Data.LoadFromTextFile<Simulation>(TrainDataPath, hasHeader: true, separatorChar: ',');
                        IDataView testDataView = mlContext.Data.LoadFromTextFile<Simulation>(TestDataPath, hasHeader: true, separatorChar: ',');*/

            // STEP 2: Display first few rows of the training data
            ConsoleHelper.ShowDataViewInConsole(mlContext, trainDataView);

            // STEP 3: Initialize our user-defined progress handler that AutoML will 
            // invoke after each model it produces and evaluates.
            var progressHandler = new RegressionExperimentProgressHandler();

            // STEP 4: Run AutoML regression experiment
            ConsoleHelper.ConsoleWriteHeader("=============== Training the model ===============");
            Console.WriteLine($"Running AutoML regression experiment for {ExperimentTime} seconds...");
            ExperimentResult<RegressionMetrics> experimentResult = mlContext.Auto()
                .CreateRegressionExperiment(ExperimentTime)
                .Execute(trainDataView, LabelColumnName, progressHandler: progressHandler);

            // Print top models found by AutoML
            Console.WriteLine();
            PrintTopModels(experimentResult);

            return experimentResult;
        }

        /*            // STEP 5: Evaluate the model and print metrics
                    ConsoleHelper.ConsoleWriteHeader("===== Evaluating model's accuracy with test data =====");
                    RunDetail<RegressionMetrics> best = experimentResult.BestRun;
                    ITransformer trainedModel = best.Model;
                    IDataView predictions = trainedModel.Transform(testDataView);
                    var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: LabelColumnName, scoreColumnName: "Score");
                    // Print metrics from top model
                    ConsoleHelper.PrintRegressionMetrics(best.TrainerName, metrics);*/

        // STEP 6: Save/persist the trained model to a .ZIP file
        /*            mlContext.Model.Save(trainedModel, trainingDataView.Schema, ModelPath);

                    Console.WriteLine("The model is saved to {0}", ModelPath);

                    return trainedModel;
                }*/

        private static void EvaluateModel(MLContext mlContext, ITransformer model, string trainerName)
        {
            ConsoleHelper.ConsoleWriteHeader("===== Evaluating model's accuracy with test data =====");
            IDataView predictions = model.Transform(testDataView);
            var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: LabelColumnName, scoreColumnName: "Score");
            ConsoleHelper.PrintRegressionMetrics(trainerName, metrics);
        }

        private static void TestSinglePrediction(MLContext mlContext)
        {
            ConsoleHelper.ConsoleWriteHeader("=============== Testing prediction engine ===============");

            // Sample: 
            // vendor_id,rate_code,passenger_count,trip_time_in_secs,trip_distance,payment_type,fare_amount
            // VTS,1,1,1140,3.75,CRD,15.5

            var cycleTimeSample = new Simulation
            {
                Time = 3600F,
                Material = 3130253F,
                InDueTotal = 17F,
                Consumab = 19918.82F,
                CycleTime = 1041.941176F, // 1041.941176F,
                Assembly = 5.849511F,
                Lateness = -1341.529F,
                Total = 17F,
            };

            ITransformer trainedModel = mlContext.Model.Load(ModelPath, out var modelInputSchema);

            // Create prediction engine related to the loaded trained model.
            var predEngine = mlContext.Model.CreatePredictionEngine<Simulation, CycleTimePrediction>(trainedModel);

            // Score.
            var predictedResult = predEngine.Predict(cycleTimeSample);

            Console.WriteLine("**********************************************************************");
            Console.WriteLine($"Predicted CycleTime: {predictedResult.CycleTime:0.####}, actual CycleTime: {cycleTimeSample.CycleTime}");
            Console.WriteLine($"Difference in %: {((predictedResult.CycleTime / cycleTimeSample.CycleTime) - 1) * 100}");
            Console.WriteLine("**********************************************************************");
        }

        private static void SaveModel(MLContext mlContext, ITransformer model)
        {
            ConsoleHelper.ConsoleWriteHeader("=============== Saving the model ===============");
            mlContext.Model.Save(model, trainDataView.Schema, ModelPath);
            Console.WriteLine($"The model is saved to {ModelPath}");
        }
    
/*        private static void PlotRegressionChart(MLContext mlContext,
                                        string testDataSetPath,
                                        int numberOfRecordsToRead,
                                        string[] args)
        {
            ITransformer trainedModel;
            using (var stream = new FileStream(ModelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                trainedModel = mlContext.Model.Load(stream, out var modelInputSchema);
            }

            // Create prediction engine related to the loaded trained model
            var predFunction = mlContext.Model.CreatePredictionEngine<Simulation, CycleTimePrediction>(trainedModel);

            string chartFileName = "";
            using (var pl = new PLStream())
            {
                // use SVG backend and write to SineWaves.svg in current directory.
                if (args.Length == 1 && args[0] == "svg")
                {
                    pl.sdev("svg");
                    chartFileName = "CycleTimeRegressionDistribution.svg";
                    pl.sfnam(chartFileName);
                }
                else
                {
                    pl.sdev("pngcairo");
                    chartFileName = "CycleTimeRegressionDistribution.png";
                    pl.sfnam(chartFileName);
                }

                // use white background with black foreground.
                pl.spal0("cmap0_alternate.pal");

                // Initialize plplot.
                pl.init();

                // Set axis limits.
                const int xMinLimit = 0;
                const int xMaxLimit = 35; // Rides larger than $35 are not shown in the chart.
                const int yMinLimit = 0;
                const int yMaxLimit = 35;  // Rides larger than $35 are not shown in the chart.
                pl.env(xMinLimit, xMaxLimit, yMinLimit, yMaxLimit, AxesScale.Independent, AxisBox.BoxTicksLabelsAxes);

                // Set scaling for mail title text 125% size of default.
                pl.schr(0, 1.25);

                // The main title.
                pl.lab("Measured", "Predicted", "Distribution of Taxi Fare Prediction");

                // plot using different colors
                // see http://plplot.sourceforge.net/examples.php?demo=02 for palette indices
                pl.col0(1);

                int totalNumber = numberOfRecordsToRead;
                var testData = new SimulationCsvReader().GetDataFromCsv(testDataSetPath, totalNumber).ToList();

                // This code is the symbol to paint
                var code = (char)9;

                // plot using other color
                //pl.col0(9); //Light Green
                //pl.col0(4); //Red
                pl.col0(2); //Blue

                double yTotal = 0;
                double xTotal = 0;
                double xyMultiTotal = 0;
                double xSquareTotal = 0;

                for (int i = 0; i < testData.Count; i++)
                {
                    var x = new double[1];
                    var y = new double[1];

                    // Make Prediction.
                    var cycleTimePrediction = predFunction.Predict(testData[i]);

                    x[0] = testData[i].CycleTime;
                    y[0] = cycleTimePrediction.CycleTime;

                    // Paint a dot
                    pl.poin(x, y, code);

                    xTotal += x[0];
                    yTotal += y[0];

                    double multi = x[0] * y[0];
                    xyMultiTotal += multi;

                    double xSquare = x[0] * x[0];
                    xSquareTotal += xSquare;

                    double ySquare = y[0] * y[0];

                    Console.WriteLine("-------------------------------------------------");
                    Console.WriteLine($"Predicted : {cycleTimePrediction.CycleTime}");
                    Console.WriteLine($"Actual:    {testData[i].CycleTime}");
                    Console.WriteLine("-------------------------------------------------");
                }

                // Regression Line calculation explanation:
                // https://www.khanacademy.org/math/statistics-probability/describing-relationships-quantitative-data/more-on-regression/v/regression-line-example

                double minY = yTotal / totalNumber;
                double minX = xTotal / totalNumber;
                double minXY = xyMultiTotal / totalNumber;
                double minXsquare = xSquareTotal / totalNumber;

                double m = ((minX * minY) - minXY) / ((minX * minX) - minXsquare);

                double b = minY - (m * minX);

                // Generic function for Y for the regression line
                // y = (m * x) + b;

                double x1 = 1;

                // Function for Y1 in the line
                double y1 = (m * x1) + b;

                double x2 = 39;

                // Function for Y2 in the line
                double y2 = (m * x2) + b;

                var xArray = new double[2];
                var yArray = new double[2];
                xArray[0] = x1;
                yArray[0] = y1;
                xArray[1] = x2;
                yArray[1] = y2;

                pl.col0(4);
                pl.line(xArray, yArray);

                // End page (writes output to disk)
                pl.eop();

                // Output version of PLplot
                pl.gver(out var verText);
                Console.WriteLine("PLplot version " + verText);

            } // The pl object is disposed here

            // Open chart file in Microsoft Photos App (or default app for .svg or .png, like browser)

            Console.WriteLine("Showing chart...");
            var p = new Process();
            string chartFileNamePath = @".\" + chartFileName;
            p.StartInfo = new ProcessStartInfo(chartFileNamePath)
            {
                UseShellExecute = true
            };
            p.Start();
        }*/
        private static void PrintTopModels(ExperimentResult<RegressionMetrics> experimentResult)
        {
            // Get top few runs ranked by R-Squared.
            // R-Squared is a metric to maximize, so OrderByDescending() is correct.
            // For RMSE and other regression metrics, OrderByAscending() is correct.
            var topRuns = experimentResult.RunDetails
                .Where(r => r.ValidationMetrics != null && !double.IsNaN(r.ValidationMetrics.RSquared))
                .OrderByDescending(r => r.ValidationMetrics.RSquared).Take(3);

            Console.WriteLine("Top models ranked by R-Squared --");
            ConsoleHelper.PrintRegressionMetricsHeader();
            for (var i = 0; i < topRuns.Count(); i++)
            {
                var run = topRuns.ElementAt(i);
                ConsoleHelper.PrintIterationMetrics(i + 1, run.TrainerName, run.ValidationMetrics, run.RuntimeInSeconds);
            }
        }

        public static string GetAbsolutePath(string relativePath)
        {
            var _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }
    }

    public class SimulationCsvReader
    {
        public IEnumerable<Simulation> GetDataFromCsv(string dataLocation, int numMaxRecords)
        {
            IEnumerable<Simulation> records =
                File.ReadAllLines(dataLocation)
                .Skip(1)
                .Select(x => x.Split(','))
                .Select(x => new Simulation()
                {
                    Time = int.Parse(x[0]),
                    Material = float.Parse(x[1]),
                    InDueTotal = float.Parse(x[2]),
                    Consumab = float.Parse(x[3]),
                    CycleTime = float.Parse(x[4]),
                    Assembly = float.Parse(x[5])
                })
                .Take<Simulation>(numMaxRecords);

            return records;
        }
    }
}