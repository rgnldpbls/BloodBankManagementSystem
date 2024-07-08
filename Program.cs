using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BBMS.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using BBMS.Services;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<BloodBankDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BloodBankDBContext") ?? throw new InvalidOperationException("Connection string 'BloodBankDBContext' not found.")));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<InventoryService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/Home/Login";
    options.LogoutPath = "/Admin/Logout";
    options.AccessDeniedPath = "/Admin/AccessDenied";
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DonorOnly", policy => policy.RequireRole("Donor"));
    options.AddPolicy("PhysicianOnly", policy => policy.RequireRole("Physician"));
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == 200 && context.User.Identity.IsAuthenticated)
    {
        context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        context.Response.Headers["Pragma"] = "no-cache";
        context.Response.Headers["Expires"] = "0";
    }
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
