namespace Security.Service;

public class SeederService(ISecurityService securityService, LocationService locationsService)
{
    private readonly ISecurityService securityService = securityService;
    private readonly LocationService locationsService = locationsService;
    public async Task Seed()
    {
        var roles = new[] { "User", "Manager" };
        var locations = new[] { "CHKS Garage", "TP Garage" };
        await securityService.CreateRoles(roles.ToList());
        await locationsService.CreateLocations(locations.ToList());
        await securityService.SeedAdminUser();
    }
}