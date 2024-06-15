using System;
using System.Collections.Generic;

namespace DB.Models;

public partial class Ventum
{
    public int IdVenta { get; set; }

    public decimal Monto { get; set; }

    public DateTime Fecha { get; set; }

    public int IdTienda { get; set; }

    public virtual ICollection<DetalleVentum> DetalleVenta { get; set; } = new List<DetalleVentum>();

    public virtual Tiendum IdTiendaNavigation { get; set; } = null!;
}
