using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Repositories;
using InmobiliariaGarciaJesus.Services;
using InmobiliariaGarciaJesus.Middleware;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configurar sesiones
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Reducir a 30 minutos
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    // Agregar identificador único del servidor para invalidar cookies al reiniciar
    options.Cookie.Name = $"InmobiliariaSession_{Environment.ProcessId}_{DateTime.Now.Ticks}";
});

// Configurar autenticación con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Reducir a 30 minutos
        options.SlidingExpiration = false; // Desactivar extensión automática
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        // Agregar identificador único del servidor para invalidar cookies al reiniciar
        options.Cookie.Name = $"InmobiliariaAuth_{Environment.ProcessId}_{DateTime.Now.Ticks}";
    });


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
builder.Services.AddScoped<ProfilePhotoService>();

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

app.UseSession(); // Habilitar sesiones
app.UseAuthentication();

// Middleware personalizado para validar consistencia de sesión
app.UseMiddleware<SessionValidationMiddleware>();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
