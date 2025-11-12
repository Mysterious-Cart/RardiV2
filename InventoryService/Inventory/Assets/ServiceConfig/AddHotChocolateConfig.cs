using Npgsql;

namespace Inventory.Assets.ServiceConfig;

public static class AddHotChocolateConfig
{
    public static IServiceCollection AddHotChocolateConfiguration(this IServiceCollection services, WebApplicationBuilder builder)
    {
        services
            .AddGraphQLServer()
            .AddApolloFederation()
            .AddQueryType<Inventory.Graphql.Query>()
            .AddMutationType<Inventory.Graphql.Mutation>()
            .AddSubscriptionType<Inventory.Graphql.Subscription>()
            .AddPostgresSubscriptions((sp, option) => {
                option.ConnectionFactory = ct =>
                {
                    var Npgsqlbuilder = new NpgsqlDataSourceBuilder(  
                        builder.Configuration.GetConnectionString("DefaultConnection")
                    );

                    // we do not need pooling for long running connections
                    Npgsqlbuilder.ConnectionStringBuilder.Pooling = false;
                    // we set the keep alive to 30 seconds
                    Npgsqlbuilder.ConnectionStringBuilder.KeepAlive = 30;
                    // as these tasks often run in the background we do not want to enlist them so they do not
                    // interfere with the main transaction
                    Npgsqlbuilder.ConnectionStringBuilder.Enlist = false;

                    var dataSource = Npgsqlbuilder.Build();

                    return dataSource.OpenConnectionAsync(ct);
                }
                
                ;
            })
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