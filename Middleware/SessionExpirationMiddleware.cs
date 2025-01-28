using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using PetiversoAPI.Models;
using System.Security.Claims;

namespace PetiversoAPI.Middleware
{
    public class SessionExpirationMiddleware(RequestDelegate next)
    {
        public async Task Invoke(HttpContext context)
        {
            if (context.User.Identity is { IsAuthenticated: true })
            {
                var sessionToken = context.User.FindFirst("SessionToken")?.Value;
                if (!string.IsNullOrEmpty(sessionToken))
                {
                    var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();
                    var session = await dbContext.Sessions.FirstOrDefaultAsync(s => s.SessionToken == sessionToken);

                    if (session != null)
                    {
                        var now = DateTime.UtcNow;
                        var remainingTime = session.ExpiresAt - now;

                        // Se o tempo restante for menor que metade da validade original, renova
                        if (remainingTime <= TimeSpan.FromHours(4))
                        {
                            session.ExpiresAt = now.AddHours(8); // Nova duração
                            await dbContext.SaveChangesAsync();

                            // Atualiza o cookie com o novo valor de expiração
                            var claimsIdentity = new ClaimsIdentity(context.User.Identity);
                            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, new AuthenticationProperties
                            {
                                ExpiresUtc = session.ExpiresAt,
                                IsPersistent = true
                            });
                        }
                    }
                }
            }

            await next(context);
        }
    }

}
