using GestionesCIST.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GestionesCIST.Domain.Entities;

namespace GestionesCIST.Infrastructure.Configurations
{
    public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
    {
        public void Configure(EntityTypeBuilder<Ticket> builder)
        {
            builder.ToTable("Tickets");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.CodigoTicket)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(t => t.TipoEquipo)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.Marca)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.Modelo)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.NumeroSerie)
                .HasMaxLength(100);

            builder.Property(t => t.Titulo)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(t => t.DescripcionProblema)
                .IsRequired()
                .HasColumnType("NVARCHAR(MAX)");

            builder.Property(t => t.Prioridad)
                .HasDefaultValue(2);

            builder.Property(t => t.ConfianzaPrediccion)
                .HasColumnType("decimal(5,4)");

            builder.Property(t => t.TiempoEstimadoIA_Horas)
                .HasColumnType("decimal(5,2)");

            builder.Property(t => t.Estado)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("ABIERTO");

            builder.Property(t => t.Origen)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("PORTAL_WEB");

            builder.Property(t => t.FechaCreacion)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(t => t.FechaUltimaActualizacion)
                .HasDefaultValueSql("GETUTCDATE()");

            // Check constraint
            builder.ToTable(t => t.HasCheckConstraint("CK_Tickets_Estado",
                "Estado IN ('ABIERTO', 'ASIGNADO', 'EN_PROGRESO', 'PENDIENTE_CLIENTE', 'RESUELTO', 'CERRADO', 'CANCELADO')"));
        }
    }
}