namespace Security.Assets.ServiceConfig;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Security.Data;
public static class AddIdentityConfig
{
    public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services) {
        services.AddIdentityCore<User>()
        .AddRoles<Role>()
        .AddEntityFrameworkStores<SecurityContext>()
        .AddUserManager<UserManager<User>>()
        .AddSignInManager<SignInManager<User>>()
        .AddRoleManager<RoleManager<Role>>()
        .AddDefaultTokenProviders();

        return services;
    }
}