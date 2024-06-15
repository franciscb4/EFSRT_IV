using System;
using System.Collections.Generic;

namespace DB.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string Nombre { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string Clave { get; set; } = null!;

    public string Dni { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public bool Estado { get; set; }

    public virtual ICollection<Tiendum> Tienda { get; set; } = new List<Tiendum>();
}
