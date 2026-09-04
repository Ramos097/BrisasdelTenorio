using System;
using System.Collections.Generic;

namespace BrisasdelTenorio.Models;

public partial class Producto
{
    public int ProductoId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public int PesoGramos { get; set; }

    public decimal Precio { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<DetallePedido> DetallePedidos { get; set; } = new List<DetallePedido>();
}
