using Microsoft.AspNetCore.Authentication.Cookies;
using SV22T1020581.BusinessLayers;
using SV22T1020581.DataLayers.Interfaces;
using SV22T1020581.DataLayers.SQLServer;
using SV22T1020581.Models.Catalog;
using SV22T1020581.Models.DataDictionary;
using SV22T1020581.Models.Partner;
using SV22T1020581.Shop.AppCodes;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
Configuration.Initialize(connectionString);

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "TNPShop.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.None
            : CookieSecurePolicy.Always;
    });

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IGenericRepository<Category>>(_ => new CategoryRepository(connectionString));
builder.Services.AddScoped<IGenericRepository<Customer>>(_ => new CustomerRepository(connectionString));
builder.Services.AddScoped<IDataDictionaryRepository<Province>>(_ => new ProvinceRepository(connectionString));
builder.Services.AddScoped<IProductRepository>(_ => new ProductRepository(connectionString));
builder.Services.AddScoped<IOrderRepository>(_ => new OrderRepository(connectionString));

var app = builder.Build();

ApplicationContext.Configure(
    app.Services.GetRequiredService<IHttpContextAccessor>(),
    app.Services.GetRequiredService<IWebHostEnvironment>(),
    app.Configuration);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
