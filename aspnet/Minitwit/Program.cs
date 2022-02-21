using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Minitwit.Controllers;
using Minitwit.DatabaseUtil;
using Minitwit.Models.Context;
using Minitwit.Models.DTO;
using Minitwit.Models.Entity;
using Minitwit.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<MinitwitContext>();

builder.Services.AddDbContext<MinitwitContext>(
    optionsAction: options => { options.UseNpgsql(builder.Configuration.GetConnectionString("Minitwit")); });

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
});

builder.Services.AddScoped<IEntityAccessor, EntityAccessor>();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.MaxFailedAccessAttempts = 1000;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 1;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyz���ABCDEFGHIJKLMNOPQRSTUVWXYZ���0123456789-._@+ ";
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Disabled for testing purposes
// app.UseHttpsRedirection();
app.UseStaticFiles();


app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

//Preload data model (to speed up the first few requests)
var thing = builder.Services.BuildServiceProvider().GetService<MinitwitContext>();
thing.Follows.FirstOrDefaultAsync();
thing.Users.FirstOrDefaultAsync();
thing.Posts.FirstOrDefaultAsync();
thing.Latest.FirstOrDefaultAsync();

app.Run();
