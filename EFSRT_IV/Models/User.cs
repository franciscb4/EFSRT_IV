namespace EFSRT_IV.Models
{
    public class User
    {
        public string id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string phone{ get; set; }
        public string password{ get; set; }
        public User()
        {
            id = "";
            name = "";
            email = "";
            phone = "";
            password = "";
        }
    }
}
