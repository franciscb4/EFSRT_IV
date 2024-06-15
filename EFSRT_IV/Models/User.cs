using System.ComponentModel.DataAnnotations;

namespace EFSRT_IV.Models
{
    public class User
    {
        public int id { get; set; }
        [Required(ErrorMessage = "Campo requerido.")]
        [StringLength(255, ErrorMessage = "Máximo 255 digitos.")]
        public string name { get; set; }
        
        [Required(ErrorMessage = "Campo requerido.")]
        [StringLength(255, ErrorMessage = "Máximo 255 digitos.")]
        [RegularExpression(
            @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",
            ErrorMessage = "El campo debe ser un correo electrónico válido."
            )]
        public string email { get; set; }
        
        [Required(ErrorMessage = "Campo requerido.")]
        [StringLength(255, ErrorMessage = "Máximo 255 digitos.")]
        public string password { get; set; }
        
        [Required(ErrorMessage = "Campo requerido.")]
        [StringLength(8, ErrorMessage = "Máximo 8 digitos.")]
        public string dni { get; set; }
        
        [Required(ErrorMessage = "Campo requerido.")]
        [StringLength(9, ErrorMessage = "Máximo 9 digitos.")]
        [Range(900000000, 999999999, ErrorMessage = "El campo debe ser un múmero válido")]
        public string phone { get; set; }
        
        public bool state { get; set; }


        public User()
        {
            id = 0;
            name = "";
            email = "";
            password = "";
            dni = "";
            phone = "";
            state = false;
        }
    }
}
