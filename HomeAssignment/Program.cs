using HomeAssignment.Data;
using HomeAssignment.Factories;
using HomeAssignment.Filters;
using HomeAssignment.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddMemoryCache();



builder.Services.AddKeyedScoped<IItemsRepository, ItemsInMemoryRepository>("memory");
builder.Services.AddKeyedScoped<IItemsRepository, ItemsDbRepository>("db");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ItemsInMemoryRepository>();
builder.Services.AddScoped<ItemsDbRepository>();

builder.Services.AddSingleton<ImportItemFactory>();

builder.Services.AddScoped<ApprovalAuthorizationFilter>();


var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();           
app.UseAuthorization();

// Map routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
