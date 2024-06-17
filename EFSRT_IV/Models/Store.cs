using System.ComponentModel.DataAnnotations;

namespace EFSRT_IV.Models
{
    public class Store
    {
        public int id { get; set; }
        
        [Required(ErrorMessage = "Campo requerido.")]
        [StringLength(255, ErrorMessage = "Máximo 255 digitos.")]
        public string businessName { get; set; }
        
        [Required(ErrorMessage = "Campo requerido.")]
        [StringLength(255, ErrorMessage = "Máximo 255 digitos.")]
        public string ruc { get; set; }
        public bool state { get; set; }
    }
}
