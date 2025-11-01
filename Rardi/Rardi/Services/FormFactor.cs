using Rardi.Shared.Services;

namespace Rardi.Services
{
    public class FormFactor : IFormFactor
    {
        public DevicePlateform GetFormFactor()
        {
            var result = DeviceInfo.Idiom.ToString() switch
            {
                nameof(DeviceIdiom.Phone) => DevicePlateform.Mobile,
                nameof(DeviceIdiom.Tablet) => DevicePlateform.Tablet,
                nameof(DeviceIdiom.Desktop) => DevicePlateform.Desktop,
                _ => DevicePlateform.Unknown,
            };
            return result;
        }

        public string GetPlatform()
        {
            return DeviceInfo.Platform.ToString() + " - " + DeviceInfo.VersionString;
        }
    }
}
