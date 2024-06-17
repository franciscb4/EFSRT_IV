using System;
using System.Collections.Generic;

namespace DB.Models;

public partial class Gasto
{
    public int IdGasto { get; set; }

    public decimal Monto { get; set; }

    public DateTime Fecha { get; set; }

    public int IdCategoriaGasto { get; set; }

    public int IdProveedor { get; set; }

    public int IdTienda { get; set; }

    public virtual ICollection<DetalleGasto> DetalleGastos { get; set; } = new List<DetalleGasto>();

    public virtual CategoriaGasto IdCategoriaGastoNavigation { get; set; } = null!;

    public virtual Proveedor IdProveedorNavigation { get; set; } = null!;

    public virtual Tiendum IdTiendaNavigation { get; set; } = null!;
}
