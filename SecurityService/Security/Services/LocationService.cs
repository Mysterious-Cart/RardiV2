namespace Security.Service;

using Microsoft.EntityFrameworkCore;
using Security.Data;
public class LocationService(SecurityContext context){
    private readonly SecurityContext _context = context;
    public async Task<List<Location>> GetAllLocations()
    {
        return _context.Locations.ToList();
    }

    public async Task<Location?> GetLocationById(Guid locationId)
    {
        return await _context.Locations.FindAsync(locationId);
    }

    public async Task<Location?> GetLocation(string name)
    {
        return await _context.Locations.FirstOrDefaultAsync(l => l.Name.Contains(name));
    }
    /// <summary>
    /// Create a new location.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Locations with the same name already exist.</exception>
    public async Task<Location> CreateLocation(string name)
    {
        if(await _context.Locations.AnyAsync(l => l.Name == name))
            throw new ArgumentException($"Location with name {name} already exists.");
        var location = new Location { Id = Guid.NewGuid(), Name = name };
        _context.Locations.Add(location);
        await _context.SaveChangesAsync();
        return location;
    }

    /// <summary>
    /// Create multiple locations. Ignore if any location already exists.
    /// </summary>
    /// <param name="names"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<List<Location>> CreateLocations(List<string> names)
    {
        var existingLocations = await _context.Locations
            .Where(l => names.Contains(l.Name))
            .Select(l => l.Name)
            .ToListAsync();
        var duplicateNames = names.Intersect(existingLocations).ToList();
        var namesToCreate = names.Except(duplicateNames).ToList();
        var locations = namesToCreate.Select(name => new Location { Id = Guid.NewGuid(), Name = name }).ToList();

        _context.Locations.AddRange(locations);
        await _context.SaveChangesAsync();
        return locations;
    }
    public async Task<Location> UpdateLocation(Guid locationId, string name)
    {
        var location = await _context.Locations.FindAsync(locationId);
        ArgumentNullException.ThrowIfNull(location, nameof(location));

        location.Name = name;
        await _context.SaveChangesAsync();
        return location;
    }

    public async Task<Guid> DeleteLocation(Guid locationId)
    {
        var location = await _context.Locations.FindAsync(locationId);
        ArgumentNullException.ThrowIfNull(location, nameof(location));

        _context.Locations.Remove(location);
        await _context.SaveChangesAsync();
        return location.Id;
    }


}