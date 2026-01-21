using SpaWellnessAppointmentManagementSystem.Services;
using SpaWellnessAppointmentManagementSystem.Repo.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. ADD SERVICES TO THE CONTAINER
builder.Services.AddControllersWithViews();

// Essential for Session and HttpContextAccessor
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".SerenitySpa.Session"; // Good practice to name your cookie
});

// Dependency Injection: Service Registration
builder.Services.AddScoped<IAuthService, AuthService>();

// Database Configuration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// 2. CONFIGURE THE HTTP REQUEST PIPELINE
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// THE SECURITY TRIO (Order is critical)
app.UseSession(); // Must be before Authentication/Authorization
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();