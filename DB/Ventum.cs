using System;
using System.Collections.Generic;

namespace DB;

public partial class Ventum
{
    public int IdVenta { get; set; }

    public int? IdCliente { get; set; }

    public decimal Monto { get; set; }

    public DateTime Fecha { get; set; }

    public int IdTienda { get; set; }

    public virtual ICollection<DetalleVentum> DetalleVenta { get; set; } = new List<DetalleVentum>();

    public virtual Cliente? IdClienteNavigation { get; set; }

    public virtual Tiendum IdTiendaNavigation { get; set; } = null!;
}
