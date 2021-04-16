using Microsoft.ML.Data;

namespace ML_API_Advanced.DataStuctures
{
    public class SimulationKpis
    {
        [ColumnName("Assembly"), LoadColumn(0)]
        public float Assembly { get; set; }

        [ColumnName("Material"), LoadColumn(1)]
        public float Material { get; set; }

        [ColumnName("OpenOrders"), LoadColumn(2)]
        public float OpenOrders { get; set; }

        [ColumnName("NewOrders"), LoadColumn(3)]
        public float NewOrders { get; set; }

        [ColumnName("TotalWork"), LoadColumn(4)]
        public float TotalWork { get; set; }

        [ColumnName("TotalSetup"), LoadColumn(5)]
        public float TotalSetup { get; set; }

        [ColumnName("SumDuration"), LoadColumn(6)]
        public float SumDuration { get; set; }

        [ColumnName("SumOperation"), LoadColumn(7)]
        public float SumOperation { get; set; }

        [ColumnName("ProductionOrders"), LoadColumn(8)]
        public float ProductionOrders { get; set; }

        [ColumnName("CycleTime"), LoadColumn(9)]
        public float CycleTime { get; set; }
    }
}
