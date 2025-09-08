using InmobiliariaGarciaJesus.Data;
using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Repositories;
using InmobiliariaGarciaJesus.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configurar autenticación con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

// Registrar MySqlConnectionManager
builder.Services.AddSingleton<MySqlConnectionManager>();

// Registrar repositorios
builder.Services.AddScoped<IRepository<Inquilino>, InquilinoRepository>();
builder.Services.AddScoped<IRepository<Inmueble>, InmuebleRepository>();
builder.Services.AddScoped<IRepository<Contrato>, ContratoRepository>();
builder.Services.AddScoped<IRepository<Propietario>, PropietarioRepository>();
builder.Services.AddScoped<IRepository<Pago>, PagoRepository>();
builder.Services.AddScoped<IRepository<Configuracion>, ConfiguracionRepository>();
builder.Services.AddScoped<InmuebleImagenRepository>();

// Registrar repositorios concretos para controladores que los necesiten directamente
builder.Services.AddScoped<InmuebleRepository>();
builder.Services.AddScoped<ContratoRepository>();
builder.Services.AddScoped<PropietarioRepository>();
builder.Services.AddScoped<InquilinoRepository>();
builder.Services.AddScoped<PagoRepository>();
builder.Services.AddScoped<ConfiguracionRepository>();

// Registrar repositorios de autenticación
builder.Services.AddScoped<EmpleadoRepository>();
builder.Services.AddScoped<UsuarioRepository>();


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
builder.Services.AddScoped<IInmuebleImagenService, InmuebleImagenService>();

// Registrar servicios de autenticación
builder.Services.AddScoped<EmpleadoService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<DatabaseSeederService>();

// Servicio en segundo plano para actualización automática de pagos
builder.Services.AddHostedService<PaymentBackgroundService>();

var app = builder.Build();

// Seed default admin user
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeederService>();
    await seeder.SeedDefaultAdminUserAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Para servir archivos estáticos subidos dinámicamente
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
