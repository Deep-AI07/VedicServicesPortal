using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VedicServicesPortal.Data;
using VedicServicesPortal.Models;

namespace VedicServicesPortal.Services
{
    public class SiteSettingService : ISiteSettingService
    {
        private readonly ApplicationDbContext _context;
        private static SiteSetting _cachedSettings = null;
        private static readonly object _syncLock = new object();

        public SiteSettingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public SiteSetting GetSettings()
        {
            if (_cachedSettings != null)
            {
                return _cachedSettings;
            }

            lock (_syncLock)
            {
                if (_cachedSettings != null)
                {
                    return _cachedSettings;
                }

                try
                {
                    var settings = _context.SiteSettings.AsNoTracking().FirstOrDefault();
                    if (settings == null)
                    {
                        settings = CreateDefaultSettings();
                        _context.SiteSettings.Add(settings);
                        _context.SaveChanges();
                    }
                    _cachedSettings = settings;
                    return _cachedSettings;
                }
                catch
                {
                    // Fallback to in-memory default if DB is temporarily migrating
                    return CreateDefaultSettings();
                }
            }
        }

        public async Task<SiteSetting> GetSettingsAsync()
        {
            if (_cachedSettings != null)
            {
                return _cachedSettings;
            }

            try
            {
                var settings = await _context.SiteSettings.AsNoTracking().FirstOrDefaultAsync();
                if (settings == null)
                {
                    settings = CreateDefaultSettings();
                    _context.SiteSettings.Add(settings);
                    await _context.SaveChangesAsync();
                }
                _cachedSettings = settings;
                return _cachedSettings;
            }
            catch
            {
                return CreateDefaultSettings();
            }
        }

        public void UpdateCache(SiteSetting settings)
        {
            lock (_syncLock)
            {
                _cachedSettings = settings;
            }
        }

        public void ClearCache()
        {
            lock (_syncLock)
            {
                _cachedSettings = null;
            }
        }

        private static SiteSetting CreateDefaultSettings()
        {
            return new SiteSetting
            {
                BrandName = "VedicServicesPortal",
                BrandTagline = "Faith • Tradition • Blessed Living",
                BrandLogoOm = "ॐ",
                SiteDescription = "Authentic Vedic rituals, sacred pujas, and verified Gurukul Pandits booking portal. Faith • Tradition • Blessed Living.",
                BrandStory = "Bridging timeless Vedic spirituality and contemporary technology. Consecrated with devotion, our mission is to ensure every family experiences pure Shastric rituals, sacred mantras, and learned Gurukul Acharyas.",
                HeroHeadline = "Bring Divine Blessings",
                HeroSubheadline = "Into Your Home",
                HeroDescription = "Connect with experienced, verified Pandits for authentic Vedic rituals, sacred pujas, and sanctified ceremonies — conveniently booked online with absolute Shastra vidhi.",
                TrustStatsText = "4.9/5 from 12,500+ sacred ceremonies performed across India & Diaspora",
                AcharyaDeskTitle = "Contact Acharya Desk",
                AcharyaDeskSubtitle = "Dedicated Acharya desk for astrological guidance & rituals query.",
                ContactPhone = "+91 8780495951",
                ContactEmail = "vivekmaghudiya6@gmail.com",
                ContactAddress = "Ramnagar, Jam-khambhalya",
                HelplineHours = "6:00 AM - 9:30 PM (All 7 Days)",
                EmergencyWhatsApp = "+91 8780495951",
                AcharyaInquiryResponseTime = "Our senior Purohit will call your contact number within 4 business hours.",
                GuaranteeBadgeText = "SHASTRA CERTIFIED • AUTHENTICITY GUARANTEE",
                FooterCopyrightText = "Sanctified with devotion & authentic Vedic tradition.",
                UpiId = "vedicservices@upi",
                QrCodeImagePath = null,
                LastUpdated = DateTime.UtcNow
            };
        }
    }
}
