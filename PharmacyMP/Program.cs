using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PharmacyMP.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMvc();

string connection = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<PharmacyDbContext>(options => options.UseSqlServer(connection));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).
    AddCookie(option =>
    {
        option.LoginPath = "/Authorization/Login";
        option.LogoutPath = "/Products/Index";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}");

app.Run();