using Microsoft.Extensions.DependencyInjection;

namespace TicketBooking.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Tự động tìm tất cả các Command/Query Handler trong project này để đăng ký
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

            return services;
        }
    }
}