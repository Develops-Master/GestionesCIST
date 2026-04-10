using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GestionesCIST.Domain.Entities;

namespace GestionesCIST.Infrastructure.Configurations
{
    public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
            builder.ToTable("Clientes");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.TipoCliente)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("NATURAL");

            builder.Property(c => c.RazonSocial)
                .HasMaxLength(200);

            builder.Property(c => c.RUC)
                .HasMaxLength(20);

            builder.Property(c => c.ContactoPrincipal)
                .HasMaxLength(150);

            builder.Property(c => c.TelefonoFijo)
                .HasMaxLength(20);

            builder.Property(c => c.TelefonoMovil)
                .HasMaxLength(20);

            builder.Property(c => c.CorreoFacturacion)
                .HasMaxLength(100);

            builder.Property(c => c.Departamento)
                .HasMaxLength(50);

            builder.Property(c => c.Provincia)
                .HasMaxLength(50);

            builder.Property(c => c.Distrito)
                .HasMaxLength(50);

            builder.Property(c => c.DireccionFiscal)
                .HasMaxLength(200);

            builder.Property(c => c.Categoria)
                .HasMaxLength(20)
                .HasDefaultValue("NUEVO");

            builder.Property(c => c.SLA_TiempoRespuestaHoras)
                .HasDefaultValue(4);

            builder.Property(c => c.SLA_TiempoSolucionDias)
                .HasDefaultValue(3);

            builder.Property(c => c.Activo)
                .HasDefaultValue(true);

            builder.Property(c => c.FechaRegistro)
                .HasDefaultValueSql("GETUTCDATE()");

            // Check constraints
            builder.ToTable(t => t.HasCheckConstraint("CK_Clientes_TipoCliente",
                "TipoCliente IN ('NATURAL', 'JURIDICO')"));

            builder.ToTable(t => t.HasCheckConstraint("CK_Clientes_Categoria",
                "Categoria IN ('VIP', 'CORPORATIVO', 'FRECUENTE', 'NUEVO')"));
        }
    }
}