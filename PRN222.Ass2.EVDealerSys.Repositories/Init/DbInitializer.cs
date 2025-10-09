using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.Repositories.Context;

namespace PRN222.Ass2.EVDealerSys.Repositories.Init;
public static class DbInitializer
{
    public static async Task SeedAsync(EvdealerDbContext context, IConfiguration config)
    {
        var email = config["AdminAccount:Email"];
        var password = config["AdminAccount:Password"];

        if (!await context.Users.AnyAsync(u => u.Email == email))
        {
            context.Users.Add(new User { Email = email, Password = password, Role = 1, Name = "Default admin" });
            await context.SaveChangesAsync();
        }
    }
}
