using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Razor;
using TareasMVC.Servicios;

namespace TareasMVC.Services.Extension
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddMyServices(this IServiceCollection services, IConfiguration configuration)
        {
            var politicaUsuariosAutenticados = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            // Add services to the container.
            services.AddControllersWithViews(opciones =>
            {
                opciones.Filters.Add(new AuthorizeFilter(politicaUsuariosAutenticados));
            }).AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
            .AddDataAnnotationsLocalization(opciones =>
            {
                opciones.DataAnnotationLocalizerProvider = (_, factoria) => factoria.Create(typeof(RecursoCompartido));
            });

            services.AddDbContext<AplicationDbContext>(options =>
                options.UseSqlServer("name=DefaultConnection"));

            services.AddAuthentication().AddMicrosoftAccount(opciones =>
            {
                opciones.ClientId = configuration["MicrosoftClientId"];
                opciones.ClientSecret = configuration["MicrosoftClientSecret"];
            });
            services.AddIdentity<IdentityUser, IdentityRole>(opciones =>
            {
                opciones.SignIn.RequireConfirmedAccount = true;
            }).AddEntityFrameworkStores<AplicationDbContext>()
              .AddDefaultTokenProviders();

            services.PostConfigure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, opciones =>
            {
                opciones.LoginPath = "/Usuarios/Login";
                opciones.AccessDeniedPath = "/Usuarios/Login";
            });

            services.AddLocalization(opciones =>
            {
                opciones.ResourcesPath = "Recursos";
            });

            services.AddTransient<IServicioUsuarios, ServicioUsuarios>();
            services.AddAutoMapper(typeof(Program));

            return services;
        }
    }
}
