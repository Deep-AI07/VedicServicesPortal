using System.Threading.Tasks;
using VedicServicesPortal.Models;

namespace VedicServicesPortal.Services
{
    public interface ISiteSettingService
    {
        SiteSetting GetSettings();
        Task<SiteSetting> GetSettingsAsync();
        void UpdateCache(SiteSetting settings);
        void ClearCache();
    }
}
