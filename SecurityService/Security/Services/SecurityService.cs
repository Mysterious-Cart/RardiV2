namespace Security.Service;

using Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Security.Asset;
using Security.Data;

public class SecurityService(
        ILogger<SecurityService> logger,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        RoleManager<Role> roleManager,
        JwtTokenService jwtTokenService,
        SecurityContext context
    ) : ISecurityService
{
    private readonly ILogger<SecurityService> _logger = logger;
    private readonly UserManager<User> _userManager = userManager;
    private readonly SignInManager<User> _signInManager = signInManager;
    private readonly RoleManager<Role> _roleManager = roleManager;
    private readonly JwtTokenService _jwtTokenService = jwtTokenService;
    private readonly SecurityContext _context = context;

    public async Task<UserRegistrationResult> CreateUser(string username, string password)
    {

        try
        {
            var user = new User { UserName = username, Email = username };
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                var RegisteredUser = await _userManager.FindByNameAsync(username);
                return new UserRegistrationResult(true, RegisteredUser, null);
            }
            else
            {
                _logger.LogWarning("Creation Failed. Reason: {errors}", string.Join(", ", result.Errors.Select(e => e.Description)));

                return new UserRegistrationResult(false, null,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }

        }
        catch (Exception ex)
        {
            _logger.LogError("An fatal error occurred while creating user: {message}", ex.Message);
            throw new InvalidOperationException($"A fatal error occurred while creating user: {ex.Message}");
        }
    }

    public async Task<User?> GetUserById(Guid userId) => await _userManager.FindByIdAsync(userId.ToString());
    public async Task<User?> GetUserByName(string username) => await _userManager.FindByNameAsync(username);
    public async Task<List<User>> GetAllUsers() => await _userManager.Users.ToListAsync();

    public async Task<bool> IsAuthenticated(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user != null;
    }

    public async Task<LoginPayload> Login(string username, string password, CancellationToken cancellationToken = default)
    {
/*
#if DEBUG
        // Create a test user
        // This code only runs in debug builds
        if (username == "admin" && password == "Admin@123")
        {
            var (admin, role) = await SeedTestUser(cancellationToken);
            return new LoginPayload(
                true,
                admin,
                _jwtTokenService.GenerateToken(
                    admin,
                    // Role is guaranteed to be not null here
                    [role.Name!.ToString()]
                ),
                "Login successful."
            );
        }
#endif
*/
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByNameAsync(username);
        if (user != null && await _userManager.CheckPasswordAsync(user, password))
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                await _signInManager.SignInAsync(user, isPersistent: false);
                var token = _jwtTokenService.GenerateToken(user, await GetRolesForUserId(user.Id));
                return new LoginPayload(true, user, token, "Login successful.");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred during login for user {username}: {message}. Traces {Traces}", username, ex.Message, ex.StackTrace);
                return new LoginPayload(false, null, null, "Login failed due to internal error, please retry at other time.");
            }

        }
        else
        {
            return new LoginPayload(false, null, null, "Invalid username or password.");
        }
    }

    private async Task<(User, Role)> SeedTestUser(CancellationToken cancellationToken = default)
    {
        // Avoid too much round trip to Db
        var admin = new User { UserName = "admin", Email = "admin@example.com" };
        var AdministratorRole = new Role { Name = "Administrator" };

        if ((await _userManager.FindByNameAsync(admin.UserName)) == null)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                //Create User call Admin 
                await _userManager.CreateAsync(admin, "Admin@123");
                cancellationToken.ThrowIfCancellationRequested();
                //Create Role call Administrator
                await _roleManager.CreateAsync(AdministratorRole);
                cancellationToken.ThrowIfCancellationRequested();
                //Assign Role to User
                await _userManager.AddToRoleAsync(admin, AdministratorRole.Name);
                //Sign in the user
                await _signInManager.SignInAsync(admin, isPersistent: false, "JwtBearer");
                var token = _jwtTokenService.GenerateToken(admin, [AdministratorRole.Name.ToString()]);
                await transaction.CommitAsync(cancellationToken);

                return (admin, AdministratorRole);

            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError("An error occurred while creating test admin user: {message}. Traces {Traces}", ex.Message, ex.StackTrace);
                throw new InvalidOperationException($"Login failed due to an internal error: {ex.Message}", ex.InnerException);
            }
            finally
            {
                await transaction.DisposeAsync();
            }
        }
        else
        {
            //If user already exists, just return it

            return (admin, AdministratorRole);
        }


    }



    public async Task<User?> ValidateUserClaim(Guid userId, string claimType, string claimValue)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }
        var userClaims = await _userManager.GetClaimsAsync(user);
        var hasClaim = userClaims.Any(c => c.Type == claimType && c.Value == claimValue);

        if (hasClaim)
        {
            return user;
        }
        else
        {
            throw new UnauthorizedAccessException("User does not have the required claim.");
        }
    }
    public async Task Logout()
    {
        await _signInManager.SignOutAsync();
    }

    // ==== ROLE METHOD ====

    public async Task<List<Role>> GetRoles() => await _roleManager.Roles.ToListAsync();
    public async Task<List<string>> GetRolesForUserId(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }
        var roles = await _userManager.GetRolesAsync(user);
        return roles?.ToList() ?? [];
    }

    public async Task<Role> CreateRole(string roleName)
    {
        if(await _roleManager.RoleExistsAsync(roleName))
        {
            throw new InvalidOperationException("Role already exists.");
        }
        var result = await _roleManager.CreateAsync(new Role() { Name = roleName });
        if (result.Succeeded)
        {
            return await _roleManager.FindByNameAsync(roleName)
            ?? throw new InvalidOperationException("Role creation failed.");
        }
        else
        {
            throw new InvalidOperationException(
                $"Failed to create role: {string.Join(", ", result.Errors.Select(e => e.Description))}"
            );
        }
    }

    public async Task<List<Role>> CreateRoles(List<string> roleName, bool ignoreIfExists = true)
    {
        var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Check Role with same name
            var matches = await _context.Roles.Where(r => roleName.Contains(r.Name!)).ToListAsync();
            var roles = new List<Role>();
            // Convert Name to Role
            foreach (var r in roleName)
            {
                var role = new Role() { Name = r };
                roles.Add(role);
            }
            // Ignore Roles if exist
            if (!matches.Count.Equals(0) && ignoreIfExists)
            {
                foreach (var match in matches)
                {
                    roles.RemoveAll(r => r.Name == match.Name);
                }
            }
            else if(!matches.Count.Equals(0) && !ignoreIfExists)
            {
                throw new InvalidOperationException("Some roles already exist.");
            }

            await _context.AddRangeAsync(roles);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();


            return await _context.Roles.Where(r => roleName.Contains(r.Name!)).ToListAsync()
                ?? throw new InvalidOperationException("Role creation failed.");
            
        }catch(Exception ex)
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException($"A fatal error occurred while creating roles: {ex.Message}");
        }
        finally
        {
            await transaction.DisposeAsync();
        }
        
    }

    public async Task<bool> AssignRole(Guid userId, Guid roleId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        var role = await _roleManager.FindByIdAsync(roleId.ToString());

        if (user == null || role == null)
        {
            throw new InvalidOperationException("User or role not found.");
        }

        var result = await _userManager.AddToRoleAsync(user, role.Name!);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to assign role: {string.Join(", ", result.Errors.Select(e => e.Description))}"
            );
        }
        return true;
    }

    public async Task<bool> UnassignRole(Guid userId, Guid roleId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        var role = await _roleManager.FindByIdAsync(roleId.ToString());

        if (user == null || role == null)
        {
            throw new InvalidOperationException("User or role not found.");
        }

        var result = await _userManager.RemoveFromRoleAsync(user, role.Name!);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to remove role: {string.Join(", ", result.Errors.Select(e => e.Description))}"
            );
        }
        return true;
    }

    public async Task<bool> DeleteRole(Guid roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString()) ?? throw new InvalidOperationException("Role not found.");


        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to delete role: {string.Join(", ", result.Errors.Select(e => e.Description))}"
            );
        }
        return true;
    }
}