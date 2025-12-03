using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TicketBooking.Application.Common.Behaviors;

namespace TicketBooking.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Get the current assembly (TicketBooking.Application) to scan for types.
            var assembly = typeof(DependencyInjection).Assembly;

            // Configure MediatR (The messaging pattern library).
            services.AddMediatR(configuration =>
            {
                // Register all Command/Query Handlers found in this assembly.
                configuration.RegisterServicesFromAssembly(assembly);

                // Register the ValidationBehavior into the Request Pipeline.
                // This ensures every request goes through validation BEFORE reaching the Handler.
                configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            // Scan the assembly and automatically register all FluentValidation validators.
            // (e.g., CreateUserCommandValidator will be found and registered here).
            services.AddValidatorsFromAssembly(assembly);

            // Return the service collection to allow chaining in Program.cs.
            return services;
        }
    }
}