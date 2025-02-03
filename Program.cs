using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PetiversoAPI.Middleware;
using PetiversoAPI.Models;
using PetiversoAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Adiciona servi�os do Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Petiverso API", Version = "v1" });

    // Definir autentica��o baseada em Cookie
    options.AddSecurityDefinition("Session", new OpenApiSecurityScheme
    {
        Name = "Cookie",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Cookie,
        Description = "Session authentication via HttpOnly Cookie"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Session"
                }
            },
            new List<string>()
        }
    });
});

// Configura��o do banco de dados
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar pol�tica de CORS
var corsPolicyName = "AllowFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policy =>
    {
        policy.WithOrigins("http://localhost:4200") // URL do Angular
              .AllowAnyMethod() // Permite todos os m�todos (GET, POST, PUT, DELETE, etc.)
              .AllowAnyHeader() // Permite todos os headers
              .AllowCredentials(); // Permite envio de cookies ou credenciais
    });
});

// Configura��o de autentica��o baseada em cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.LoginPath = "/api/auth/login";
        options.LogoutPath = "/api/auth/logout";
        options.SlidingExpiration = true;
        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = async context =>
            {
                var sessionToken = context.Principal?.FindFirst("SessionToken")?.Value;
                if (sessionToken == null) return;

                using var scope = context.HttpContext.RequestServices.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var session = await dbContext.Sessions.FirstOrDefaultAsync(s => s.SessionToken == sessionToken);
                if (session == null || session.ExpiresAt < DateTime.UtcNow)
                {
                    // Sess�o inv�lida ou expirada, logout autom�tico
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }
                else
                {
                    // Renova a validade da sess�o
                    session.ExpiresAt = DateTime.UtcNow.AddHours(8);
                    await dbContext.SaveChangesAsync();
                }
            }
        };
    });

// Inje��o de depend�ncias
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddSingleton<PasswordHasher>();

// Adicionar controladores
builder.Services.AddControllers();

var app = builder.Build();

// Configura��es do pipeline de middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors(corsPolicyName);

app.UseAuthentication();
app.UseMiddleware<SessionExpirationMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
