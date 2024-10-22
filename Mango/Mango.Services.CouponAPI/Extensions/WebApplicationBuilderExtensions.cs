using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Mango.Services.CouponAPI.Extensions
{
    /// <summary>
    /// We extended the Web application builder to a
    /// different folder because the coupon API
    /// program.cs was getting too cluttered with the code
    /// </summary>
    public static class WebApplicationBuilderExtensions
    {
        public static WebApplicationBuilder AddAppAuthentication(this WebApplicationBuilder builder)
        {
            //adding the authentication to the DI pipeline
            var apiConfig = builder.Configuration.GetSection("ApiSettings");

            var secret = apiConfig.GetValue<string>("Secret");
            var issuer = apiConfig.GetValue<string>("Issuer");
            var audience = apiConfig.GetValue<string>("Audience");

            var key = Encoding.ASCII.GetBytes(secret);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true, // Ensures token hasn't expired
                    ClockSkew = TimeSpan.Zero  // Optional: removes the default 5 minutes clock skew
                };
            });
            return builder;
        }
    }
}
