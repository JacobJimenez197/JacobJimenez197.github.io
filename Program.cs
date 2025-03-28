using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PlataformaAPI.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

ConfigureFirebase(builder);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
ConfigureSwagger(builder);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
    o => o.EnableRetryOnFailure()));

builder.Services.AddIdentity<User, Role>(options => {
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

ConfigureJwtAuthentication(builder);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

app.Use(async (context, next) => {
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR: {ex}");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Error interno del servidor");
    }
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection(); 
app.MapControllers();
InitializeDatabase(app);
app.UseCors();
app.Run();

void ConfigureFirebase(WebApplicationBuilder builder)
{
    try
    {
        var credential = GoogleCredential.FromFile("firebase-config.json");
        FirebaseApp.Create(new AppOptions()
        {
            Credential = credential,
            ProjectId = "plataformalaboratorio-77da5"
        });
        Console.WriteLine("Firebase configurado correctamente");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR Firebase: {ex}");
    }
}

void ConfigureSwagger(WebApplicationBuilder builder)
{
    builder.Services.AddSwaggerGen(c => {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Plataforma API", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer"
        });
    });
}

void ConfigureJwtAuthentication(WebApplicationBuilder builder)
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
            ValidAudience = builder.Configuration["JWT:ValidAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
        };
    });
}

void InitializeDatabase(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        Console.WriteLine("Base de datos inicializada");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR BD: {ex}");
    }
}