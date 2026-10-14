using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace voltajemoda_backend.Models;

public partial class VoltajeDbContext : DbContext
{
    public VoltajeDbContext()
    {
    }

    public VoltajeDbContext(DbContextOptions<VoltajeDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Carrito> Carritos { get; set; }

    public virtual DbSet<CarritoItem> CarritoItems { get; set; }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Cupone> Cupones { get; set; }

    public virtual DbSet<DetallesPedido> DetallesPedidos { get; set; }

    public virtual DbSet<DireccionesEntrega> DireccionesEntregas { get; set; }

    public virtual DbSet<Empleado> Empleados { get; set; }

    public virtual DbSet<HistorialInventario> HistorialInventarios { get; set; }

    public virtual DbSet<MediosDePago> MediosDePagos { get; set; }

    public virtual DbSet<Pedido> Pedidos { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<ProductoImagene> ProductoImagenes { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Variante> Variantes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Carrito>(entity =>
        {
            entity.HasKey(e => e.CarritoId).HasName("PK__Carritos__778D580B6590FCE0");

            entity.HasIndex(e => e.ClienteId, "UQ__Carritos__71ABD0A620C7C1B9").IsUnique();

            entity.Property(e => e.CarritoId).HasColumnName("CarritoID");
            entity.Property(e => e.ClienteId).HasColumnName("ClienteID");
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Cliente).WithOne(p => p.Carrito)
                .HasForeignKey<Carrito>(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Carritos__Client__72C60C4A");
        });

        modelBuilder.Entity<CarritoItem>(entity =>
        {
            entity.HasKey(e => e.CarritoItemId).HasName("PK__CarritoI__BFC8807A2482E1B5");

            entity.Property(e => e.CarritoItemId).HasColumnName("CarritoItemID");
            entity.Property(e => e.Cantidad).HasDefaultValue(1);
            entity.Property(e => e.CarritoId).HasColumnName("CarritoID");
            entity.Property(e => e.VarianteId).HasColumnName("VarianteID");

            entity.HasOne(d => d.Carrito).WithMany(p => p.CarritoItems)
                .HasForeignKey(d => d.CarritoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CarritoIt__Carri__76969D2E");

            entity.HasOne(d => d.Variante).WithMany(p => p.CarritoItems)
                .HasForeignKey(d => d.VarianteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CarritoIt__Varia__778AC167");
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.CategoriaId).HasName("PK__Categori__F353C1C5067DEBC9");

            entity.HasIndex(e => e.Nombre, "UQ__Categori__75E3EFCF33097ACA").IsUnique();

            entity.Property(e => e.CategoriaId).HasColumnName("CategoriaID");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.ClienteId).HasName("PK__Clientes__71ABD0A72B308698");

            entity.HasIndex(e => e.UsuarioId, "UQ__Clientes__2B3DE79919434641").IsUnique();

            entity.Property(e => e.ClienteId).HasColumnName("ClienteID");
            entity.Property(e => e.Apellido)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");

            entity.HasOne(d => d.Usuario).WithOne(p => p.Cliente)
                .HasForeignKey<Cliente>(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Clientes__Usuari__5441852A");
        });

        modelBuilder.Entity<Cupone>(entity =>
        {
            entity.HasKey(e => e.CuponId).HasName("PK__Cupones__C43568B783EA92BF");

            entity.HasIndex(e => e.Codigo, "UQ__Cupones__06370DAC03D1671A").IsUnique();

            entity.Property(e => e.CuponId).HasColumnName("CuponID");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Codigo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Descuento).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.ValidoDesde).HasColumnType("datetime");
            entity.Property(e => e.ValidoHasta).HasColumnType("datetime");
        });

        modelBuilder.Entity<DetallesPedido>(entity =>
        {
            entity.HasKey(e => e.DetalleId).HasName("PK__Detalles__6E19D6FA599798C5");

            entity.ToTable("DetallesPedido");

            entity.Property(e => e.DetalleId).HasColumnName("DetalleID");
            entity.Property(e => e.PedidoId).HasColumnName("PedidoID");
            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.VarianteId).HasColumnName("VarianteID");

            entity.HasOne(d => d.Pedido).WithMany(p => p.DetallesPedidos)
                .HasForeignKey(d => d.PedidoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DetallesP__Pedid__07C12930");

            entity.HasOne(d => d.Variante).WithMany(p => p.DetallesPedidos)
                .HasForeignKey(d => d.VarianteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DetallesP__Varia__08B54D69");
        });

        modelBuilder.Entity<DireccionesEntrega>(entity =>
        {
            entity.HasKey(e => e.DireccionId).HasName("PK__Direccio__68906D445A10DA95");

            entity.ToTable("DireccionesEntrega");

            entity.Property(e => e.DireccionId).HasColumnName("DireccionID");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Ciudad)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ClienteId).HasColumnName("ClienteID");
            entity.Property(e => e.DireccionFisica)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Etiqueta)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Cliente).WithMany(p => p.DireccionesEntregas)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Direccion__Clien__5BE2A6F2");
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.HasKey(e => e.EmpleadoId).HasName("PK__Empleado__958BE6F03EDCF0C6");

            entity.HasIndex(e => e.UsuarioId, "UQ__Empleado__2B3DE79954479D07").IsUnique();

            entity.Property(e => e.EmpleadoId).HasColumnName("EmpleadoID");
            entity.Property(e => e.Apellido)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Cargo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");

            entity.HasOne(d => d.Usuario).WithOne(p => p.Empleado)
                .HasForeignKey<Empleado>(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Empleados__Usuar__5812160E");
        });

