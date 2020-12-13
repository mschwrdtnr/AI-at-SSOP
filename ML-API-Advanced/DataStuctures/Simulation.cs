using Microsoft.ML.Data;

namespace ML_API_Advanced.DataStuctures
{
    public class Simulation
    {
        [ColumnName("Time"), LoadColumn(0)]
        public float Time { get; set; }


        [ColumnName("Material"), LoadColumn(1)]
        public float Material { get; set; }


        [ColumnName("InDueTotal"), LoadColumn(2)]
        public float InDueTotal { get; set; }


        [ColumnName("Consumab"), LoadColumn(3)]
        public float Consumab { get; set; }


        [ColumnName("CycleTime"), LoadColumn(4)]
        public float CycleTime { get; set; }


        [ColumnName("Assembly"), LoadColumn(5)]
        public float Assembly { get; set; }


        [ColumnName("Lateness"), LoadColumn(6)]
        public float Lateness { get; set; }


        [ColumnName("Total"), LoadColumn(7)]
        public float Total { get; set; }
    }
}
