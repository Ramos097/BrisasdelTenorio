using System;
using System.Collections.Generic;

namespace BrisasdelTenorio.Models;

public partial class VResumenSemanal
{
    public int? Semana { get; set; }

    public int? Anio { get; set; }

    public int? TotalPedidos { get; set; }

    public decimal? VentasTotales { get; set; }

    public decimal? Cobrado { get; set; }

    public decimal? PorCobrar { get; set; }

    public int? ClientesActivos { get; set; }
}
