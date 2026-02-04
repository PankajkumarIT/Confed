using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
namespace API.Resources
{
    public static class AuthenticationExtensions
    {
        public static void ConfigureJwtAuthentication(this IServiceCollection services, byte[] key)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.ContainsKey("AuthToken"))
                        {
                            context.Token = context.Request.Cookies["AuthToken"];
                        }
                        //else if (context.Request.Headers.ContainsKey("Authorization"))
                        //{
                        //    var authHeader = context.Request.Headers["Authorization"].ToString();
                        //    if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        //    {
                        //        context.Token = authHeader.Substring("Bearer ".Length).Trim();
                        //    }
                        //}

                        return Task.CompletedTask;
                    }
                };

                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
        }

        public static void ConfigureCustomAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(SD.IsAccess, x =>
                {
                    x.RequireAuthenticatedUser();
                    x.AddRequirements(new IsAccssRequirement());
                });
            });
        }

    }
}
