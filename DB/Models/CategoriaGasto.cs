using System;
using System.Collections.Generic;

namespace DB.Models;

public partial class CategoriaGasto
{
    public int IdCategoriaGasto { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Gasto> Gastos { get; set; } = new List<Gasto>();
}
