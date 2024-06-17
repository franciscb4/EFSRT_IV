using System;
using System.Collections.Generic;

namespace DB.Models;

public partial class DetalleGasto
{
    public int IdDetalleGasto { get; set; }

    public int? IdProducto { get; set; }

    public string? Descripcion { get; set; }

    public decimal Monto { get; set; }

    public int IdGasto { get; set; }

    public virtual Gasto IdGastoNavigation { get; set; } = null!;

    public virtual Producto? IdProductoNavigation { get; set; }
}
