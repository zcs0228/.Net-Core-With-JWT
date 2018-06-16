using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

public class TokenGenerator
{
    TokenGenerateOption _Option;

    public IUserValidate UserValidator { get; private set; }
    public TokenGenerator(TokenGenerateOption option, IUserValidate validator)
    {
        _Option = option;
        UserValidator = validator;
    }

    async Task BadRequest(HttpContext context, string msg)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync(msg);
    }

    internal void GenerateToken(IApplicationBuilder app)
    {
        app.Run(async context =>
        {
            if (!context.Request.Method.Equals("POST") || !context.Request.HasFormContentType)
            {
                await BadRequest(context,"format not corrent");
                return;
            }

            var username = context.Request.Form["username"];
            var password = context.Request.Form["password"];

            var userModel = UserValidator?.GetUserByContext(username, password);
            if (userModel == null)
            {
                await BadRequest(context, "Invalid username or password.");
                return;
            }

            var now = DateTime.UtcNow;
            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userModel.UniqueId),
                new Claim(JwtRegisteredClaimNames.UniqueName,userModel.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, now.ToString(), ClaimValueTypes.Integer64)
            };

            var jwt = new JwtSecurityToken(_Option.Issuer, _Option.Audience, claims, now, 
                now.Add(_Option.Expiration), _Option.SigningCredentials);
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            //var Subject = new ClaimsIdentity(claims);
            //var encodedJwt = new JwtSecurityTokenHandler().CreateEncodedJwt(_Option.Issuer, _Option.Audience, Subject, now,
            //    now.Add(_Option.Expiration), now, _Option.SigningCredentials);

            var response = new
            {
                access_token = encodedJwt,
                expires_in = (int)_Option.Expiration.TotalSeconds,
            };

            // Serialize and return the response
            context.Response.ContentType = "application/json";
            string responseStr = JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented });
            await context.Response.WriteAsync(responseStr);
        });
    }

}