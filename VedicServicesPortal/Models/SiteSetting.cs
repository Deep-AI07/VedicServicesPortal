using System;
using System.ComponentModel.DataAnnotations;

namespace VedicServicesPortal.Models
{
    public class SiteSetting
    {
        [Key]
        public int Id { get; set; }

        // ==========================================
        // 1. BRAND IDENTITY
        // ==========================================

        [Required(ErrorMessage = "Brand name is required")]
        [Display(Name = "Brand Name")]
        [StringLength(100, ErrorMessage = "Brand name cannot exceed 100 characters")]
        public string BrandName { get; set; } = "VedicServicesPortal";

        [Display(Name = "Brand Tagline")]
        [StringLength(200, ErrorMessage = "Tagline cannot exceed 200 characters")]
        public string BrandTagline { get; set; } = "Faith • Tradition • Blessed Living";

        [Display(Name = "Sacred Symbol / Logo")]
        [StringLength(20, ErrorMessage = "Symbol cannot exceed 20 characters")]
        public string BrandLogoOm { get; set; } = "ॐ";

        [Display(Name = "Meta / Search Description")]
        public string SiteDescription { get; set; } = "Authentic Vedic rituals, sacred pujas, and verified Gurukul Pandits booking portal. Faith • Tradition • Blessed Living.";

        [Display(Name = "Brand Story (Footer Narrative)")]
        public string BrandStory { get; set; } = "Bridging timeless Vedic spirituality and contemporary technology. Consecrated with devotion, our mission is to ensure every family experiences pure Shastric rituals, sacred mantras, and learned Gurukul Acharyas.";

        // ==========================================
        // 2. HERO SECTION & PROMOTIONS
        // ==========================================

        [Display(Name = "Hero Main Headline")]
        public string HeroHeadline { get; set; } = "Bring Divine Blessings";

        [Display(Name = "Hero Italic Subheadline")]
        public string HeroSubheadline { get; set; } = "Into Your Home";

        [Display(Name = "Hero Supporting Description")]
        public string HeroDescription { get; set; } = "Connect with experienced, verified Pandits for authentic Vedic rituals, sacred pujas, and sanctified ceremonies — conveniently booked online with absolute Shastra vidhi.";

        [Display(Name = "Devotee Trust Statistics Text")]
        public string TrustStatsText { get; set; } = "4.9/5 from 12,500+ sacred ceremonies performed across India & Diaspora";

        // ==========================================
        // 3. CONTACT ACHARYA DESK
        // ==========================================

        [Required(ErrorMessage = "Acharya Desk title is required")]
        [Display(Name = "Acharya Desk Section Title")]
        public string AcharyaDeskTitle { get; set; } = "Contact Acharya Desk";

        [Display(Name = "Acharya Desk Subtitle / Purpose")]
        public string AcharyaDeskSubtitle { get; set; } = "Dedicated Acharya desk for astrological guidance & rituals query.";

        [Required(ErrorMessage = "Contact phone number is required")]
        [Display(Name = "Helpline Mobile Number")]
        public string ContactPhone { get; set; } = "+91 8780495951";

        [Required(ErrorMessage = "Contact email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Acharya Desk Email")]
        public string ContactEmail { get; set; } = "vivekmaghudiya6@gmail.com";

        [Display(Name = "Temple / Office Physical Address")]
        public string ContactAddress { get; set; } = "Ramnagar, Jam-khambhalya";

        [Display(Name = "Helpline Operating Hours")]
        public string HelplineHours { get; set; } = "6:00 AM - 9:30 PM (All 7 Days)";

        [Display(Name = "Emergency / WhatsApp Number")]
        public string EmergencyWhatsApp { get; set; } = "+91 8780495951";

        [Display(Name = "Inquiry Response Commitment Note")]
        public string AcharyaInquiryResponseTime { get; set; } = "Our senior Purohit will call your contact number within 4 business hours.";

        // ==========================================
        // 4. PORTAL ADDITIONS ("ETC")
        // ==========================================

        [Display(Name = "Shastra Guarantee Badge Text")]
        public string GuaranteeBadgeText { get; set; } = "SHASTRA CERTIFIED • AUTHENTICITY GUARANTEE";

        [Display(Name = "Footer Copyright Subtext")]
        public string FooterCopyrightText { get; set; } = "Sanctified with devotion & authentic Vedic tradition.";

        [Display(Name = "UPI ID for Dakshina QR Code")]
        public string UpiId { get; set; } = "vedicservices@upi";

        [Display(Name = "Custom Uploaded QR Code Image")]
        public string QrCodeImagePath { get; set; }

        [Display(Name = "Last Modified Date")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
