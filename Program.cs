using InmobiliariaGarciaJesus.Data;
using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Repositories;
using InmobiliariaGarciaJesus.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Registrar MySqlConnectionManager
builder.Services.AddSingleton<MySqlConnectionManager>();

// Registrar repositorios
builder.Services.AddScoped<IRepository<Inquilino>, InquilinoRepository>();
builder.Services.AddScoped<IRepository<Inmueble>, InmuebleRepository>();
builder.Services.AddScoped<IRepository<Contrato>, ContratoRepository>();
builder.Services.AddScoped<IRepository<Propietario>, PropietarioRepository>();
builder.Services.AddScoped<IRepository<Pago>, PagoRepository>();
builder.Services.AddScoped<IRepository<Configuracion>, ConfiguracionRepository>();


// Registrar servicios de negocio
builder.Services.AddScoped<InmobiliariaGarciaJesus.Services.IContratoService>(provider =>
    new InmobiliariaGarciaJesus.Services.ContratoService(
        provider.GetRequiredService<IRepository<Contrato>>(),
        provider.GetRequiredService<IRepository<Pago>>(),
        provider.GetRequiredService<IRepository<Configuracion>>()
    ));
builder.Services.AddScoped<InmobiliariaGarciaJesus.Services.IPagoService>(provider =>
    new InmobiliariaGarciaJesus.Services.PagoService(
        provider.GetRequiredService<IRepository<Pago>>(),
        provider.GetRequiredService<IRepository<Contrato>>(),
        provider.GetRequiredService<IRepository<Configuracion>>()
    ));
builder.Services.AddScoped<InmobiliariaGarciaJesus.Services.IConfiguracionService, InmobiliariaGarciaJesus.Services.ConfiguracionService>();

// Servicio en segundo plano para actualización automática de pagos
builder.Services.AddHostedService<PaymentBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
