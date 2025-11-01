namespace Security.Service;

public class SeederService(ISecurityService securityService)
{
    private readonly ISecurityService securityService = securityService;
    public async Task Seed()
    {
        var roles = new[] { "Administrator", "User", "Manager" };

        await securityService.CreateRoles(roles.ToList());
    }
}