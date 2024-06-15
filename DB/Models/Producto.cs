using System;
using System.Collections.Generic;

namespace DB.Models;

public partial class Producto
{
    public int IdProducto { get; set; }

    public string Nombre { get; set; } = null!;

    public decimal Precio { get; set; }

    public int Stock { get; set; }

    public bool Estado { get; set; }

    public int IdCategoria { get; set; }

    public int IdTienda { get; set; }

    public virtual ICollection<DetalleVentum> DetalleVenta { get; set; } = new List<DetalleVentum>();

    public virtual Categorium IdCategoriaNavigation { get; set; } = null!;

    public virtual Tiendum IdTiendaNavigation { get; set; } = null!;
}
