using System.Security.Claims;
using HotChocolate.Authorization;
using Security.Assets;

namespace Security.Graphql;

public class Mutation
{
    /// <summary>
    /// Register a new user
    /// </summary>
    [Authorize(Roles = new[] { "Administrator" })]
    public async Task<AuthPayload> Register(
        [Service] ISecurityService securityService,
        string username,
        string password,
        Guid locationId)
    {
        try
        {
            if(await securityService.GetUserByName(username) is not null)
            {
                return new AuthPayload(false,null, "Username already exists");
            }
            var userRegistrationResult = await securityService.CreateUser(username, password, locationId);
            var user = userRegistrationResult.User;
            if (!userRegistrationResult.Succeeded)
            {
                return new AuthPayload(false, null, userRegistrationResult.Message!);
            }
            return new AuthPayload(true, user, "User created successfully");
        }
        catch (Exception ex)
        {
            return new AuthPayload(false, null, ex.Message);
        }
    }

    /// <summary>
    /// Login user
    /// </summary>
    public async Task<LoginPayload> Login(
        [Service] ISecurityService securityService,
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await securityService.Login(username, password, cancellationToken);
        }
        catch(Exception ex)
        {
            return new LoginPayload(false, null, null, ex.Message + ex.StackTrace);
        }
    }

    /// <summary>
    /// Logout user
    /// </summary>
    [Authorize]
    public async Task<bool> Logout(
        [Service] ISecurityService securityService,
        ClaimsPrincipal claimsPrincipal)
    {
        try
        {
            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return false;

            await securityService.Logout();
            return true;
        }
        catch
        {
            return false;
        }
    }

    [Authorize]
    public async Task<RolePayload> CreateRole(
        [Service] ISecurityService securityService,
        string roleName
    )
    {
        try
        {
            var role = await securityService.CreateRole(roleName);
            return new RolePayload(true, role, "Role created successfully");
        }
        catch (Exception ex)
        {
            return new RolePayload(false, null, ex.Message);
        }
        
    }
    [Authorize]
    public async Task<bool> AssignRole(
        [Service] ISecurityService securityService,
        Guid userId,
        string role)
    {
        var user = await securityService.GetUserById(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        if (role == null)
        {
            throw new InvalidOperationException("Invalid role.");
        }
        return true;
        // Assign the role to the user
        // This would need to be implemented in your SecurityService
        // await securityService.AssignRoleToUserAsync(user, role);
    }

    /// <summary>
    /// Grant permission to user (Admin only)
    /// </summary>
    [Authorize(Roles = new[] { "Admin" })]
    public async Task<bool> GrantPermission(
        [Service] ISecurityService securityService,
        string userId,
        string permission)
    {
        try
        {
            // This would need to be implemented in your SecurityService
            // await securityService.GrantPermissionAsync(userId, permission);
            return true;
        }
        catch
        {
            return false;
        }
    }
}