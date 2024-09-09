using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using api.Data;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text;
using HospitalProject.Services;
using HospitalProject.UserContext;
using Serilog; // Serilog i�in gerekli namespace
using Serilog.Events; // LogLevel'ler i�in gerekli

var builder = WebApplication.CreateBuilder(args);

// 1. Serilog yap�land�rmas�
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()  // Minimum log seviyesi
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)  // Microsoft kaynakl� loglarda uyar� seviyesine ayarlad�k
    .Enrich.FromLogContext()  // Loglara ba�lam bilgisi ekle
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)  // G�nl�k log dosyalar�
    .CreateLogger();

// 2. ASP.NET Core uygulamas�na Serilog'u ekliyoruz
builder.Host.UseSerilog();

// Uygulama ba�larken loglama
Log.Information("Uygulama ba�l�yor...");

try
{
    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "HospitalProject", Version = "v1" });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "example: \" Bearer token\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                new List<string>()
            }
        });
    });

    builder.Services.AddDbContext<ApplicationDBContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
               .EnableSensitiveDataLogging()
               .LogTo(Console.WriteLine, LogLevel.Information);
    });

    builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddHostedService<SlotCreationService>();

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
        };
    });

    //authorization policies
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Uygulama ba�ar�yla ba�lat�ld���nda loglama
    Log.Information("Uygulama ba�ar�yla ba�lat�ld�.");

    app.Run();
}
catch (Exception ex)
{
    // Uygulama ba�lat�lamad�ysa hata loglama
    Log.Fatal(ex, "Uygulama ba�lat�lamad�!");
}
finally
{
    // Uygulama durdu�unda loglama
    Log.Information("Uygulama durduruluyor...");
    Log.CloseAndFlush();
}
