using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BrisasdelTenorio.Models;

public partial class BrisasDelTenorioContext : DbContext
{
    public BrisasDelTenorioContext()
    {
    }

    public BrisasDelTenorioContext(DbContextOptions<BrisasDelTenorioContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<DetallePedido> DetallePedidos { get; set; }

    public virtual DbSet<Pedido> Pedidos { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<UsuariosAdmin> UsuariosAdmins { get; set; }

    public virtual DbSet<VCobrosPendiente> VCobrosPendientes { get; set; }

    public virtual DbSet<VResumenSemanal> VResumenSemanals { get; set; }

    public virtual DbSet<VTopClientesMe> VTopClientesMes { get; set; }
    public virtual DbSet<PrecioEspecial> PreciosEspeciales { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=BrisasdelTenorio;User Id=sa;Password=sistemas;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.ClienteId).HasName("PK__CLIENTES__71ABD0878E5C1C93");

            entity.ToTable("CLIENTES");

            entity.HasIndex(e => e.TokenPedido, "IX_Clientes_Token");

            entity.HasIndex(e => e.TokenPedido, "UQ__CLIENTES__BA69926A2683FD54").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Direccion).HasMaxLength(300);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Nombre).HasMaxLength(150);
            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.Property(e => e.TokenPedido)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<DetallePedido>(entity =>
        {
            entity.HasKey(e => e.DetallePedidoId).HasName("PK__DETALLE___6ED21C21B756F644");

            entity.ToTable("DETALLE_PEDIDO");

            entity.HasIndex(e => e.PedidoId, "IX_Detalle_PedidoId");

            entity.HasIndex(e => e.ProductoId, "IX_Detalle_ProductoId");

            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Subtotal).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Pedido).WithMany(p => p.DetallePedidos)
                .HasForeignKey(d => d.PedidoId)
                .HasConstraintName("FK_Detalle_Pedidos");

            entity.HasOne(d => d.Producto).WithMany(p => p.DetallePedidos)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Detalle_Productos");
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.HasKey(e => e.PedidoId).HasName("PK__PEDIDOS__09BA143014107BE5");

            entity.ToTable("PEDIDOS");

            entity.HasIndex(e => e.ClienteId, "IX_Pedidos_ClienteId");

            entity.HasIndex(e => e.EstadoPago, "IX_Pedidos_EstadoPago");

            entity.HasIndex(e => e.FechaEntrega, "IX_Pedidos_FechaEntrega");

            entity.HasIndex(e => e.FechaPedido, "IX_Pedidos_FechaPedido");

            entity.Property(e => e.Estado)
                .HasMaxLength(30)
                .HasDefaultValue("Pendiente");
            entity.Property(e => e.EstadoPago)
                .HasMaxLength(30)
                .HasDefaultValue("Pendiente");
            entity.Property(e => e.FechaPedido).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Observaciones).HasMaxLength(500);
            entity.Property(e => e.Total).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Cliente).WithMany(p => p.Pedidos)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pedidos_Clientes");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.ProductoId).HasName("PK__PRODUCTO__A430AEA3F531AEE8");

            entity.ToTable("PRODUCTOS");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Descripcion).HasMaxLength(300);
            entity.Property(e => e.Nombre).HasMaxLength(150);
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<UsuariosAdmin>(entity =>
        {
            entity.HasKey(e => e.UsuarioId).HasName("PK__USUARIOS__2B3DE7B8A377256F");

            entity.ToTable("USUARIOS_ADMIN");

            entity.HasIndex(e => e.NombreUsuario, "UQ__USUARIOS__6B0F5AE0856325B9").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Nombre).HasMaxLength(150);
            entity.Property(e => e.NombreUsuario).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.Rol)
                .HasMaxLength(30)
                .HasDefaultValue("Admin");
        });

        modelBuilder.Entity<VCobrosPendiente>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_CobrosPendientes");

            entity.Property(e => e.Cliente).HasMaxLength(150);
            entity.Property(e => e.Estado).HasMaxLength(30);
            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.Property(e => e.Total).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<VResumenSemanal>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_ResumenSemanal");

            entity.Property(e => e.Cobrado).HasColumnType("decimal(38, 2)");
            entity.Property(e => e.PorCobrar).HasColumnType("decimal(38, 2)");
            entity.Property(e => e.VentasTotales).HasColumnType("decimal(38, 2)");
        });

        modelBuilder.Entity<VTopClientesMe>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_TopClientesMes");

            entity.Property(e => e.Nombre).HasMaxLength(150);
            entity.Property(e => e.TotalComprado).HasColumnType("decimal(38, 2)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
