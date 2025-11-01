namespace Rardi.Shared.Services
{

    // Service to be resolve by project to provide device-specific information
    public interface IFormFactor
    {
        public DevicePlateform GetFormFactor();
        public string GetPlatform();
    }

    public enum DevicePlateform
    {
        Unknown,
        Web,
        WebAssembly,
        Mobile,
        Desktop,
        Tablet
    }
    
}
