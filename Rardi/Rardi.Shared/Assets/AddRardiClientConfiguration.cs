using Microsoft.Extensions.DependencyInjection;

namespace Rardi.Shared.Assets
{
    public static class AddRardiClientConfiguration
    {
        public static IServiceCollection AddRardiClient(this IServiceCollection services)
        {
            string RouterBaseAddress = "http://localhost:4000/graphql/";
            services.AddRardiClientWeb()
            .ConfigureHttpClient(client => client.BaseAddress = new Uri(RouterBaseAddress));

            return services;
        }
    }
}