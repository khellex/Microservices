using Mango.Web.Service;
using Mango.Web.Service.IService;
using Mango.Web.Utilities;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// registering the cookie authentication in the pipeline
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(10); //this property is different from the CookieOption.Expires property in the TokenProvider.cs
    });
//adding the httpclient, httpcontextaccessor to the DI pipeline
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<ICouponService, CouponService>();
builder.Services.AddHttpClient<IAuthService, AuthService>();
builder.Services.AddHttpClient<IProductService, ProductService>();

//assigns the couponAPI, AuthApi, ProdcutAPI base URL
var urlConfig = builder.Configuration.GetSection("ServiceUrls");

StaticDetails.CouponApiBaseURL = urlConfig.GetValue<string>("CouponAPI");
StaticDetails.AuthApiBaseURL = urlConfig.GetValue<string>("AuthAPI");
StaticDetails.ProductApiBaseURL = urlConfig.GetValue<string>("ProductAPI");

//adding the ICouponService,IAuthService,ITokenProvider
//and IBaseService interface to the DI pipeline
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBaseService, BaseService>();
builder.Services.AddScoped<ITokenProvider, TokenProvider>();
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
