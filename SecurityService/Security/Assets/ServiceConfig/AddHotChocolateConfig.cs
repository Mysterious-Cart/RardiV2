using Npgsql;

namespace Security.Assets.ServiceConfig;

public static class AddHotChocolateConfig
{
    public static IServiceCollection AddHotChocolateConfiguration(this IServiceCollection services, WebApplicationBuilder builder)
    {
        services
            .AddGraphQLServer()
            .AddApolloFederation()
            .AddQueryType<Graphql.Query>()
            .AddMutationType<Graphql.Mutation>()
            .AddProjections()
            .AddFiltering()
            .AddSorting()
            .AddAuthorization()
            .InitializeOnStartup()
            .ModifyRequestOptions(
                o => o.IncludeExceptionDetails =
                    builder.Environment.IsDevelopment());

        return services;
    }
}