using Microsoft.ML.Data;

namespace ML_API_Advanced.DataStuctures
{
    public class SimulationKpis
    {
        [ColumnName("Lateness_t2"), LoadColumn(0)]
        public float Lateness_t2 { get; set; }

        [ColumnName("Assembly_t2"), LoadColumn(1)]
        public float Assembly_t2 { get; set; }

        [ColumnName("Total_t2"), LoadColumn(2)]
        public float Total_t2 { get; set; }

        [ColumnName("CycleTime_t2"), LoadColumn(3)]
        public float CycleTime_t2 { get; set; }

        [ColumnName("Consumab_t2"), LoadColumn(4)]
        public float Consumab_t2 { get; set; }

        [ColumnName("Material_t2"), LoadColumn(5)]
        public float Material_t2 { get; set; }

        [ColumnName("InDueTotal_t2"), LoadColumn(6)]
        public float InDueTotal_t2 { get; set; }

        [ColumnName("Lateness_t1"), LoadColumn(7)]
        public float Lateness_t1 { get; set; }

        [ColumnName("Assembly_t1"), LoadColumn(8)]
        public float Assembly_t1 { get; set; }

        [ColumnName("Total_t1"), LoadColumn(9)]
        public float Total_t1 { get; set; }

        [ColumnName("CycleTime_t1"), LoadColumn(10)]
        public float CycleTime_t1 { get; set; }

        [ColumnName("Consumab_t1"), LoadColumn(11)]
        public float Consumab_t1 { get; set; }

        [ColumnName("Material_t1"), LoadColumn(12)]
        public float Material_t1 { get; set; }

        [ColumnName("InDueTotal_t1"), LoadColumn(13)]
        public float InDueTotal_t1 { get; set; }

        [ColumnName("Lateness_t0"), LoadColumn(14)]
        public float Lateness_t0 { get; set; }

        [ColumnName("Assembly_t0"), LoadColumn(15)]
        public float Assembly_t0 { get; set; }

        [ColumnName("Total_t0"), LoadColumn(16)]
        public float Total_t0 { get; set; }

        [ColumnName("Consumab_t0"), LoadColumn(17)]
        public float Consumab_t0 { get; set; }

        [ColumnName("Material_t0"), LoadColumn(18)]
        public float Material_t0 { get; set; }

        [ColumnName("InDueTotal_t0"), LoadColumn(19)]
        public float InDueTotal_t0 { get; set; }

        [ColumnName("CycleTime_t0"), LoadColumn(20)]
        public float CycleTime_t0 { get; set; }
    }
}
