using System.Security.Cryptography;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.DAL.Context;

namespace PRN222.Ass2.EVDealerSys.DAL.Init;
public static class DbInitializer
{
    public static async Task SeedAsync(EvdealerDbContext context, IConfiguration config)
    {
        var email = config["AdminAccount:Email"];
        var password = config["AdminAccount:Password"];

        // Hash password first
        var hashedPassword = HashPassword(password);

        // Kiểm tra xem admin đã tồn tại chưa
        var existingAdmin = await context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (existingAdmin != null)
        {
            // Nếu admin tồn tại, cập nhật password (không xóa) để giữ nguyên Id và liên kết ActivityLog
            var needUpdate = existingAdmin.Password != hashedPassword || existingAdmin.Role != 1 || string.IsNullOrEmpty(existingAdmin.Name);
            if (needUpdate)
            {
                existingAdmin.Password = hashedPassword;
                existingAdmin.Role = 1;
                existingAdmin.Name = string.IsNullOrEmpty(existingAdmin.Name) ? "Default admin" : existingAdmin.Name;
                context.Users.Update(existingAdmin);
                await context.SaveChangesAsync();
            }
        }
        else
        {
            // Tạo admin mới với password đã hash
            context.Users.Add(new User { Email = email, Password = hashedPassword, Role = 1, Name = "Default admin" });
            await context.SaveChangesAsync();
        }
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
