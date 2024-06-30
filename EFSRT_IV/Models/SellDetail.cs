namespace EFSRT_IV.Models
{
    public class SellDetail
    {
        public int id { get; set; }
        public string product { get; set; }
        public int quantity { get; set; }
        public decimal singlePrice { get; set; }
        public decimal subtotal { get; set; }
    }
}
