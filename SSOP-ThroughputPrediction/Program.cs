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

/*Description:
 - Program allow to use AutoML to find the best Trainer for the given train data
 - Inspired of https://docs.microsoft.com/en-us/dotnet/machine-learning/tutorials/predict-prices-with-model-builder 
    and https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/end-to-end-apps/Forecasting-Sales
 */

/* TODOS:
 * - Implement regression chart
 */

namespace ML_API_Advanced
{
    internal static class Program
    {
        private static string rootDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../"));

        // You can choose different trained models --> This path with override  
        private static string ModelPath = Path.Combine(rootDir, "MLModels/");
        private static string trainDataPath = Path.Combine(rootDir, "Data/10028_training_001.csv");
        private static string evalDataPath = Path.Combine(rootDir, "Data/10029_training_001.csv");

        private static MLContext mlContext = new MLContext();

        private static IDataView trainDataView = mlContext.Data.LoadFromTextFile<SimulationKpis>(trainDataPath, hasHeader: true, separatorChar: ',');
        private static IDataView evalDataView = mlContext.Data.LoadFromTextFile<SimulationKpis>(evalDataPath, hasHeader: true, separatorChar: ',');

        private static string LabelColumnName = "CycleTime"; // Target Column; Variable which should be forecasted

        // Experiment Time for AutoMLExperiment
        private static uint ExperimentTime = 300; // If > 300 RunAutoMLExperiment will sometimes not finish

        private static int NumberOfPredictions = 158; //Used for evaluation

        static void Main(string[] args)
        {
            // Run an AutoML experiment on the dataset.
            var experimentResult = RunAutoMLExperiment(mlContext);

            // Evaluate the model and print metrics.
            EvaluateModel(mlContext, experimentResult.BestRun.Model, experimentResult.BestRun.TrainerName);

            // Save / persist the best model to a .ZIP file. !This will override the actual .ZIP file
            SaveModel(mlContext, experimentResult.BestRun.Model, experimentResult.BestRun.TrainerName);

            // To evaluate a model without RunAutoMLExperiment
            //EvaluateSavedModel(mlContext, evalDataView);

            //Predict NumberOfPredictions Values
            //PredictWithSavedModel(mlContext, NumberOfPredictions);

            // Paint regression distribution chart for a number of elements read from a Test DataSet file !NOT WORKING!
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

        private static void SaveModel(MLContext mlContext, ITransformer model, string trainerName)
        {
            ModelPath = ModelPath + "ML_" + trainerName + ".zip";
            ConsoleHelper.ConsoleWriteHeader("=============== Saving the model ===============");
            mlContext.Model.Save(model, trainDataView.Schema, ModelPath);
            Console.WriteLine($"The model is saved to {ModelPath}");
        }

        private static void PredictWithSavedModel(MLContext mlContext, int numberOfPredictions)
        {
            ITransformer trainedModel = mlContext.Model.Load(ModelPath, out var modelInputSchema);

            // Create prediction engine related to the loaded trained model.
            var predEngine = mlContext.Model.CreatePredictionEngine<SimulationKpis, CycleTimePrediction>(trainedModel);

            Console.WriteLine("======================================================================================================");
            Console.WriteLine($"================== Visualize/eval {numberOfPredictions} predictions for model Model.zip ==================");
            //Visualize 10 evals comparing prediction with actual/observed values from the eval dataset
            ModelScoringTester.VisualizeSomePredictions(mlContext, evalDataPath, predEngine, numberOfPredictions);
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
    }
}