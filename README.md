# Gestiones CIST - Control Inteligente de Soporte Técnico

## Descripción
Sistema integral de gestión de tickets y órdenes de servicio para reparación de equipos informáticos con inteligencia artificial.

## Tecnologías
- .NET 9.0
- ASP.NET Core Web API
- ASP.NET Core MVC
- Entity Framework Core 9
- SQL Server
- ML.NET
- SignalR
- JWT Authentication

## Estructura del Proyecto
- **01-Presentation**: WebAPI, WebMVC
- **02-Application**: DTOs, Servicios, Interfaces
- **03-Domain**: Entidades, ValueObjects, Enums
- **04-Infrastructure**: Repositorios, DbContext
- **05-Tests**: UnitTests, IntegrationTests

## Configuración Inicial
1. Clonar repositorio
2. Ejecutar `dotnet restore`
3. Configurar connection string en appsettings.json
4. Ejecutar migraciones: `dotnet ef database update`
5. Ejecutar aplicación: `dotnet run --project src/01-Presentation/GestionesCIST.WebAPI`

## Autor
Ingeniería de Sistemas - LUIS HIDER MANCO BERROCAL
