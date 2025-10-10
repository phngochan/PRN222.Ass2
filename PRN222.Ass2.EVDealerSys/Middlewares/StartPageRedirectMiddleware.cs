using System.Security.Claims;

namespace PRN222.Ass2.EVDealerSys.Middlewares;
public class StartPageRedirectMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path == "/")
        {
            if (context.User.Identity?.IsAuthenticated ?? false)
            {
                var roleClaim = context.User.FindFirst(ClaimTypes.Role)?.Value;

                switch (roleClaim)
                {
                    case "1":
                        context.Response.Redirect("/Dashboard/Admin");
                        return;

                    case "2":
                        context.Response.Redirect("/Dashboard/Manager");
                        return;

                    case "3":
                        context.Response.Redirect("/Dashboard/Staff");
                        return;

                    default:
                        context.Response.Redirect("/Account/Login");
                        return;
                }
            }
            else
            {
                context.Response.Redirect("/Account/Login");
                return;
            }
        }

        await _next(context);
    }
}

