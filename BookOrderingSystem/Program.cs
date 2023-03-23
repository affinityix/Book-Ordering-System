using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BookOrderingSystem.DataAccess.Data;
using BookOrderingSystem.DataAccess.Repositories.Interfaces;
using BookOrderingSystem.DataAccess.Repositories;
using BookOrderingSystem.DataAccess.Services;
using BookOrderingSystem.Utility.Details;
using Microsoft.AspNetCore.Identity.UI.Services;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

var configuration = builder.Configuration;

var connectionString = configuration.GetConnectionString("DefaultConnection");

services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
services.AddRazorPages();

services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();

services.Configure<StripeSetting>(configuration.GetSection("Stripe"));

services.AddControllersWithViews();

services.AddTransient<IEmailSender, EmailSender>();

services.AddTransient<IUnitOfWork, UnitOfWork>();

services.ConfigureApplicationCookie(options =>
{
	options.LogoutPath = $"/Identity/Account/Logout";
	options.LoginPath = $"/Identity/Account/Login";
	options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

StripeConfiguration.ApiKey = configuration.GetSection("Stripe:SecretKey").Get<string>();

app.UseAuthentication();;

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{Area=User}/{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
