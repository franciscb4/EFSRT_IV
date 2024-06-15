using System;
using System.Collections.Generic;

namespace DB.Models;

public partial class DetalleVentum
{
    public int IdDetalleVenta { get; set; }

    public int IdProducto { get; set; }

    public int Cantidad { get; set; }

    public decimal Subtotal { get; set; }

    public int IdVenta { get; set; }

    public virtual Producto IdProductoNavigation { get; set; } = null!;

    public virtual Ventum IdVentaNavigation { get; set; } = null!;
}
