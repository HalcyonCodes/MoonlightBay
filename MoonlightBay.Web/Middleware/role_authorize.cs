


using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MoonlightBay.Data.Interfaces;
using MoonlightBay.Model;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MoonlightBay.Web.Middleware
{
    public class RoleAuthorizeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserRepository _userRepository;

        public RoleAuthorizeMiddleware(
            RequestDelegate next,
            IServiceScopeFactory scopeFactory,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager,
            IUserRepository userRepository)
        {
            _next = next;
            _scopeFactory = scopeFactory;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _userRepository = userRepository;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            // 检查是否为POST请求
            if (context.Request.Method == "POST")
            {
                // 确保请求体被缓冲
                context.Request.EnableBuffering();

                // 读取请求体的内容
                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8))
                {
                    var requestBody = await reader.ReadToEndAsync();

                    // 解析JSON
                    var json = JsonConvert.DeserializeObject<dynamic>(requestBody);
                    if (json == null) return;

                    // 获取 role 字段的值
                    var role = json?.role?.ToString();

                    // 检查 role 是否为 "Admin"
                    if (!string.IsNullOrEmpty(role) && role == "Admin")
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            bool? hasAdmin = await _userRepository.IsHasAdminAsync();
                            
                            if (hasAdmin != true)
                            {
                                await _next(context);
                            }
                            else
                            {
                                context.Response.StatusCode = 403;
                                await context.Response.WriteAsync("未授权");
                            }
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = 403;
                        await context.Response.WriteAsync("未授权");
                    }

                    // 重新定位请求体流的位置
                    context.Request.Body.Position = 0;
                }
            }

            await _next(context);
        }
    }

    public static class RoleAuthorizeMiddlewareExtensions
    {
        public static IApplicationBuilder UseRoleAuthorization(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RoleAuthorizeMiddleware>();
        }
    }
}


