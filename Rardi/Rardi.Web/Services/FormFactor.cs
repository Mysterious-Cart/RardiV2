using Rardi.Shared.Services;

namespace Rardi.Web.Services
{
    public class FormFactor : IFormFactor
    {
        public DevicePlateform GetFormFactor()
        {
            return DevicePlateform.Web;
        }

        public string GetPlatform()
        {
            return Environment.OSVersion.ToString();
        }
    }
}
