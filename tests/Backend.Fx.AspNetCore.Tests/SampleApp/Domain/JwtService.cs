using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Fx.AspNetCore.Tests.SampleApp.Domain
{
    public class JwtService
    {
        private static readonly JwtSecurityTokenHandler JwtSecurityTokenHandler = new();

        public static readonly SigningCredentials SigningCredentials =
            new(new SymmetricSecurityKey(Guid.NewGuid().ToByteArray()), SecurityAlgorithms.HmacSha256);

        public static string IssueJwt(string identityName)
        {
            var jwt = new JwtSecurityToken(
                "SampleApp",
                "SampleApp",
                new[] { new Claim(ClaimTypes.Name, identityName) },
                DateTime.UtcNow,
                DateTime.UtcNow.AddMinutes(5),
                SigningCredentials);

            string jwtString = JwtSecurityTokenHandler.WriteToken(jwt);
            return jwtString;
        }

        public static TokenValidationParameters TokenValidationParameters()
        {
            return new TokenValidationParameters
            {
                NameClaimTypeRetriever = (token, s) => ClaimTypes.Name,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = SigningCredentials.Key,
                ValidateIssuer = true,
                ValidIssuer = "SampleApp",
                ValidateAudience = true,
                ValidAudience = "SampleApp",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2)
            };
        }
    }
}
