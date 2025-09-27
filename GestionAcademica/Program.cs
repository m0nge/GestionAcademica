using GestionAcademica.Models;
using GestionAcademica.Services;
using Microsoft.EntityFrameworkCore;

namespace GestionAcademica
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60); // 1 hora
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddControllers();

            // Configurar nuestro DbContext para la gestión académica
            var gestionConnection = builder.Configuration.GetConnectionString("GestionAcademicaConnection")
                ?? throw new InvalidOperationException("Connection string 'GestionAcademicaConnection' not found.");

            builder.Services.AddDbContext<GestionAcademicaContext>(options =>
                options.UseSqlServer(gestionConnection));

            // Registrar servicios personalizados
            builder.Services.AddScoped<EstudianteValidationService>();

            // ===============================
            // NUEVOS SERVICIOS PARA 2FA
            // ===============================
            builder.Services.AddScoped<EmailService, EmailService>();
            builder.Services.AddScoped<ITwoFactorService, TwoFactorService>();

            // Configurar logging
            builder.Services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.MapControllers();
            app.UseSession();
            app.MapRazorPages();

            // Middleware de autenticación personalizado (MEJORADO)
            app.Use(async (context, next) =>
            {
                var path = context.Request.Path.Value?.ToLower() ?? "";
                var docenteLogueado = context.Session.GetString("DocenteLogueado");
                var twoFactorPending = context.Session.GetString("TwoFactorPending");

                // Si está en la raíz
                if (path == "/")
                {
                    if (string.IsNullOrEmpty(docenteLogueado))
                    {
                        context.Response.Redirect("/Login");
                        return;
                    }
                    else
                    {
                        context.Response.Redirect("/Home/Index");
                        return;
                    }
                }

                // Si hay 2FA pendiente y no está en las páginas permitidas
                if (!string.IsNullOrEmpty(twoFactorPending) &&
                    !path.Contains("/twofactor") &&
                    !path.Contains("/logout") &&
                    !path.Contains("/css") &&
                    !path.Contains("/js") &&
                    !path.Contains("/lib"))
                {
                    context.Response.Redirect("/TwoFactor");
                    return;
                }

                // Si intenta acceder a páginas protegidas sin sesión
                if (string.IsNullOrEmpty(docenteLogueado) &&
                    string.IsNullOrEmpty(twoFactorPending) &&
                    !path.Contains("/login") &&
                    !path.Contains("/twofactor") &&
                    !path.Contains("/error") &&
                    !path.Contains("/css") &&
                    !path.Contains("/js") &&
                    !path.Contains("/lib"))
                {
                    context.Response.Redirect("/Login");
                    return;
                }

                await next();
            });

            app.Run();
        }
    }
}