using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PetiversoAPI.Models;
using PetiversoAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuração do banco de dados
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar política de CORS
var corsPolicyName = "AllowFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policy =>
    {
        policy.WithOrigins("http://localhost:4200") // URL do Angular
              .AllowAnyMethod() // Permite todos os métodos (GET, POST, PUT, DELETE, etc.)
              .AllowAnyHeader() // Permite todos os headers
              .AllowCredentials(); // Permite envio de cookies ou credenciais
    });
});

// Configuração de autenticação baseada em cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.LoginPath = "/api/auth/login";
        options.LogoutPath = "/api/auth/logout";
        options.SlidingExpiration = true;
    });

// Injeção de dependências
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddSingleton<PasswordHasher>();

// Adicionar controladores
builder.Services.AddControllers();

var app = builder.Build();

// Configurações do pipeline de middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors(corsPolicyName);

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
