namespace EFSRT_IV.Models
{
    public class Product
    {
        public int id { get; set; }
        public string name { get; set; }
        public string image { get; set; }
        public string category { get; set; }
        public decimal price { get; set; }
        public int stock { get; set; }
        public int minStock { get; set; }
        public int maxStock { get; set; }
        public bool state{ get; set; }

        public Product()
        {
            id = 0;
            name = "";
            image = "";
            price = 0;
            stock = 0;
            minStock = 0;
            maxStock = 0;
            state = false;
        }
    }
}
