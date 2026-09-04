using System;
using System.Collections.Generic;

namespace BrisasdelTenorio.Models;

public partial class VCobrosPendiente
{
    public int ClienteId { get; set; }

    public string Cliente { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public int PedidoId { get; set; }

    public DateTime FechaPedido { get; set; }

    public DateOnly FechaEntrega { get; set; }

    public decimal Total { get; set; }

    public string Estado { get; set; } = null!;
}
