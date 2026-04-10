using GestionesCIST.Domain.Entities;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace GestionesCIST.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Tablas Principales
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Tecnico> Tecnicos { get; set; }
        public DbSet<Especialidad> Especialidades { get; set; }
        public DbSet<TecnicoEspecialidad> TecnicoEspecialidades { get; set; }

        // Equipos
        public DbSet<Marca> Marcas { get; set; }
        public DbSet<ModeloEquipo> ModelosEquipo { get; set; }
        public DbSet<EquipoCliente> EquiposCliente { get; set; }

        // Tickets
        public DbSet<CategoriaTicket> CategoriasTicket { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketAsignacion> TicketAsignaciones { get; set; }
        public DbSet<ComentarioTicket> ComentariosTicket { get; set; }
        public DbSet<NotaTicket> NotasTicket { get; set; }

        // Órdenes de Servicio
        public DbSet<OrdenServicio> OrdenesServicio { get; set; }
        public DbSet<InformeTecnico> InformesTecnicos { get; set; }

        // Repuestos e Inventario
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<CategoriaRepuesto> CategoriasRepuesto { get; set; }
        public DbSet<Repuesto> Repuestos { get; set; }
        public DbSet<SolicitudRepuesto> SolicitudesRepuesto { get; set; }
        public DbSet<SolicitudRepuestoDetalle> SolicitudRepuestoDetalles { get; set; }
        public DbSet<MovimientoInventario> MovimientosInventario { get; set; }

        // Garantías
        public DbSet<Garantia> Garantias { get; set; }
        public DbSet<RMADevolucion> RMADevoluciones { get; set; }

        // Notificaciones y Auditoría
        public DbSet<Notificacion> Notificaciones { get; set; }
        public DbSet<EncuestaSatisfaccion> EncuestasSatisfaccion { get; set; }
        public DbSet<AuditoriaLog> AuditoriaLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplicar todas las configuraciones desde el ensamblado
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // Configuraciones adicionales que no requieren clase separada
            ConfigureRelationships(modelBuilder);
            ConfigureIndexes(modelBuilder);
            ConfigureDefaultValues(modelBuilder);
        }

        private void ConfigureRelationships(ModelBuilder modelBuilder)
        {
            // Ticket -> Cliente (Restrict Delete)
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Cliente)
                .WithMany(c => c.Tickets)
                .HasForeignKey(t => t.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrdenServicio -> Cliente (Restrict Delete)
            modelBuilder.Entity<OrdenServicio>()
                .HasOne(o => o.Cliente)
                .WithMany(c => c.OrdenesServicio)
                .HasForeignKey(o => o.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrdenServicio -> Ticket (Uno a Uno opcional)
            modelBuilder.Entity<OrdenServicio>()
                .HasOne(o => o.Ticket)
                .WithOne(t => t.OrdenServicio)
                .HasForeignKey<OrdenServicio>(o => o.TicketId)
                .OnDelete(DeleteBehavior.SetNull);

            // Ticket -> OrdenServicio (Inverso)
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.OrdenServicio)
                .WithOne(o => o.Ticket)
                .HasForeignKey<Ticket>(t => t.OrdenServicioId)
                .OnDelete(DeleteBehavior.SetNull);

            // Cliente -> Usuario (Uno a Uno)
            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.User)
                .WithOne(u => u.Cliente)
                .HasForeignKey<Cliente>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Tecnico -> Usuario (Uno a Uno)
            modelBuilder.Entity<Tecnico>()
                .HasOne(t => t.User)
                .WithOne(u => u.Tecnico)
                .HasForeignKey<Tecnico>(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private void ConfigureIndexes(ModelBuilder modelBuilder)
        {
            // Índices para Tickets
            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.Estado)
                .HasDatabaseName("IX_Tickets_Estado");

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.CodigoTicket)
                .IsUnique()
                .HasDatabaseName("IX_Tickets_CodigoTicket");

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.FechaCreacion)
                .HasDatabaseName("IX_Tickets_FechaCreacion");

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.Prioridad)
                .HasDatabaseName("IX_Tickets_Prioridad");

            // Índices para Órdenes de Servicio
            modelBuilder.Entity<OrdenServicio>()
                .HasIndex(o => o.Estado)
                .HasDatabaseName("IX_OrdenesServicio_Estado");

            modelBuilder.Entity<OrdenServicio>()
                .HasIndex(o => o.NumeroOrden)
                .IsUnique()
                .HasDatabaseName("IX_OrdenesServicio_NumeroOrden");

            modelBuilder.Entity<OrdenServicio>()
                .HasIndex(o => o.FechaRecepcion)
                .HasDatabaseName("IX_OrdenesServicio_FechaRecepcion");

            modelBuilder.Entity<OrdenServicio>()
                .HasIndex(o => o.TecnicoDiagnosticoId)
                .HasDatabaseName("IX_OrdenesServicio_TecnicoDiagnostico");

            modelBuilder.Entity<OrdenServicio>()
                .HasIndex(o => o.TecnicoReparacionId)
                .HasDatabaseName("IX_OrdenesServicio_TecnicoReparacion");

            // Índices para Clientes
            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.RUC)
                .HasDatabaseName("IX_Clientes_RUC")
                .HasFilter("[RUC] IS NOT NULL");

            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Categoria)
                .HasDatabaseName("IX_Clientes_Categoria");

            // Índices para Técnicos
            modelBuilder.Entity<Tecnico>()
                .HasIndex(t => t.CodigoTecnico)
                .IsUnique()
                .HasDatabaseName("IX_Tecnicos_CodigoTecnico");

            modelBuilder.Entity<Tecnico>()
                .HasIndex(t => t.Estado)
                .HasDatabaseName("IX_Tecnicos_Estado");

            modelBuilder.Entity<Tecnico>()
                .HasIndex(t => t.Sede)
                .HasDatabaseName("IX_Tecnicos_Sede");

            // Índices para Equipos
            modelBuilder.Entity<EquipoCliente>()
                .HasIndex(e => e.NumeroSerie)
                .HasDatabaseName("IX_EquiposCliente_NumeroSerie");

            // Índices para Repuestos
            modelBuilder.Entity<Repuesto>()
                .HasIndex(r => r.Codigo)
                .IsUnique()
                .HasDatabaseName("IX_Repuestos_Codigo");

            modelBuilder.Entity<Repuesto>()
                .HasIndex(r => r.StockActual)
                .HasDatabaseName("IX_Repuestos_StockActual");

            // Índices para Notificaciones
            modelBuilder.Entity<Notificacion>()
                .HasIndex(n => n.UsuarioId)
                .HasDatabaseName("IX_Notificaciones_UsuarioId");

            modelBuilder.Entity<Notificacion>()
                .HasIndex(n => n.Leida)
                .HasDatabaseName("IX_Notificaciones_Leida");

            // Índices para Auditoría
            modelBuilder.Entity<AuditoriaLog>()
                .HasIndex(a => a.FechaEvento)
                .HasDatabaseName("IX_AuditoriaLogs_FechaEvento");

            modelBuilder.Entity<AuditoriaLog>()
                .HasIndex(a => a.UsuarioId)
                .HasDatabaseName("IX_AuditoriaLogs_UsuarioId");
        }

        private void ConfigureDefaultValues(ModelBuilder modelBuilder)
        {
            // Configurar precisión decimal por defecto
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,4)");
            }

            // Fechas por defecto
            modelBuilder.Entity<Ticket>()
                .Property(t => t.FechaCreacion)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<OrdenServicio>()
                .Property(o => o.FechaRecepcion)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<Notificacion>()
                .Property(n => n.FechaCreacion)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}