namespace EFSRT_IV.Models
{
    public class SellDetail
    {
        public int id { get; set; }
        public string product { get; set; }
        public int quantity { get; set; }
        public double singlePrice{ get; set; }
        public double subtotal { get; set; }

        public SellDetail()
        {
            id = 0;
            product = "";
            quantity = 0;
            singlePrice = 0;
            subtotal = 0;
        }
    }
}
