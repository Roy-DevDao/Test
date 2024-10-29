using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Configuration;
using test2.DAO;
using test2.Data;
using test2.Models.Momo;
using test2.Services;

namespace test2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            builder.Services.AddControllersWithViews();
            builder.Services.AddMemoryCache();
            builder.Services.AddScoped<EmailService>();
            builder.Services.AddScoped<TokenService>();
            builder.Services.AddDbContext<DocCareContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DocCare"));
            });

            // Cấu hình Google OAuth và Cookie authentication
            builder.Services.AddAuthentication(options =>
            {
                //options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultScheme = "MyCookieAuth";
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            })

            .AddCookie("MyCookieAuth", options =>
            {
                options.Cookie.Name = "MyAuthCookie";
                options.LoginPath = "/Home/Login";
                options.AccessDeniedPath = "/Home/Login";
            }).AddGoogle(googleOptions =>
            {
                //googleOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                googleOptions.SignInScheme = "MyCookieAuth";
                //IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");
                //googleOptions.ClientId = googleAuthNSection["ClientId"];
                //googleOptions.ClientSecret = googleAuthNSection["ClientSecret"];
                googleOptions.ClientId = builder.Configuration["Google:ClientId"];
                googleOptions.ClientSecret = builder.Configuration["Google:ClientSecret"];
                googleOptions.CallbackPath = new PathString("/signin-google");

            });
            builder.Services.AddTransient<CloudinaryService>();

            builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
            builder.Services.AddScoped<IMomoService, MomoService>();
            builder.Services.AddSingleton<IVnPayService, VnPayService>();
            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddScoped<PatientDao>();
            builder.Services.AddScoped<DoctorDAO>();
            builder.Services.AddScoped<AppointmentDAO>();
            builder.Services.AddScoped<FeedbackDAO>();
            builder.Services.AddScoped<UserDAO>();
            builder.Services.AddScoped<StaffDAO>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Thêm UseAuthentication trước UseAuthorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Default route for Home controller
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Route for StaffController1
            app.MapControllerRoute(
               name: "staff",
               pattern: "{controller=Staff}/{action=Profile}/{id?}");

            app.MapControllerRoute(
               name: "doctor",
               pattern: "{controller=Doctor}/{action=Profile}/{id?}");

            app.MapControllerRoute(
               name: "patient",
               pattern: "{controller=Patient}/{action=Profile}/{id?}");

            app.MapControllerRoute(
              name: "admin",
              pattern: "{controller=Admin}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
