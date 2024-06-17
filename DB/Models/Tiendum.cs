using System;
using System.Collections.Generic;

namespace DB.Models;

public partial class Tiendum
{
    public int IdTienda { get; set; }

    public string Nombre { get; set; } = null!;

    public int IdUsuario { get; set; }

    public virtual ICollection<Gasto> Gastos { get; set; } = new List<Gasto>();

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();

    public virtual ICollection<Ventum> Venta { get; set; } = new List<Ventum>();
}
