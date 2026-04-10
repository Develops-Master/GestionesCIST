using GestionesCIST.Domain.Entities;
using GestionesCIST.Domain.Enums;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GestionesCIST.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger logger)
        {
            logger.LogInformation("Iniciando seeding de base de datos...");

            // Asegurar que la BD está creada
            await context.Database.EnsureCreatedAsync();

            // 1. Crear Roles
            await SeedRolesAsync(roleManager, logger);

            // 2. Crear Usuario Administrador
            await SeedAdminUserAsync(userManager, logger);

            // 3. Crear Datos Maestros
            await SeedMarcasAsync(context, logger);
            await SeedCategoriasTicketAsync(context, logger);
            await SeedEspecialidadesAsync(context, logger);
            await SeedCategoriasRepuestoAsync(context, logger);

            // 4. Crear Usuarios de Prueba (Solo en Development)
            if (context.Database.GetConnectionString()?.Contains("Dev") == true)
            {
                await SeedTestDataAsync(context, userManager, logger);
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Seeding de base de datos completado.");
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            string[] roles = {
                "ADMIN",
                "GERENTE_GENERAL",
                "JEFE_SOPORTE",
                "COORDINADOR_GARANTIAS",
                "TECNICO",
                "QA",
                "ALMACEN",
                "VENTAS",
                "CLIENTE"
            };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var role = new IdentityRole
                    {
                        Name = roleName,
                        NormalizedName = roleName.ToUpper()
                    };
                    await roleManager.CreateAsync(role);
                    logger.LogInformation("Rol creado: {RoleName}", roleName);
                }
            }
        }

        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, ILogger logger)
        {
            var adminEmail = "admin@gestionescist.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Nombres = "Administrador",
                    Apellidos = "Sistema",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    EstadoAprobacion = EstadoAprobacion.Aprobado,
                    FechaRegistro = DateTime.UtcNow,
                    FechaAprobacion = DateTime.UtcNow,
                    TipoDocumento = "DNI",
                    NumeroDocumento = "00000000"
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@2026!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "ADMIN");
                    await userManager.AddToRoleAsync(adminUser, "GERENTE_GENERAL");
                    logger.LogInformation("Usuario Administrador creado: {Email}", adminEmail);
                }
                else
                {
                    logger.LogError("Error al crear usuario admin: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        private static async Task SeedMarcasAsync(AppDbContext context, ILogger logger)
        {
            if (!await context.Marcas.AnyAsync())
            {
                var marcas = new List<Marca>
                {
                    new() { Nombre = "HP" },
                    new() { Nombre = "Dell" },
                    new() { Nombre = "Lenovo" },
                    new() { Nombre = "Apple" },
                    new() { Nombre = "Asus" },
                    new() { Nombre = "Acer" },
                    new() { Nombre = "Toshiba" },
                    new() { Nombre = "Samsung" },
                    new() { Nombre = "Epson" },
                    new() { Nombre = "Canon" },
                    new() { Nombre = "Brother" }
                };

                await context.Marcas.AddRangeAsync(marcas);
                logger.LogInformation("{Count} marcas creadas", marcas.Count);
            }
        }

        private static async Task SeedCategoriasTicketAsync(AppDbContext context, ILogger logger)
        {
            if (!await context.CategoriasTicket.AnyAsync())
            {
                var categorias = new List<CategoriaTicket>
                {
                    new() { Nombre = "Hardware - No Enciende", Descripcion = "El equipo no enciende o no da señal de video", TiempoEstimadoHoras = 6 },
                    new() { Nombre = "Hardware - Pantalla", Descripcion = "Problemas con la pantalla o display", TiempoEstimadoHoras = 4 },
                    new() { Nombre = "Hardware - Teclado/Touchpad", Descripcion = "Fallas en teclado o touchpad", TiempoEstimadoHoras = 3 },
                    new() { Nombre = "Hardware - Batería/Cargador", Descripcion = "Problemas de carga o batería", TiempoEstimadoHoras = 2 },
                    new() { Nombre = "Software - Inicio Lento", Descripcion = "El sistema operativo inicia muy lento", TiempoEstimadoHoras = 4 },
                    new() { Nombre = "Software - Virus/Malware", Descripcion = "Infección por virus o malware", TiempoEstimadoHoras = 5 },
                    new() { Nombre = "Software - Actualización", Descripcion = "Actualización de sistema o drivers", TiempoEstimadoHoras = 2 },
                    new() { Nombre = "Red - Sin Conexión", Descripcion = "No hay conexión a internet", TiempoEstimadoHoras = 3 },
                    new() { Nombre = "Red - WiFi Intermitente", Descripcion = "Conexión WiFi inestable", TiempoEstimadoHoras = 4 },
                    new() { Nombre = "Impresora - Atasco", Descripcion = "Atasco de papel", TiempoEstimadoHoras = 2 },
                    new() { Nombre = "Impresora - Calidad", Descripcion = "Mala calidad de impresión", TiempoEstimadoHoras = 3 },
                    new() { Nombre = "Servidor - Caída", Descripcion = "Servidor no responde", TiempoEstimadoHoras = 8 }
                };

                await context.CategoriasTicket.AddRangeAsync(categorias);
                logger.LogInformation("{Count} categorías de ticket creadas", categorias.Count);
            }
        }

        private static async Task SeedEspecialidadesAsync(AppDbContext context, ILogger logger)
        {
            if (!await context.Especialidades.AnyAsync())
            {
                var especialidades = new List<Especialidad>
                {
                    new() { Nombre = "Hardware - Laptops", Descripcion = "Reparación de laptops y notebooks" },
                    new() { Nombre = "Hardware - Desktops", Descripcion = "Reparación de computadoras de escritorio" },
                    new() { Nombre = "Hardware - Impresoras", Descripcion = "Reparación de impresoras y multifuncionales" },
                    new() { Nombre = "Software - Sistemas Operativos", Descripcion = "Instalación y configuración de SO" },
                    new() { Nombre = "Software - Aplicaciones", Descripcion = "Soporte de software de oficina y especializado" },
                    new() { Nombre = "Redes - Cableado", Descripcion = "Instalación y mantenimiento de redes cableadas" },
                    new() { Nombre = "Redes - WiFi", Descripcion = "Configuración de redes inalámbricas" },
                    new() { Nombre = "Servidores", Descripcion = "Administración y mantenimiento de servidores" },
                    new() { Nombre = "Mac", Descripcion = "Reparación de equipos Apple" },
                    new() { Nombre = "Dispositivos Móviles", Descripcion = "Reparación de tablets y smartphones" }
                };

                await context.Especialidades.AddRangeAsync(especialidades);
                logger.LogInformation("{Count} especialidades creadas", especialidades.Count);
            }
        }

        private static async Task SeedCategoriasRepuestoAsync(AppDbContext context, ILogger logger)
        {
            if (!await context.CategoriasRepuesto.AnyAsync())
            {
                var categorias = new List<CategoriaRepuesto>
                {
                    new() { Nombre = "Discos Duros/SSD" },
                    new() { Nombre = "Memorias RAM" },
                    new() { Nombre = "Baterías" },
                    new() { Nombre = "Cargadores" },
                    new() { Nombre = "Pantallas/LCD" },
                    new() { Nombre = "Teclados" },
                    new() { Nombre = "Placas Base" },
                    new() { Nombre = "Ventiladores" },
                    new() { Nombre = "Cartuchos/Toner" },
                    new() { Nombre = "Cables y Adaptadores" },
                    new() { Nombre = "Procesadores" }
                };

                await context.CategoriasRepuesto.AddRangeAsync(categorias);
                logger.LogInformation("{Count} categorías de repuesto creadas", categorias.Count);
            }
        }

        private static async Task SeedTestDataAsync(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger logger)
        {
            logger.LogInformation("Creando datos de prueba para entorno Development...");

            // Crear Cliente de Prueba
            var clienteEmail = "cliente@test.com";
            if (await userManager.FindByEmailAsync(clienteEmail) == null)
            {
                var clienteUser = new ApplicationUser
                {
                    UserName = clienteEmail,
                    Email = clienteEmail,
                    Nombres = "Juan",
                    Apellidos = "Pérez",
                    EmailConfirmed = true,
                    EstadoAprobacion = EstadoAprobacion.Aprobado,
                    FechaRegistro = DateTime.UtcNow,
                    FechaAprobacion = DateTime.UtcNow,
                    TipoDocumento = "DNI",
                    NumeroDocumento = "12345678"
                };

                await userManager.CreateAsync(clienteUser, "Cliente@2026!");
                await userManager.AddToRoleAsync(clienteUser, "CLIENTE");

                var cliente = new Cliente
                {
                    UserId = clienteUser.Id,
                    TipoCliente = "NATURAL",
                    RazonSocial = "Juan Pérez",
                    TelefonoMovil = "987654321",
                    Categoria = "NUEVO"
                };

                await context.Clientes.AddAsync(cliente);
            }

            // Crear Técnico de Prueba
            var tecnicoEmail = "tecnico@test.com";
            if (await userManager.FindByEmailAsync(tecnicoEmail) == null)
            {
                var tecnicoUser = new ApplicationUser
                {
                    UserName = tecnicoEmail,
                    Email = tecnicoEmail,
                    Nombres = "Carlos",
                    Apellidos = "Rodríguez",
                    EmailConfirmed = true,
                    EstadoAprobacion = EstadoAprobacion.Aprobado,
                    FechaRegistro = DateTime.UtcNow
                };

                await userManager.CreateAsync(tecnicoUser, "Tecnico@2026!");
                await userManager.AddToRoleAsync(tecnicoUser, "TECNICO");

                var tecnico = new Tecnico
                {
                    UserId = tecnicoUser.Id,
                    CodigoTecnico = "TEC-001",
                    FechaIngreso = DateTime.UtcNow.AddMonths(-6),
                    Nivel = "SEMI_SENIOR",
                    AniosExperiencia = 3,
                    Sede = "Lima",
                    Estado = "DISPONIBLE"
                };

                await context.Tecnicos.AddAsync(tecnico);
            }

            // Crear Jefe de Soporte de Prueba
            var jefeEmail = "jefe@test.com";
            if (await userManager.FindByEmailAsync(jefeEmail) == null)
            {
                var jefeUser = new ApplicationUser
                {
                    UserName = jefeEmail,
                    Email = jefeEmail,
                    Nombres = "María",
                    Apellidos = "González",
                    EmailConfirmed = true,
                    EstadoAprobacion = EstadoAprobacion.Aprobado,
                    FechaRegistro = DateTime.UtcNow
                };

                await userManager.CreateAsync(jefeUser, "Jefe@2026!");
                await userManager.AddToRoleAsync(jefeUser, "JEFE_SOPORTE");
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Datos de prueba creados exitosamente.");
        }
    }
}