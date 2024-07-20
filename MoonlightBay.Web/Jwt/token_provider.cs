using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MoonlightBay.Web.Jwt
{
    public class TokenEntity //构造完成的jwt和过期时间1
    {
        public required string Access_token { get; set; }
        public required int Expires_in { get; set; }
    }
    public class TokenProvider(TokenProviderOptions options)
    {
        private readonly TokenProviderOptions _options = options; //传入jwt设置，包括签发者和受众

        //构造jwt
        public async Task<TokenEntity?> GenerateToken(HttpContext context, string userName, string password)
        {
            ClaimsIdentity? identity = await GetIdentity(userName);
            if (identity == null)
                return null;
            
            DateTime now = DateTime.UtcNow;

            //声明claim，用claim来构造jwt
            var claims = new Claim[]
            {
                //用户名
                new Claim(JwtRegisteredClaimNames.Sub, userName),
                //jwt的id
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                //签发时间
                new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(now).ToString(), ClaimValueTypes.Integer64)
            };

            //jwt的真正构造函数
            var jwt = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(_options.Expiration),
                //expires: DateTime.MaxValue,
                signingCredentials: _options.SigningCredentials);
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new TokenEntity
            {
                Access_token = encodedJwt,
                Expires_in = (int)_options.Expiration.TotalSeconds
                //Expires_in = (int)DateTime.MaxValue.Subtract(now).TotalSeconds
            };
            return response;
        }
        private static Task<ClaimsIdentity> GetIdentity(string username)
        {
            return Task.FromResult(new ClaimsIdentity(
                new System.Security.Principal.GenericIdentity(username, "Token"), 
                new Claim[] {new Claim(ClaimTypes.Name, username) }));
        }
        public static long ToUnixEpochDate(DateTime date)
            => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);

    }
}