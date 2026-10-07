using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using VedicServicesPortal.Models;

namespace VedicServicesPortal.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // ==========================================
            // CREATE DATABASE IF IT DOES NOT EXIST
            // ==========================================

            context.Database.EnsureCreated();

            // Ensure Username column exists on Users table
            try
            {
                context.Database.ExecuteSqlRaw(@"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Username')
                    BEGIN
                        ALTER TABLE Users ADD Username NVARCHAR(100) NULL;
                    END
                ");
            }
            catch { }

            // Ensure SiteSettings table exists
            try
            {
                context.Database.ExecuteSqlRaw(@"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SiteSettings')
                    BEGIN
                        CREATE TABLE SiteSettings (
                            Id INT NOT NULL PRIMARY KEY IDENTITY(1,1),
                            BrandName NVARCHAR(100) NOT NULL,
                            BrandTagline NVARCHAR(200) NULL,
                            BrandLogoOm NVARCHAR(20) NULL,
                            SiteDescription NVARCHAR(MAX) NULL,
                            BrandStory NVARCHAR(MAX) NULL,
                            HeroHeadline NVARCHAR(200) NULL,
                            HeroSubheadline NVARCHAR(200) NULL,
                            HeroDescription NVARCHAR(MAX) NULL,
                            TrustStatsText NVARCHAR(300) NULL,
                            AcharyaDeskTitle NVARCHAR(200) NOT NULL,
                            AcharyaDeskSubtitle NVARCHAR(300) NULL,
                            ContactPhone NVARCHAR(100) NOT NULL,
                            ContactEmail NVARCHAR(150) NOT NULL,
                            ContactAddress NVARCHAR(300) NULL,
                            HelplineHours NVARCHAR(150) NULL,
                            EmergencyWhatsApp NVARCHAR(100) NULL,
                            AcharyaInquiryResponseTime NVARCHAR(300) NULL,
                            GuaranteeBadgeText NVARCHAR(200) NULL,
                            FooterCopyrightText NVARCHAR(300) NULL,
                            UpiId NVARCHAR(100) NULL,
                            QrCodeImagePath NVARCHAR(500) NULL,
                            LastUpdated DATETIME2 NOT NULL DEFAULT GETUTCDATE()
                        );
                    END
                    ELSE
                    BEGIN
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SiteSettings') AND name = 'QrCodeImagePath')
                        BEGIN
                            ALTER TABLE SiteSettings ADD QrCodeImagePath NVARCHAR(500) NULL;
                        END
                    END
                ");
            }
            catch { }

            // Ensure Default Site Settings exist
            try
            {
                var siteSetting = context.SiteSettings.FirstOrDefault();
                if (siteSetting == null)
                {
                    siteSetting = new SiteSetting
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
                        LastUpdated = DateTime.UtcNow
                    };
                    context.SiteSettings.Add(siteSetting);
                    context.SaveChanges();
                }
            }
            catch { }

            // ==========================================
            // CREATE / UPDATE ADMIN USER
            // ==========================================

            var admin = context.Users
                .FirstOrDefault(x => x.Email == "admin@gmail.com");

            if (admin == null)
            {
                admin = new User
                {
                    Name = "Admin",
                    Username = "admin",
                    Email = "admin@gmail.com",
                    Password = "Admin@123",
                    Role = "Admin"
                };

                context.Users.Add(admin);
                context.SaveChanges();
            }
            else
            {
                admin.Role = "Admin";
                if (string.IsNullOrEmpty(admin.Username))
                {
                    admin.Username = "admin";
                }
                if (admin.Password == "123456")
                {
                    admin.Password = "Admin@123";
                }
                context.SaveChanges();
            }


            // ==========================================
            // EXPANDED VEDIC SERVICES (12 SACRED CEREMONIES)
            // ==========================================

            var allServices = new List<Service>
            {
                new Service
                {
                    ServiceName = "Ganesh Puja & Havan",
                    Description = "Traditional Ganesh Puja & Havan performed by an experienced Pandit to eliminate obstacles and bestow auspicious beginnings.",
                    Price = 1500,
                    Duration = 2,
                    IsActive = true
                },
                new Service
                {
                    ServiceName = "Satyanarayan Katha",
                    Description = "Traditional Satyanarayan Katha & Puja with sacred Prasad vidhi and Vedic chants.",
                    Price = 2500,
                    Duration = 3,
                    IsActive = true
                },
                new Service
                {
                    ServiceName = "Griha Pravesh & Vastu Shanti",
                    Description = "Complete house warming ceremony according to Vedic traditions with Vastu Dosh Nivaran and Navgraha invocation.",
                    Price = 3500,
                    Duration = 4,
                    IsActive = true
                },
                new Service
                {
                    ServiceName = "Navgraha Shanti Puja",
                    Description = "Vedic Navgraha Puja and 9 Graha Havan for planetary harmony, nullifying doshas and restoring cosmic balance.",
                    Price = 3000,
                    Duration = 3,
                    IsActive = true
                },
                new Service
                {
                    ServiceName = "Maha Mrityunjaya Jaap & Homa",
                    Description = "Potent Shiva healing chants and homa invoking longevity, health, overcoming chronic ailments, and spiritual renewal.",
                    Price = 4500,
                    Duration = 5,
                    IsActive = true
                },
                new Service
                {
                    ServiceName = "Lakshmi Kuber Homa",
                    Description = "Dedicated fire ritual with Sri Suktam recitation to manifest financial stability, eliminate debt, and accelerate business prosperity.",
                    Price = 3800,
                    Duration = 3,
                    IsActive = true
                },
                new Service
                {
                    ServiceName = "Rudrabhishek Puja",
                    Description = "Sacred Abhishekam of Shiva Lingam with 11 sacred Dravyas (milk, honey, sugarcane juice) and continuous Sri Rudram chanting.",
                    Price = 2100,
                    Duration = 2,
                    IsActive = true
                },
                new Service
                {
                    ServiceName = "Vivah Sanskar (Vedic Wedding)",
                    Description = "Authentic 7 Pheras, Kanyadaan, Saptapadi, and sacred Agni Vivah Sanskar ceremony by learned Gurukul Acharyas.",
                    Price = 11000,
                    Duration = 6,
                    IsActive = true
                },
                new Service
                {
                    ServiceName = "Namkaran Sanskar (Naming Ceremony)",
                    Description = "Sacred Vedic naming ceremony for newborns with Nakshatra Puja, Surya Darshan, and family blessings.",
                    Price = 2100,
                    Duration = 2,
                    IsActive = true
                },
                new Service
                {
                    ServiceName = "Saraswati Puja & Vidya Arambh",
                    Description = "Goddess Saraswati blessing ritual for students, learning, academic success, exams, and creative arts.",
                    Price = 1800,
                    Duration = 2,
                    IsActive = true
                },
                new Service
                {
                    ServiceName = "Kaal Sarp Dosh Nivaran Puja",
                    Description = "Comprehensive Rahu-Ketu Shanti ritual to mitigate Kaal Sarp Yog obstacles in marital life, health, and career.",
                    Price = 5100,
                    Duration = 4,
                    IsActive = true
                },
                new Service
                {
                    ServiceName = "Sundarkand Paath & Hanuman Puja",
                    Description = "Devotional recitation of Sundarkand with Bajrang Baan, Sindoor Arpan, and Hanuman Havan for courage and protection.",
                    Price = 2500,
                    Duration = 3,
                    IsActive = true
                }
            };

            foreach (var svc in allServices)
            {
                var existing = context.Services.FirstOrDefault(s => s.ServiceName == svc.ServiceName);
                if (existing == null)
                {
                    // Also check if short name matches (e.g. "Ganesh Puja" vs "Ganesh Puja & Havan")
                    var shortMatch = context.Services.FirstOrDefault(s => svc.ServiceName.StartsWith(s.ServiceName));
                    if (shortMatch != null)
                    {
                        shortMatch.ServiceName = svc.ServiceName;
                        shortMatch.Description = svc.Description;
                        shortMatch.Price = svc.Price;
                        shortMatch.Duration = svc.Duration;
                    }
                    else
                    {
                        context.Services.Add(svc);
                    }
                }
            }
            context.SaveChanges();


            // ==========================================
            // EXPANDED GURUKUL PANDITS (8 VERIFIED SCHOLARS)
            // ==========================================

            var allPandits = new List<Pandit>
            {
                new Pandit
                {
                    Name = "Pandit Ramesh Sharma",
                    Mobile = "9876543210",
                    Email = "ramesh@gmail.com",
                    Experience = 10,
                    Specialization = "Ganesh Puja & Shukla Yajurveda",
                    IsAvailable = true
                },
                new Pandit
                {
                    Name = "Pandit Mahesh Joshi",
                    Mobile = "9876543211",
                    Email = "mahesh@gmail.com",
                    Experience = 8,
                    Specialization = "Satyanarayan Katha & Vrata",
                    IsAvailable = true
                },
                new Pandit
                {
                    Name = "Pandit Suresh Trivedi",
                    Mobile = "9876543212",
                    Email = "suresh@gmail.com",
                    Experience = 12,
                    Specialization = "Griha Pravesh & Vastu Shanti",
                    IsAvailable = true
                },
                new Pandit
                {
                    Name = "Acharya Vidyadhar Shukla",
                    Mobile = "9876543213",
                    Email = "vidyadhar@gmail.com",
                    Experience = 22,
                    Specialization = "Griha Pravesh, Vivah & Shukla Yajurveda",
                    IsAvailable = true
                },
                new Pandit
                {
                    Name = "Pandit Raghavan Namboodiri",
                    Mobile = "9876543214",
                    Email = "raghavan@gmail.com",
                    Experience = 18,
                    Specialization = "Ganapathi Homam, Sudarshana & Kerala Tantra",
                    IsAvailable = true
                },
                new Pandit
                {
                    Name = "Pandit Rameshwar Joshi",
                    Mobile = "9876543215",
                    Email = "rameshwar@gmail.com",
                    Experience = 15,
                    Specialization = "Satyanarayan Katha, Rudrabhishek & Krishna Yajurveda",
                    IsAvailable = true
                },
                new Pandit
                {
                    Name = "Acharya Devkinandan Sharma",
                    Mobile = "9876543216",
                    Email = "devkinandan@gmail.com",
                    Experience = 20,
                    Specialization = "Navgraha Shanti, Jyotish & Vastu Dosha",
                    IsAvailable = true
                },
                new Pandit
                {
                    Name = "Pandit Kanhaiya Lal Shastri",
                    Mobile = "9876543217",
                    Email = "kanhaiya@gmail.com",
                    Experience = 16,
                    Specialization = "Maha Mrityunjaya Jaap & Vedic Homas",
                    IsAvailable = true
                }
            };

            foreach (var p in allPandits)
            {
                var existing = context.Pandits.FirstOrDefault(x => x.Email == p.Email || x.Name == p.Name);
                if (existing == null)
                {
                    context.Pandits.Add(p);
                }
                else
                {
                    existing.Specialization = p.Specialization;
                    existing.Experience = p.Experience;
                    existing.IsAvailable = true;
                }
            }
            context.SaveChanges();


            // ==========================================
            // CREATE SAMPLE TIME SLOTS FOR PANDITS
            // ==========================================

            var panditsList = context.Pandits.Where(p => p.IsAvailable).ToList();
            DateTime tomorrow = DateTime.Today.AddDays(1);
            DateTime dayAfter = DateTime.Today.AddDays(2);

            foreach (var pandit in panditsList)
            {
                bool hasTomorrowSlots = context.TimeSlots.Any(t => t.PanditId == pandit.PanditId && t.Date.Date == tomorrow.Date);
                if (!hasTomorrowSlots)
                {
                    context.TimeSlots.AddRange(
                        new TimeSlot
                        {
                            PanditId = pandit.PanditId,
                            Date = tomorrow,
                            StartTime = "09:00 AM",
                            EndTime = "11:00 AM",
                            IsBooked = false
                        },
                        new TimeSlot
                        {
                            PanditId = pandit.PanditId,
                            Date = tomorrow,
                            StartTime = "02:30 PM",
                            EndTime = "04:30 PM",
                            IsBooked = false
                        }
                    );
                }

                bool hasDayAfterSlots = context.TimeSlots.Any(t => t.PanditId == pandit.PanditId && t.Date.Date == dayAfter.Date);
                if (!hasDayAfterSlots)
                {
                    context.TimeSlots.AddRange(
                        new TimeSlot
                        {
                            PanditId = pandit.PanditId,
                            Date = dayAfter,
                            StartTime = "06:00 AM",
                            EndTime = "08:00 AM",
                            IsBooked = false
                        },
                        new TimeSlot
                        {
                            PanditId = pandit.PanditId,
                            Date = dayAfter,
                            StartTime = "11:45 AM",
                            EndTime = "01:45 PM",
                            IsBooked = false
                        }
                    );
                }
            }

            context.SaveChanges();

            // ==========================================
            // SEED SAMPLE DEVOTEE USERS
            // ==========================================
            var sampleDevotees = new List<User>
            {
                new User
                {
                    Name = "Rajesh Patel",
                    Username = "rajesh_patel",
                    Email = "rajesh.patel@gmail.com",
                    Password = "User@123",
                    Role = "User"
                },
                new User
                {
                    Name = "Ananya Sharma",
                    Username = "ananya_sharma",
                    Email = "ananya.sharma@gmail.com",
                    Password = "User@123",
                    Role = "User"
                },
                new User
                {
                    Name = "Siddharth Trivedi",
                    Username = "siddharth_trivedi",
                    Email = "siddharth.trivedi@gmail.com",
                    Password = "User@123",
                    Role = "User"
                }
            };

            foreach (var d in sampleDevotees)
            {
                if (!context.Users.Any(u => u.Email == d.Email))
                {
                    context.Users.Add(d);
                }
            }
            context.SaveChanges();

            // ==========================================
            // SEED SAMPLE BOOKINGS ACROSS DIFFERENT DATES & TIMES (WITHIN 2 MONTHS)
            // ==========================================
            if (context.Bookings.Count() < 6)
            {
                var adminUser = context.Users.FirstOrDefault(u => u.Email == "admin@gmail.com");
                var rajeshUser = context.Users.FirstOrDefault(u => u.Email == "rajesh.patel@gmail.com") ?? adminUser;
                var ananyaUser = context.Users.FirstOrDefault(u => u.Email == "ananya.sharma@gmail.com") ?? adminUser;
                var siddharthUser = context.Users.FirstOrDefault(u => u.Email == "siddharth.trivedi@gmail.com") ?? adminUser;

                var services = context.Services.ToList();
                var pandits = context.Pandits.ToList();

                var ganesh = services.FirstOrDefault(s => s.ServiceName.Contains("Ganesh")) ?? services.First();
                var rudra = services.FirstOrDefault(s => s.ServiceName.Contains("Rudra")) ?? services.First();
                var satya = services.FirstOrDefault(s => s.ServiceName.Contains("Satyanarayan")) ?? services.First();
                var griha = services.FirstOrDefault(s => s.ServiceName.Contains("Griha Pravesh")) ?? services.First();
                var navgraha = services.FirstOrDefault(s => s.ServiceName.Contains("Navgraha")) ?? services.First();
                var mrityunjaya = services.FirstOrDefault(s => s.ServiceName.Contains("Mrityunjaya")) ?? services.First();
                var lakshmi = services.FirstOrDefault(s => s.ServiceName.Contains("Lakshmi")) ?? services.First();
                var vivah = services.FirstOrDefault(s => s.ServiceName.Contains("Vivah")) ?? services.First();
                var kaalSarp = services.FirstOrDefault(s => s.ServiceName.Contains("Kaal Sarp")) ?? services.First();
                var sundarkand = services.FirstOrDefault(s => s.ServiceName.Contains("Sundarkand")) ?? services.First();
                var namkaran = services.FirstOrDefault(s => s.ServiceName.Contains("Namkaran")) ?? services.First();

                var pRamesh = pandits.FirstOrDefault(p => p.Name.Contains("Ramesh Sharma")) ?? pandits.First();
                var pRameshwar = pandits.FirstOrDefault(p => p.Name.Contains("Rameshwar")) ?? pandits.First();
                var pMahesh = pandits.FirstOrDefault(p => p.Name.Contains("Mahesh")) ?? pandits.First();
                var pSuresh = pandits.FirstOrDefault(p => p.Name.Contains("Suresh")) ?? pandits.First();
                var pDevki = pandits.FirstOrDefault(p => p.Name.Contains("Devkinandan")) ?? pandits.First();
                var pKanhaiya = pandits.FirstOrDefault(p => p.Name.Contains("Kanhaiya")) ?? pandits.First();
                var pRaghavan = pandits.FirstOrDefault(p => p.Name.Contains("Raghavan")) ?? pandits.First();
                var pVidyadhar = pandits.FirstOrDefault(p => p.Name.Contains("Vidyadhar")) ?? pandits.First();

                // Define sample bookings across 2 months with distinct dates, times, categories, and vidhis
                var sampleSpecs = new[]
                {
                    new {
                        User = adminUser, Pandit = pRamesh, Service = ganesh,
                        Days = 4, Start = "07:30 AM", End = "09:30 AM",
                        Pay = "UPI", Status = "Confirmed", Address = "B-402 Shanti Niketan, Ramnagar, Jam-khambhalya"
                    },
                    new {
                        User = adminUser, Pandit = pRameshwar, Service = rudra,
                        Days = 10, Start = "05:00 PM", End = "07:00 PM",
                        Pay = "Cash on Completion", Status = "Confirmed", Address = "B-402 Shanti Niketan, Ramnagar, Jam-khambhalya"
                    },
                    new {
                        User = adminUser, Pandit = pMahesh, Service = satya,
                        Days = 17, Start = "11:45 AM", End = "02:45 PM",
                        Pay = "Card", Status = "Confirmed", Address = "B-402 Shanti Niketan, Ramnagar, Jam-khambhalya"
                    },
                    new {
                        User = adminUser, Pandit = pSuresh, Service = griha,
                        Days = 30, Start = "06:00 AM", End = "10:00 AM",
                        Pay = "Cash on Completion", Status = "Confirmed", Address = "Plot 14, Vedic Enclave, Dwarka Highway"
                    },
                    new {
                        User = adminUser, Pandit = pRaghavan, Service = lakshmi,
                        Days = 45, Start = "06:30 PM", End = "09:30 PM",
                        Pay = "UPI", Status = "Confirmed", Address = "B-402 Shanti Niketan, Ramnagar, Jam-khambhalya"
                    },
                    new {
                        User = adminUser, Pandit = pVidyadhar, Service = vivah,
                        Days = 55, Start = "10:00 AM", End = "04:00 PM",
                        Pay = "Cash on Completion", Status = "Confirmed", Address = "Mangal Vihar Banquet, Jamnagar Road"
                    },
                    new {
                        User = adminUser, Pandit = pDevki, Service = kaalSarp,
                        Days = -8, Start = "10:00 AM", End = "02:00 PM",
                        Pay = "Cash on Completion", Status = "Completed", Address = "B-402 Shanti Niketan, Ramnagar, Jam-khambhalya"
                    },
                    new {
                        User = rajeshUser, Pandit = pDevki, Service = navgraha,
                        Days = 24, Start = "02:15 PM", End = "05:15 PM",
                        Pay = "UPI", Status = "Confirmed", Address = "77 Gayatri Krupa, Station Road"
                    },
                    new {
                        User = ananyaUser, Pandit = pKanhaiya, Service = mrityunjaya,
                        Days = 37, Start = "08:30 AM", End = "01:30 PM",
                        Pay = "Card", Status = "Confirmed", Address = "Flat 12, Omkar Heights, Temple Chowk"
                    },
                    new {
                        User = siddharthUser, Pandit = pRamesh, Service = sundarkand,
                        Days = 58, Start = "04:30 PM", End = "07:30 PM",
                        Pay = "Cash on Completion", Status = "Confirmed", Address = "G-3 Somnath Society, Bypass Road"
                    },
                    new {
                        User = rajeshUser, Pandit = pMahesh, Service = namkaran,
                        Days = -12, Start = "09:30 AM", End = "11:30 AM",
                        Pay = "Cash on Completion", Status = "Completed", Address = "77 Gayatri Krupa, Station Road"
                    }
                };

                foreach (var spec in sampleSpecs)
                {
                    var ceremonyDate = DateTime.Today.AddDays(spec.Days);

                    // Create or find matching TimeSlot
                    var slot = context.TimeSlots.FirstOrDefault(t =>
                        t.PanditId == spec.Pandit.PanditId &&
                        t.Date.Date == ceremonyDate.Date &&
                        t.StartTime == spec.Start);

                    if (slot == null)
                    {
                        slot = new TimeSlot
                        {
                            PanditId = spec.Pandit.PanditId,
                            Date = ceremonyDate,
                            StartTime = spec.Start,
                            EndTime = spec.End,
                            IsBooked = true
                        };
                        context.TimeSlots.Add(slot);
                        context.SaveChanges();
                    }
                    else
                    {
                        slot.IsBooked = true;
                    }

                    // Create Booking
                    var newBooking = new Booking
                    {
                        UserId = spec.User.UserId,
                        PanditId = spec.Pandit.PanditId,
                        ServiceId = spec.Service.ServiceId,
                        SlotId = slot.SlotId,
                        BookingDate = ceremonyDate,
                        Address = spec.Address,
                        TotalAmount = spec.Service.Price,
                        PaymentMethod = spec.Pay,
                        Status = spec.Status,
                        CreatedAt = DateTime.Now.AddDays(spec.Days < 0 ? spec.Days - 1 : -1)
                    };

                    context.Bookings.Add(newBooking);
                }

                context.SaveChanges();
            }
        }
    }
}
