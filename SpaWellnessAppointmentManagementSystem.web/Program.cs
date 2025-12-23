
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Optional (dev convenience): hot reload of Razor views
// builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseStatusCodePages(); // optional: simple pages for 404/500 in dev
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// ✅ Serves static files from wwwroot
app.UseStaticFiles();

// ✅ Enables endpoint routing
app.UseRouting();

// If you add authentication later, enable it before authorization:
// app.UseAuthentication();
app.UseAuthorization();

// ✅ Conventional MVC routing: default goes to Home/Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// If also using Razor Pages (not necessary for pure MVC):
// app.MapRazorPages();

app.Run();




