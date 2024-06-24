namespace EFSRT_IV.Models
{
    public class ChartViewModel
    {
        public List<string>? IngresosLabels { get; set; }
        public List<decimal>? IngresosData { get; set; }
        public List<string>? GastosLabels { get; set; }
        public List<decimal>? GastosData { get; set; }
        public List<string>? VentasLabels { get; set; }
        public List<decimal>? VentasData { get; set; }
    }
}
