using Mango.Services.AuthAPI.Data;
using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.Service;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//added the AppDbContext DI to the container
builder.Services.AddDbContext<ApplicationDbContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//adding the JWT tokenization to the DI pipeline using the config set in the AppSettings
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("ApiSettings:JwtOptions"));

//set up Identity services for user authentication and authorization
//AddIdentity<IdentityUser, IdentityRole>() : allows you to manage user accounts, passwords, roles, and other identity-related tasks.
//AddEntityFrameworkStores<ApplicationDbContext>() : configures Identity to use Entity Framework Core for storing user and role information in a database
//AddDefaultTokenProviders() Adds the default token providers for generating security tokens:
// - Email confirmation tokens
// - Password reset tokens
// - Two-factor authentication tokens
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

//adding the Auth, Jwt generator interface to the DI pipeline
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtGenerator, JwtGenerator>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//in the pipeline always keep the authentication part before the authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

//this method is used to check for any pending migrations and execute them
ApplyPendingMigrations();

app.Run();

void ApplyPendingMigrations()
{
    using (var scope = app.Services.CreateScope())
    {
        var _db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        //this line can be used to check for the initial setup
        //where if the db is not present then the db is first created
        //and then the table structure is added 

        //But this line DOES NOT ADD THE MIGRATIONS FOLDER
        //IF YOU ARE RUNNING THE SCRIPT FOR THE FIRST TIME
        _db.Database.EnsureCreated();

        if (_db.Database.GetPendingMigrations().Any())
        {
            _db.Database.Migrate();
        }
    }
}