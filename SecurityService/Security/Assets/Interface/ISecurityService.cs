namespace Security;

using Security.Asset;
using Security.Data;
using Security.Service;

public interface ISecurityService
{
    //CRUD USER
    Task<UserRegistrationResult> CreateUser(string username, string password);
    //Task<bool> DeleteUser(string userId);
    Task<List<User>> GetAllUsers();
    Task<User?> GetUserById(Guid userId);
    Task<User?> GetUserByName(string username);
    Task<bool> IsAuthenticated(Guid userId);
    Task<LoginPayload> Login(string username, string password, CancellationToken cancellationToken = default);
    Task<User?> ValidateUserClaim(Guid userId, string claimType, string claimValue);
    Task Logout();

    // ==== ROLE METHOD ====
    Task<List<Role>> GetRoles();
    Task<List<string>> GetRolesForUserId(Guid userId);
    Task<Role> CreateRole(string roleName);
    Task<List<Role>> CreateRoles(List<string> roleNames, bool ignoreIfExists = true);
    Task<bool> AssignRole(Guid userId, Guid roleId);
    //Task<bool> DeleteRole(string roleId);


}