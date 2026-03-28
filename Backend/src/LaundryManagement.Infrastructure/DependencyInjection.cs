using LaundryManagement.Application.Interfaces;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Infrastructure.Configuration;
using LaundryManagement.Infrastructure.Persistence;
using LaundryManagement.Infrastructure.Repositories;
using LaundryManagement.Infrastructure.Reports;
using LaundryManagement.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LaundryManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register DbContext for EF Core (Writes)
        services.AddDbContext<LaundryDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null
                    );
                    sqlOptions.CommandTimeout(30); // 30 segundos timeout para comandos SQL
                }
            )
            .EnableSensitiveDataLogging() // Solo para desarrollo - mostrar parámetros SQL
            .EnableDetailedErrors() // Mostrar errores detallados
        );

        // Register Dapper connection factory (Reads)
        services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register DDD Repositories
        services.AddScoped<IOrderRepository, OrderRepositoryPure>();
        services.AddScoped<IUserRepository, UserRepositoryPure>();
        services.AddScoped<IRoleRepository, RoleRepositoryPure>();
        services.AddScoped<IPermissionRepository, PermissionRepositoryPure>();
        services.AddScoped<IServiceRepository, ServiceRepositoryPure>();
        services.AddScoped<IServiceGarmentRepository, ServiceGarmentRepositoryPure>();
        services.AddScoped<IClientRepository, ClientRepositoryPure>();
        services.AddScoped<IDiscountRepository, DiscountRepositoryPure>();
        services.AddScoped<ICashClosingRepository, CashClosingRepositoryPure>();
        services.AddScoped<ICategoryRepository, CategoryRepositoryPure>();

        // Register Authentication Services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Register Stored Procedure Services
        services.AddScoped<IOrdenService, OrdenService>();
        services.AddScoped<IPagoService, PagoService>();
        services.AddScoped<ICorteCajaService, CorteCajaService>();
        services.AddScoped<IReporteService, ReporteService>();

        // Register Thermal Printer Service
        services.Configure<PrinterSettings>(configuration.GetSection(PrinterSettings.SectionName));
        services.AddScoped<ITicketPrinterService, TicketPrinterService>();

        // Report Services
        services.Configure<ReportSettings>(configuration.GetSection(ReportSettings.SectionName));
        services.AddScoped<IReportDataService,   ReportDataService>();
        services.AddScoped<IReportConfigService, ReportConfigService>();
        services.AddScoped<IEmailService,        SmtpEmailService>();
        services.AddScoped<IReportFileGenerator, PdfReportGenerator>();
        services.AddScoped<IReportFileGenerator, ExcelReportGenerator>();

        return services;
    }
}
