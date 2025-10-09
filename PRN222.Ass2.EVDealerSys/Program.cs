using Microsoft.EntityFrameworkCore;
using PRN222.Ass2.EVDealerSys.Repositories.Context;
using PRN222.Ass2.EVDealerSys.Repositories.Interfaces;
using PRN222.Ass2.EVDealerSys.Repositories.Implementations;
using PRN222.Ass2.EVDealerSys.Repositories.Init;

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

app.UseAuthorization();

app.MapRazorPages();

app.Run();
