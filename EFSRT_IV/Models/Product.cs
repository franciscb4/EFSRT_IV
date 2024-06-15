namespace EFSRT_IV.Models
{
    public class Product
    {
        public int id { get; set; }
        public string name { get; set; }
        public int category { get; set; }
        public double price { get; set; }
        public int stock { get; set; }
        public bool state{ get; set; }
    }
}
