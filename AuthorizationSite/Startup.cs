using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace zcsdotnet
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IUserValidate,UserValidate>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
 
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
 
             string secretKey = "encrypt_the_validate_site_key";
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var options = new TokenGenerateOption
            {
                 Path = "/token",
                 Audience = "http://validateSite.woailibian.com",
                 Issuer = "http://thisSite.woailibian.com",
                 SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
                 Expiration = TimeSpan.FromMinutes(15),
             };
            var userValidate = app.ApplicationServices.GetService<IUserValidate>();
             // var userValidate = new UserValidate();
 
             var tokenGenerator = new TokenGenerator(options, userValidate);
             app.Map(options.Path, tokenGenerator.GenerateToken);
 
             app.Run(async (context) =>
             {
                 await context.Response.WriteAsync("This Service only use for authentication! ");
             });
        }
    }
}
