using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GestionesCIST.Domain.Entities;

namespace GestionesCIST.Infrastructure.Configurations
{
    public class TecnicoConfiguration : IEntityTypeConfiguration<Tecnico>
    {
        public void Configure(EntityTypeBuilder<Tecnico> builder)
        {
            builder.ToTable("Tecnicos");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.CodigoTecnico)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(t => t.FechaIngreso)
                .IsRequired();

            builder.Property(t => t.Nivel)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("JUNIOR");

            builder.Property(t => t.Certificaciones)
                .HasColumnType("NVARCHAR(MAX)");

            builder.Property(t => t.Sede)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Lima");

            builder.Property(t => t.RadioCoberturaKM)
                .HasDefaultValue(15);

            builder.Property(t => t.Estado)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("DISPONIBLE");

            builder.Property(t => t.CargaMaximaDiaria)
                .HasDefaultValue(8);

            builder.Property(t => t.CargaActual)
                .HasDefaultValue(0);

            builder.Property(t => t.PromedioCalificacion)
                .HasColumnType("decimal(3,2)")
                .HasDefaultValue(0);

            builder.Property(t => t.Activo)
                .HasDefaultValue(true);

            // Check constraints
            builder.ToTable(t => t.HasCheckConstraint("CK_Tecnicos_Nivel",
                "Nivel IN ('JUNIOR', 'SEMI_SENIOR', 'SENIOR', 'EXPERTO')"));

            builder.ToTable(t => t.HasCheckConstraint("CK_Tecnicos_Estado",
                "Estado IN ('DISPONIBLE', 'OCUPADO', 'EN_RUTA', 'AUSENTE', 'VACACIONES')"));
        }
    }
}