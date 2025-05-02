using Microsoft.AspNetCore.Localization;
using System.Globalization;
using TareasMVC.Services.Extension;
using TareasMVC.Servicios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMyServices(builder.Configuration);

var app = builder.Build();

app.UseRequestLocalization(opciones =>
{
    opciones.DefaultRequestCulture = new RequestCulture("es");  // ESTA ES LA CULTURA POR DEFECTO.
    opciones.SupportedUICultures = Constantes.CulturasUISoportadas.Select(cultura => new CultureInfo(cultura.Value)).ToList();
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
