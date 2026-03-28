using System.Reflection;
using FluentValidation;
using LaundryManagement.Application.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace LaundryManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var applicationAssembly = Assembly.GetExecutingAssembly();

        // Register MediatR con ValidationBehavior pipeline
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(applicationAssembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(applicationAssembly);

        // Register AutoMapper
        services.AddAutoMapper(applicationAssembly);

        return services;
    }
}
