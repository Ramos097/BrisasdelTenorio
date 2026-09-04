using System;
using System.Collections.Generic;

namespace BrisasdelTenorio.Models;

public partial class VTopClientesMe
{
    public int ClienteId { get; set; }

    public string Nombre { get; set; } = null!;

    public int? NumPedidos { get; set; }

    public decimal? TotalComprado { get; set; }
}
