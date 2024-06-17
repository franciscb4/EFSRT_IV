using System.ComponentModel.DataAnnotations;

namespace EFSRT_IV.Models
{
    public class Product
    {
        public int id { get; set; }
        
        [Required(ErrorMessage = "Campo requerido.")]
        [StringLength(255, ErrorMessage = "Máximo 255 digitos.")]
        public string name { get; set; }
        
        [Required(ErrorMessage = "Campo requerido.")]
        public int category { get; set; }
        
        [Required(ErrorMessage = "Campo requerido.")]
        [Range(0, 9999999999999999.99, ErrorMessage = "Precio inválido.")]
        public decimal price { get; set; }
        
        [Required(ErrorMessage = "Campo requerido.")]
        [Range(0, 9999999999999999, ErrorMessage = "Valor inválido.")]
        public int stock { get; set; }
        public bool state{ get; set; }

        public Product()
        {
            id = 0;
            name = "";
            category = 0;
            price = 0;
            stock = 0;
            state = false;
        }
    }
}
