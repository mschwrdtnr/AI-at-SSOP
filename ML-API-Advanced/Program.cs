using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ML_API_Advanced.DataStuctures;
using Common;
using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
using PLplot;

/*Description:
 - Program allow to use AutoML to find the best Trainer for the given train data
 - Inspired of https://docs.microsoft.com/en-us/dotnet/machine-learning/tutorials/predict-prices-with-model-builder 
    and https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/end-to-end-apps/Forecasting-Sales
 */

namespace ML_API_Advanced
{
    internal static class Program
    {
        private static string rootDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../"));
        // You can choose different trained models --> This path with override  
        private static string ModelPath = Path.Combine(rootDir, "MLModel_FastTree.zip");
        private static string trainDataPath = Path.Combine(rootDir, "Data/CycleTime_train_diffAT.csv");
        private static string evalDataPath = Path.Combine(rootDir, "Data/CycleTime_eval_defaultAT_00152.csv");
        
        private static MLContext mlContext = new MLContext();

        private static IDataView trainDataView = mlContext.Data.LoadFromTextFile<Simulation>(trainDataPath, hasHeader: true, separatorChar: ';');
        private static IDataView evalDataView = mlContext.Data.LoadFromTextFile<Simulation>(evalDataPath, hasHeader: true, separatorChar: ';');

        private static string LabelColumnName = "CycleTime";
        // Experiment Time for AutoMLExperiment
        private static uint ExperimentTime = 100; // If to high RunAutoMLExperiment sometimes don't finish
        private static int NumberOfPredictions = 161; //Used for evaluation

        static void Main(string[] args) 
        {
            // Run an AutoML experiment on the dataset.
            //var experimentResult = RunAutoMLExperiment(mlContext);

            // Evaluate the model and print metrics.
            //EvaluateModel(mlContext, experimentResult.BestRun.Model, experimentResult.BestRun.TrainerName);

            // To evaluate a model without RunAutoMLExperiment
            //EvaluateSavedModel(mlContext, evalDataView);

            // Save / persist the best model to a.ZIP file.
            //SaveModel(mlContext, experimentResult.BestRun.Model);

            // Make a single test prediction loading the model from .ZIP file
            //TestSinglePrediction(mlContext);

            //Predict X Values
            PredictWithSavedModel(mlContext, NumberOfPredictions);

            // Paint regression distribution chart for a number of elements read from a Test DataSet file
            //PlotRegressionChart(mlContext, TestDataPath, 100, args);

            Console.WriteLine("Press any key to exit..");
            Console.ReadLine();
        }

        private static ExperimentResult<RegressionMetrics> RunAutoMLExperiment(MLContext mlContext)
        {

            // Display first few rows of the training data
            ConsoleHelper.ShowDataViewInConsole(mlContext, trainDataView);

            // Initialize our user-defined progress handler that AutoML will 
            // invoke after each model it produces and evaluates.
            var progressHandler = new RegressionExperimentProgressHandler();

            // Run AutoML regression experiment
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

        private static void EvaluateModel(MLContext mlContext, ITransformer model, string trainerName)
        {
            ConsoleHelper.ConsoleWriteHeader("===== Evaluating model's accuracy with eval data =====");
            IDataView predictions = model.Transform(evalDataView);
            var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: LabelColumnName, scoreColumnName: "Score");
            ConsoleHelper.PrintRegressionMetrics(trainerName, metrics);
        }

        private static void EvaluateSavedModel(MLContext mlContext, IDataView evalDataView)
        {
            var evalDataName = evalDataPath.Substring(evalDataPath.IndexOf("CycleTime"));
            ITransformer trainedModel = mlContext.Model.Load(ModelPath, out var modelInputSchema);

            IDataView predictions = trainedModel.Transform(evalDataView);
            var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: LabelColumnName, scoreColumnName: "Score");
            ConsoleHelper.PrintRegressionMetrics(evalDataName, metrics);
        }

