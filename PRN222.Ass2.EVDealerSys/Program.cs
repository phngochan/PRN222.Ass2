using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

using PRN222.Ass2.EVDealerSys.BLL.Implementations;
using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.DAL.Context;
using PRN222.Ass2.EVDealerSys.DAL.Implementations;
using PRN222.Ass2.EVDealerSys.DAL.Init;
using PRN222.Ass2.EVDealerSys.DAL.Interfaces;
using PRN222.Ass2.EVDealerSys.Hubs;
using PRN222.Ass2.EVDealerSys.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
// ========== 1. Configuration ==========
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// ========== 2. Database Context ==========
builder.Services.AddDbContext<EvdealerDbContext>(options =>
    options.UseSqlServer(connectionString));

// ========== 3. Repositories ==========
// User & Authentication
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Customer Management
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

// Vehicle & Inventory
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();

// Orders & Payments
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// Test Drive & Dealer
builder.Services.AddScoped<ITestDriveRepository, TestDriveRepository>();
builder.Services.AddScoped<IDealerRepository, DealerRepository>();

// Reports
builder.Services.AddScoped<IReportRepository, ReportRepository>();
// Activity Logs
builder.Services.AddScoped<IActivityLogRepository, ActivityLogRepository>();

// ========== 4. Services ==========
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ITestDriveService, TestDriveService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDealerService, DealerService>();
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();

// ========== Add Authen ==========
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddSignalR();

var app = builder.Build();
// ========== Seed Database ==========
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EvdealerDbContext>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    await DbInitializer.SeedAsync(context, config);
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseMiddleware<StartPageRedirectMiddleware>();

app.MapHub<OrderHub>("/orderHub");
app.MapHub<ManagementHub>("/managementHub");
app.MapHub<VehicleHub>("/vehicleHub");
app.MapHub<ActivityLogHub>("/activityLogHub");
app.MapHub<TestDriveHub>("/testDriveHub");

app.MapRazorPages();

app.Run();
