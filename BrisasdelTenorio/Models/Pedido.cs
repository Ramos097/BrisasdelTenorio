using System;
using System.Collections.Generic;

namespace BrisasdelTenorio.Models;

public partial class Pedido
{
    public int PedidoId { get; set; }

    public int ClienteId { get; set; }

    public DateTime FechaPedido { get; set; }

    public DateOnly FechaEntrega { get; set; }

    public string Estado { get; set; } = null!;

    public string EstadoPago { get; set; } = null!;

    public DateTime? FechaPago { get; set; }

    public decimal Total { get; set; }

    public string? Observaciones { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual ICollection<DetallePedido> DetallePedidos { get; set; } = new List<DetallePedido>();
}
