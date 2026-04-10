using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GestionesCIST.Domain.Entities;

namespace GestionesCIST.Infrastructure.Configurations
{
    public class OrdenServicioConfiguration : IEntityTypeConfiguration<OrdenServicio>
    {
        public void Configure(EntityTypeBuilder<OrdenServicio> builder)
        {
            builder.ToTable("OrdenesServicio");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.NumeroOrden)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(o => o.TipoEquipo)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(o => o.Marca)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(o => o.Modelo)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(o => o.NumeroSerie)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(o => o.TipoGarantia)
                .HasMaxLength(30);

            builder.Property(o => o.ProblemaReportado)
                .IsRequired()
                .HasColumnType("NVARCHAR(MAX)");

            builder.Property(o => o.DiagnosticoInicial)
                .HasColumnType("NVARCHAR(MAX)");

            builder.Property(o => o.DiagnosticoFinal)
                .HasColumnType("NVARCHAR(MAX)");

            builder.Property(o => o.Estado)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("ASIGNACION");

            builder.Property(o => o.Prioridad)
                .HasDefaultValue(2);

            builder.Property(o => o.ColorEtiqueta)
                .HasMaxLength(20);

            builder.Property(o => o.ProbabilidadExitoQA)
                .HasColumnType("decimal(5,4)");

            builder.Property(o => o.TiempoEstimadoTotal_Horas)
                .HasColumnType("decimal(5,2)");

            builder.Property(o => o.FechaRecepcion)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(o => o.FechaCreacion)
                .HasDefaultValueSql("GETUTCDATE()");

            // Check constraints
            builder.ToTable(t => t.HasCheckConstraint("CK_OrdenesServicio_Estado",
                "Estado IN ('ASIGNACION', 'DIAGNOSTICO', 'REPARACION', 'PRUEBAS', 'ALMACEN')"));

            builder.ToTable(t => t.HasCheckConstraint("CK_OrdenesServicio_TipoGarantia",
                "TipoGarantia IS NULL OR TipoGarantia IN ('FABRICANTE', 'EXTENDIDA', 'PROVEEDOR')"));
        }
    }
}