using Rardi.Shared.Services;

namespace Rardi.Web.Client.Services
{
    public class FormFactor : IFormFactor
    {
        public DevicePlateform GetFormFactor()
        {
            return DevicePlateform.WebAssembly;
        }

        public string GetPlatform()
        {
            return Environment.OSVersion.ToString();
        }
    }
}
