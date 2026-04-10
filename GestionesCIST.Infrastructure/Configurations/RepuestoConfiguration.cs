using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GestionesCIST.Domain.Entities;

namespace GestionesCIST.Infrastructure.Configurations
{
    public class RepuestoConfiguration : IEntityTypeConfiguration<Repuesto>
    {
        public void Configure(EntityTypeBuilder<Repuesto> builder)
        {
            builder.ToTable("Repuestos");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Codigo)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(r => r.CodigoBarras)
                .HasMaxLength(100);

            builder.Property(r => r.Nombre)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(r => r.Descripcion)
                .HasMaxLength(500);

            builder.Property(r => r.Marca)
                .HasMaxLength(50);

            builder.Property(r => r.ModeloCompatible)
                .HasColumnType("NVARCHAR(MAX)");

            builder.Property(r => r.StockActual)
                .HasDefaultValue(0);

            builder.Property(r => r.StockMinimo)
                .HasDefaultValue(5);

            builder.Property(r => r.StockMaximo)
                .HasDefaultValue(50);

            builder.Property(r => r.UbicacionAlmacen)
                .HasMaxLength(50);

            builder.Property(r => r.PrecioCosto)
                .HasColumnType("decimal(10,2)");

            builder.Property(r => r.PrecioVenta)
                .HasColumnType("decimal(10,2)");

            builder.Property(r => r.Moneda)
                .HasMaxLength(3)
                .HasDefaultValue("PEN");

            builder.Property(r => r.IGV)
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(18.00m);

            builder.Property(r => r.Activo)
                .HasDefaultValue(true);
        }
    }
}