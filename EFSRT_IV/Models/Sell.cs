namespace EFSRT_IV.Models
{
    public class Sell
    {
        public int id { get; set; }
        public string client { get; set; }
        public decimal total{ get; set; }
        public DateTime date { get; set; }
        public List<SellDetail> details { get; set; }

    }
}
