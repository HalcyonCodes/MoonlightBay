using System;
using Microsoft.IdentityModel.Tokens;

namespace MoonlightBay.Web.Jwt
{
    public class TokenProviderOptions
    {
        public string Path { get; set; } = "/api/v1/Account/Login";
        public required string Issuer { get; set; } //签发者
        public required string Audience { get; set; }  //JWT受众
        //public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(500);
        public TimeSpan Expiration { get; set; } = TimeSpan.FromHours(24); //jwt时间为24小时
        //public required DateTime Expiration { get; set; } = DateTime.MaxValue;
        public required SigningCredentials SigningCredentials { get; set; }
    }
}