        modelBuilder.Entity<HistorialInventario>(entity =>
        {
            entity.HasKey(e => e.HistorialId).HasName("PK__Historia__975206EF6337CED0");

            entity.ToTable("HistorialInventario");

            entity.Property(e => e.HistorialId).HasColumnName("HistorialID");
            entity.Property(e => e.EmpleadoId).HasColumnName("EmpleadoID");
            entity.Property(e => e.FechaAjuste)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Motivo)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.VarianteId).HasColumnName("VarianteID");

            entity.HasOne(d => d.Empleado).WithMany(p => p.HistorialInventarios)
                .HasForeignKey(d => d.EmpleadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Historial__Emple__0D7A0286");

            entity.HasOne(d => d.Variante).WithMany(p => p.HistorialInventarios)
                .HasForeignKey(d => d.VarianteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Historial__Varia__0C85DE4D");
        });

        modelBuilder.Entity<MediosDePago>(entity =>
        {
            entity.HasKey(e => e.MetodoPagoId).HasName("PK__MediosDe__A8FEAF740AA70546");

            entity.ToTable("MediosDePago");

            entity.Property(e => e.MetodoPagoId).HasColumnName("MetodoPagoID");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.NombreMetodo)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.HasKey(e => e.PedidoId).HasName("PK__Pedidos__09BA141089CE2F6A");

            entity.Property(e => e.PedidoId).HasColumnName("PedidoID");
            entity.Property(e => e.ClienteId).HasColumnName("ClienteID");
            entity.Property(e => e.CuponId).HasColumnName("CuponID");

            // --- AQUÍ MAPEAMOS LAS NUEVAS COLUMNAS DE TEXTO ---
            entity.Property(e => e.DireccionEnvio)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.Property(e => e.CiudadEnvio)
                .HasMaxLength(100)
                .IsUnicode(false);
            // --------------------------------------------------

            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FechaPedido)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MetodoPagoId).HasColumnName("MetodoPagoID");
            entity.Property(e => e.Subtotal).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Total).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Cliente).WithMany(p => p.Pedidos)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Pedidos__Cliente__02084FDA");

            entity.HasOne(d => d.Cupon).WithMany(p => p.Pedidos)
                .HasForeignKey(d => d.CuponId)
                .HasConstraintName("FK__Pedidos__CuponID__04E4BC85");

            // (Se eliminó el bloque de entity.HasOne(d => d.Direccion)... porque ya no existe la llave foránea)

            entity.HasOne(d => d.MetodoPago).WithMany(p => p.Pedidos)
                .HasForeignKey(d => d.MetodoPagoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Pedidos__MetodoP__03F0984C");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.ProductoId).HasName("PK__Producto__A430AE8367F7EB26");

            entity.HasIndex(e => e.SkuBase, "UQ__Producto__3FF13B9BB98004A6").IsUnique();

            entity.Property(e => e.ProductoId).HasColumnName("ProductoID");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.CategoriaId).HasColumnName("CategoriaID");
            entity.Property(e => e.Descripcion).HasColumnType("text");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.PrecioBase).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.SkuBase)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("SKU_Base");

            entity.HasOne(d => d.Categoria).WithMany(p => p.Productos)
                .HasForeignKey(d => d.CategoriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Productos__Categ__6477ECF3");
        });

        modelBuilder.Entity<ProductoImagene>(entity =>
        {
            entity.HasKey(e => e.ImagenId).HasName("PK__Producto__0C7D20D7A4CF22EA");

            entity.Property(e => e.ImagenId).HasColumnName("ImagenID");
            entity.Property(e => e.ProductoId).HasColumnName("ProductoID");
            entity.Property(e => e.UrlImagen)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.Producto).WithMany(p => p.ProductoImagenes)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProductoI__Produ__68487DD7");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RolId).HasName("PK__Roles__F92302D1684AD258");

            entity.HasIndex(e => e.NombreRol, "UQ__Roles__4F0B537F5B2B8019").IsUnique();

            entity.Property(e => e.RolId).HasColumnName("RolID");
            entity.Property(e => e.NombreRol)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.UsuarioId).HasName("PK__Usuarios__2B3DE7980316D760");

            entity.HasIndex(e => e.CorreoElectronico, "UQ__Usuarios__531402F33ED1778A").IsUnique();

            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.CorreoElectronico)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.RolId).HasColumnName("RolID");

            entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuarios__RolID__5070F446");
        });

        modelBuilder.Entity<Variante>(entity =>
        {
            entity.HasKey(e => e.VarianteId).HasName("PK__Variante__FE18F96BD9CEADCE");

            entity.HasIndex(e => e.SkuVariante, "UQ__Variante__5437592FFA3E164C").IsUnique();

            entity.Property(e => e.VarianteId).HasColumnName("VarianteID");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Color)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ProductoId).HasColumnName("ProductoID");
            entity.Property(e => e.SkuVariante)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("SKU_Variante");
            entity.Property(e => e.Talla)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Producto).WithMany(p => p.Variantes)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Variantes__Produ__6E01572D");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
