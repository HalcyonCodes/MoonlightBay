using Microsoft.AspNetCore.Identity;

public class RoleInitializationMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context, RoleManager<IdentityRole> roleManager)
    {
        // 检查并初始化角色
        await EnsureRolesAsync(roleManager);

        // 继续处理请求
        await _next(context);
    }

    private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        // 这里添加检查和创建角色的逻辑
        // 例如，检查 "Admin" 角色是否存在，如果不存在则创建
        var adminRole = await roleManager.FindByNameAsync("Admin");
        if (adminRole == null)
        {
            adminRole = new IdentityRole("Admin");
            await roleManager.CreateAsync(adminRole);
        }
         var terminalRole = await roleManager.FindByNameAsync("Terminal");
        if (terminalRole == null)
        {
            terminalRole = new IdentityRole("Terminal");
            await roleManager.CreateAsync(terminalRole);
        }

        // 对其他角色执行相同的检查和创建逻辑
    }
}