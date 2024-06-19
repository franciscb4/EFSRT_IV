using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DB.Models;

public partial class EfsrtIvContext : DbContext
{
    public EfsrtIvContext()
    {
    }

    public EfsrtIvContext(DbContextOptions<EfsrtIvContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CategoriaGasto> CategoriaGastos { get; set; }

    public virtual DbSet<CategoriaProducto> CategoriaProductos { get; set; }

    public virtual DbSet<DetalleGasto> DetalleGastos { get; set; }

    public virtual DbSet<DetalleVentum> DetalleVenta { get; set; }

    public virtual DbSet<Gasto> Gastos { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Proveedor> Proveedors { get; set; }

    public virtual DbSet<Tiendum> Tienda { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Ventum> Venta { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("SERVER=LEON;DataBase=EFSRT_IV;Integrated Security=true;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CategoriaGasto>(entity =>
        {
            entity.HasKey(e => e.IdCategoriaGasto).HasName("PK__Categori__5962748166C13E7E");

            entity.ToTable("CategoriaGasto");

            entity.Property(e => e.Nombre)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<CategoriaProducto>(entity =>
        {
            entity.HasKey(e => e.IdCategoriaProducto).HasName("PK__Categori__8A4F21C3C54EEC6C");

            entity.ToTable("CategoriaProducto");

            entity.Property(e => e.Nombre)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<DetalleGasto>(entity =>
        {
            entity.HasKey(e => e.IdDetalleGasto).HasName("PK__DetalleG__2DC0250E39409CB0");

            entity.ToTable("DetalleGasto");

            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Monto).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdGastoNavigation).WithMany(p => p.DetalleGastos)
                .HasForeignKey(d => d.IdGasto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleGastoGasto_IdGasto");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.DetalleGastos)
                .HasForeignKey(d => d.IdProducto)
                .HasConstraintName("FK_DetalleGastoProducto_IdProducto");
        });

        modelBuilder.Entity<DetalleVentum>(entity =>
        {
            entity.HasKey(e => e.IdDetalleVenta).HasName("PK__DetalleV__AAA5CEC27D97947D");

            entity.Property(e => e.Subtotal).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.DetalleVenta)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleVentaProducto_IdProducto");

            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.DetalleVenta)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleVentaVenta_IdVenta");
        });

        modelBuilder.Entity<Gasto>(entity =>
        {
            entity.HasKey(e => e.IdGasto).HasName("PK__Gasto__C630244DCC2FCD74");

            entity.ToTable("Gasto");

            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Monto).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdCategoriaGastoNavigation).WithMany(p => p.Gastos)
                .HasForeignKey(d => d.IdCategoriaGasto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GastoCategoriaGasto_IdCategoriaGasto");

            entity.HasOne(d => d.IdProveedorNavigation).WithMany(p => p.Gastos)
                .HasForeignKey(d => d.IdProveedor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GastoProveedor_IdProveedor");

            entity.HasOne(d => d.IdTiendaNavigation).WithMany(p => p.Gastos)
                .HasForeignKey(d => d.IdTienda)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GastoTienda_IdTienda");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.IdProducto).HasName("PK__Producto__09889210A46C4278");

            entity.ToTable("Producto");

            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.Nombre)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdCategoriaProductoNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdCategoriaProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductoCategoriaProducto_IdCategoriaProducto");

            entity.HasOne(d => d.IdTiendaNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdTienda)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductoTienda_IdTienda");
        });

        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.HasKey(e => e.IdProveedor).HasName("PK__Proveedo__E8B631AFB4D3E1E9");

            entity.ToTable("Proveedor");

            entity.Property(e => e.Contacto)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Direccion)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Tiendum>(entity =>
        {
            entity.HasKey(e => e.IdTienda).HasName("PK__Tienda__5A1EB96B551C681A");

            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.RazonSocial)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Ruc)
                .HasMaxLength(11)
                .IsUnicode(false);

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Tienda)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TiendaUsuario_IdUsuario");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuario__5B65BF97182B039E");

            entity.ToTable("Usuario");

            entity.HasIndex(e => e.Correo, "UQ__Usuario__60695A196CDA9CFE").IsUnique();

            entity.HasIndex(e => e.Dni, "UQ__Usuario__C035B8DD38001E02").IsUnique();

            entity.Property(e => e.Clave)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Correo)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Dni)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("DNI");
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.Nombre)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Telefono)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Ventum>(entity =>
        {
            entity.HasKey(e => e.IdVenta).HasName("PK__Venta__BC1240BD19CB82BB");

            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Monto).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdTiendaNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdTienda)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VentaTienda_IdTienda");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
