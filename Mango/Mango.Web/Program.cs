using Mango.Web.Service;
using Mango.Web.Service.IService;
using Mango.Web.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//adding the httpclient to the DI pipeline
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<ICouponService, CouponService>();

//assigns the couponAPI base URL
StaticDetails.ApibaseURL = builder.Configuration["ServiceUrls:CouponAPI"];

//adding the ICouponService and IBaseService interface to the DI pipeline
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IBaseService, BaseService>();

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
