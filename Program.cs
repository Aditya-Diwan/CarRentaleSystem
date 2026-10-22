using CarRentalSystem.Models;
using CarRentalSystem.Repository;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace CarRentalSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder =
                WebApplication.CreateBuilder(args);

            // =====================================
            // MVC
            // =====================================

            builder.Services
                .AddControllersWithViews();

            // =====================================
            // DATABASE CONNECTION
            // =====================================

            builder.Services
                .AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(
                        builder.Configuration
                        .GetConnectionString(
                            "EliteCarConnection")));

            // =====================================
            // STRIPE CONFIGURATION
            // =====================================

            builder.Services.Configure<StripeSettings>(
                builder.Configuration.GetSection(
                    "Stripe"));

            StripeConfiguration.ApiKey =
                builder.Configuration["Stripe:SecretKey"];

            // =====================================
            // REPOSITORY SERVICES
            // =====================================

            builder.Services
                .AddScoped<DbHelper>();

            builder.Services
                .AddScoped<CarRentalSystem.Repository.Account>();

            builder.Services
                .AddScoped<CarService>();

            builder.Services
                .AddScoped<BookingService>();

            builder.Services
                .AddScoped<VerificationService>();

            builder.Services
                .AddScoped<DashboardService>();

            // =====================================
            // SESSION
            // =====================================

            builder.Services
                .AddSession(options =>
                {
                    options.IdleTimeout =
                        TimeSpan.FromMinutes(30);

                    options.Cookie.HttpOnly =
                        true;

                    options.Cookie.IsEssential =
                        true;
                });

            var app =
                builder.Build();

            // =====================================
            // ERROR HANDLING
            // =====================================

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler(
                    "/Home/Error");

                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // =====================================
            // STATIC FILES
            // =====================================

            app.UseStaticFiles();

            app.UseRouting();

            // =====================================
            // SESSION
            // =====================================

            app.UseSession();

            // =====================================
            // AUTHORIZATION
            // =====================================

            app.UseAuthorization();

            // =====================================
            // DEFAULT ROUTE
            // =====================================

            app.MapControllerRoute(
                name: "default",
                pattern:
                "{controller=Account}/{action=Login}/{id?}");

            app.Run();
        }
    }
}