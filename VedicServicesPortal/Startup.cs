using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VedicServicesPortal.Data;
using VedicServicesPortal.Services;

namespace VedicServicesPortal
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        // ==========================================
        // CONFIGURE SERVICES
        // ==========================================

        public void ConfigureServices(IServiceCollection services)
        {
            // ------------------------------------------
            // DATABASE
            // ------------------------------------------

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString(
                        "DefaultConnection"
                    )
                )
            );


            // ------------------------------------------
            // SESSION
            // ------------------------------------------

            services.AddSession(options =>
            {
                options.IdleTimeout =
                    System.TimeSpan.FromMinutes(30);

                options.Cookie.HttpOnly = true;

                options.Cookie.IsEssential = true;
            });


            // ------------------------------------------
            // MVC
            // ------------------------------------------

            services.AddControllersWithViews();

            // ------------------------------------------
            // SITE SETTINGS SERVICE
            // ------------------------------------------

            services.AddScoped<ISiteSettingService, SiteSettingService>();
        }


        // ==========================================
        // CONFIGURE APPLICATION
        // ==========================================

        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env)
        {
            // ------------------------------------------
            // DEVELOPMENT ERROR PAGE
            // ------------------------------------------

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }


            // ------------------------------------------
            // HTTPS REDIRECTION
            // ------------------------------------------

            app.UseHttpsRedirection();


            // ------------------------------------------
            // STATIC FILES
            // ------------------------------------------

            app.UseStaticFiles();


            // ------------------------------------------
            // ROUTING
            // ------------------------------------------

            app.UseRouting();


            // ------------------------------------------
            // ANTI-CACHING HEADERS (PREVENTS BROWSER BACK BUTTON ACCESS AFTER LOGOUT)
            // ------------------------------------------

            app.Use(async (context, next) =>
            {
                context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate, max-age=0";
                context.Response.Headers["Pragma"] = "no-cache";
                context.Response.Headers["Expires"] = "-1";
                await next();
            });


            // ------------------------------------------
            // SESSION
            // IMPORTANT: BEFORE AUTHORIZATION
            // ------------------------------------------

            app.UseSession();


            // ------------------------------------------
            // AUTHORIZATION
            // ------------------------------------------

            app.UseAuthorization();


            // ------------------------------------------
            // DATABASE INITIALIZER
            // ------------------------------------------

            using (var scope =
                   app.ApplicationServices.CreateScope())
            {
                var context =
                    scope.ServiceProvider
                        .GetRequiredService<ApplicationDbContext>();

                DbInitializer.Initialize(context);
            }


            // ------------------------------------------
            // DEFAULT ROUTE
            // ------------------------------------------

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern:
                        "{controller=Home}/{action=Index}/{id?}"
                );
            });
        }
    }
}
