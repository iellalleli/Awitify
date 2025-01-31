using Microsoft.EntityFrameworkCore;
using KaraokeApp.Data;
using KaraokeApp.Repositories;
using KaraokeApp.Services;
using KaraokeApp.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSession();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(typeof(AuthenticationFilter));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSession();
// Add this after app.UseSession();
app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache"; 
    context.Response.Headers["Expires"] = "0";
    
    // Prevent browser back after logout
    if (context.Request.Path.StartsWithSegments("/Account/Login") && 
        context.Session.GetInt32("UserId") != null)
    {
        context.Response.Redirect("/Home/HomePage");
        return;
    }
    
    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=LandingPage}/{action=Home}/{id?}");

app.Run();