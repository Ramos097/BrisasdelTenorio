using System;
using System.Collections.Generic;

namespace BrisasdelTenorio.Models;

public partial class UsuariosAdmin
{
    public int UsuarioId { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Rol { get; set; } = null!;

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }
}
