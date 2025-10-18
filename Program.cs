using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Repositories;
using InmobiliariaGarciaJesus.Services;
using InmobiliariaGarciaJesus.Middleware;
using InmobiliariaGarciaJesus.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // Configurar serialización JSON en camelCase (por defecto)
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Configurar DbContext con MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<InmobiliariaDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

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

// Configurar autenticación dual: Cookies para Web + JWT para API
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey no configurada");

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = false;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.Name = $"InmobiliariaAuth_{Environment.ProcessId}_{DateTime.Now.Ticks}";
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.RequireHttpsMetadata = false; // En producción cambiar a true
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
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
builder.Services.AddScoped<EmpleadoRepository>();
builder.Services.AddScoped<InmuebleImagenRepository>();
builder.Services.AddScoped<TipoInmuebleRepository>();
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
builder.Services.AddScoped<JwtService>();

// Servicio en segundo plano para actualización automática de pagos
builder.Services.AddHostedService<PaymentBackgroundService>();

// Configurar CORS para la aplicación móvil
builder.Services.AddCors(options =>
{
    options.AddPolicy("MobileAppPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configurar Swagger para documentación de la API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Inmobiliaria García Jesús - API Móvil",
        Version = "v1",
        Description = "API REST para la aplicación móvil de propietarios. " +
                      "Permite a los propietarios gestionar sus inmuebles, ver contratos y pagos.",
        Contact = new OpenApiContact
        {
            Name = "García Jesús Emanuel",
            Email = "contacto@inmobiliariagarcia.com"
        }
    });

    // Configurar autenticación JWT en Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Autenticación JWT usando el esquema Bearer. " +
                      "Ingrese 'Bearer' [espacio] y luego su token en el campo de texto.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

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

// Configurar Swagger (solo en desarrollo)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Inmobiliaria API v1");
        options.RoutePrefix = "api/docs"; // Acceder en /api/docs
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Para servir archivos estáticos subidos dinámicamente
app.UseRouting();

// Habilitar CORS
app.UseCors("MobileAppPolicy");

app.UseSession(); // Habilitar sesiones
app.UseAuthentication();

// Middleware personalizado para validar consistencia de sesión
app.UseMiddleware<SessionValidationMiddleware>();

// Middleware para forzar cambio de contraseña si RequiereCambioClave = true
app.UseRequirePasswordChange();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
