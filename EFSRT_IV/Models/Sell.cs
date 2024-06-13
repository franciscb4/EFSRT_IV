namespace EFSRT_IV.Models
{
    public class Sell
    {
        public int id { get; set; }
        public string client { get; set; }
        public double total{ get; set; }
        public DateTime date { get; set; }
        public List<SellDetail> details { get; set; }

        public Sell()
        {
            id = 0;
            client = "";
            total = 0;
            date = DateTime.Now;
            details = new List<SellDetail>();
        }
    }
}