        private static void SaveModel(MLContext mlContext, ITransformer model)
        {
            ConsoleHelper.ConsoleWriteHeader("=============== Saving the model ===============");
            mlContext.Model.Save(model, trainDataView.Schema, ModelPath);
            Console.WriteLine($"The model is saved to {ModelPath}");
        }

        private static void PredictWithSavedModel(MLContext mlContext, int numberOfPredictions)
        {
            ITransformer trainedModel = mlContext.Model.Load(ModelPath, out var modelInputSchema);

            // Create prediction engine related to the loaded trained model.
            var predEngine = mlContext.Model.CreatePredictionEngine<Simulation, CycleTimePrediction>(trainedModel);

            Console.WriteLine("======================================================================================================");
            Console.WriteLine($"================== Visualize/eval {numberOfPredictions} predictions for model Model.zip ==================");
            //Visualize 10 evals comparing prediction with actual/observed values from the eval dataset
            ModelScoringTester.VisualizeSomePredictions(mlContext, evalDataPath, predEngine, numberOfPredictions);
        }
        private static void TestSinglePrediction(MLContext mlContext)
        {
            ConsoleHelper.ConsoleWriteHeader("=============== Testing prediction engine ===============");

            var cycleTimeSample = new Simulation
            {
                Time = 4800,
                Lateness = -1295.297297F,
                Assembly = 4.962484375F,
                Total = 37,
                CycleTime = 1082.081081F,
                Consumab = 19890.6350F,
                Material = 3197679.985F,
                InDueTotal = 37
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

        // Doesn't work
        //    private static void PlotRegressionChart(MLContext mlContext,
        //                                    string evalDataSetPath,
        //                                    int numberOfRecordsToRead,
        //                                    string[] args)
        //    {
        //        ITransformer trainedModel;
        //        using (var stream = new FileStream(ModelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
        //        {
        //            trainedModel = mlContext.Model.Load(stream, out var modelInputSchema);
        //        }

        //        // Create prediction engine related to the loaded trained model
        //        var predFunction = mlContext.Model.CreatePredictionEngine<Simulation, CycleTimePrediction>(trainedModel);

        //        string chartFileName = "";
        //        using (var pl = new PLStream())
        //        {
        //            // use SVG backend and write to SineWaves.svg in current directory.
        //            if (args.Length == 1 && args[0] == "svg")
        //            {
        //                pl.sdev("svg");
        //                chartFileName = "CycleTimeRegressionDistribution.svg";
        //                pl.sfnam(chartFileName);
        //            }
        //            else
        //            {
        //                pl.sdev("pngcairo");
        //                chartFileName = "CycleTimeRegressionDistribution.png";
        //                pl.sfnam(chartFileName);
        //            }

        //            // use white background with black foreground.
        //            pl.spal0("cmap0_alternate.pal");

        //            // Initialize plplot.
        //            pl.init();

        //            // Set axis limits.
        //            const int xMinLimit = 0;
        //            const int xMaxLimit = 35; // Rides larger than $35 are not shown in the chart.
        //            const int yMinLimit = 0;
        //            const int yMaxLimit = 35;  // Rides larger than $35 are not shown in the chart.
        //            pl.env(xMinLimit, xMaxLimit, yMinLimit, yMaxLimit, AxesScale.Independent, AxisBox.BoxTicksLabelsAxes);

        //            // Set scaling for mail title text 125% size of default.
        //            pl.schr(0, 1.25);

        //            // The main title.
        //            pl.lab("Measured", "Predicted", "Distribution of Taxi Fare Prediction");

        //            // plot using different colors
        //            // see http://plplot.sourceforge.net/examples.php?demo=02 for palette indices
        //            pl.col0(1);

        //            int totalNumber = numberOfRecordsToRead;
        //            var evalData = new SimulationCsvReader().GetDataFromCsv(evalDataSetPath, totalNumber).ToList();

        //            // This code is the symbol to paint
        //            var code = (char)9;

        //            // plot using other color
        //            //pl.col0(9); //Light Green
        //            //pl.col0(4); //Red
        //            pl.col0(2); //Blue

        //            double yTotal = 0;
        //            double xTotal = 0;
        //            double xyMultiTotal = 0;
        //            double xSquareTotal = 0;

        //            for (int i = 0; i < evalData.Count; i++)
        //            {
        //                var x = new double[1];
        //                var y = new double[1];

        //                // Make Prediction.
        //                var cycleTimePrediction = predFunction.Predict(evalData[i]);

        //                x[0] = evalData[i].CycleTime;
        //                y[0] = cycleTimePrediction.CycleTime;

        //                // Paint a dot
        //                pl.poin(x, y, code);

        //                xTotal += x[0];
        //                yTotal += y[0];

        //                double multi = x[0] * y[0];
        //                xyMultiTotal += multi;

        //                double xSquare = x[0] * x[0];
        //                xSquareTotal += xSquare;

        //                double ySquare = y[0] * y[0];

        //                Console.WriteLine("-------------------------------------------------");
        //                Console.WriteLine($"Predicted : {cycleTimePrediction.CycleTime}");
        //                Console.WriteLine($"Actual:    {evalData[i].CycleTime}");
        //                Console.WriteLine("-------------------------------------------------");
        //            }

        //            // Regression Line calculation explanation:
        //            // https://www.khanacademy.org/math/statistics-probability/describing-relationships-quantitative-data/more-on-regression/v/regression-line-example

        //            double minY = yTotal / totalNumber;
        //            double minX = xTotal / totalNumber;
        //            double minXY = xyMultiTotal / totalNumber;
        //            double minXsquare = xSquareTotal / totalNumber;

        //            double m = ((minX * minY) - minXY) / ((minX * minX) - minXsquare);

        //            double b = minY - (m * minX);

        //            // Generic function for Y for the regression line
        //            // y = (m * x) + b;

        //            double x1 = 1;

        //            // Function for Y1 in the line
        //            double y1 = (m * x1) + b;

        //            double x2 = 39;

        //            // Function for Y2 in the line
        //            double y2 = (m * x2) + b;

        //            var xArray = new double[2];
        //            var yArray = new double[2];
        //            xArray[0] = x1;
        //            yArray[0] = y1;
        //            xArray[1] = x2;
        //            yArray[1] = y2;

        //            pl.col0(4);
        //            pl.line(xArray, yArray);

        //            // End page (writes output to disk)
        //            pl.eop();

        //            // Output version of PLplot
        //            pl.gver(out var verText);
        //            Console.WriteLine("PLplot version " + verText);

        //        } // The pl object is disposed here

        //        // Open chart file in Microsoft Photos App (or default app for .svg or .png, like browser)

        //        Console.WriteLine("Showing chart...");
        //        var p = new Process();
        //        string chartFileNamePath = @".\" + chartFileName;
        //        p.StartInfo = new ProcessStartInfo(chartFileNamePath)
        //        {
        //            UseShellExecute = true
        //        };
        //        p.Start();
        //    }
        //}

        //public class SimulationCsvReader
        //{
        //    public IEnumerable<Simulation> GetDataFromCsv(string dataLocation, int numMaxRecords)
        //    {
        //        IEnumerable<Simulation> records =
        //            File.ReadAllLines(dataLocation)
        //            .Skip(1)
        //            .Select(x => x.Split(';'))
        //            .Select(x => new Simulation()
        //            {
        //                Time = double.Parse(x[0]),
        //                Lateness = double.Parse(x[1]),
        //                Assembly = double.Parse(x[2]),
        //                Total = double.Parse(x[3]),
        //                CycleTime = double.Parse(x[4]),
        //                Consumab = double.Parse(x[5]),
        //                Material = double.Parse(x[6]),
        //                InDueTotal = double.Parse(x[7])
        //            })
        //            .Take<Simulation>(numMaxRecords);

        //        return records;
        //    }
    }
}