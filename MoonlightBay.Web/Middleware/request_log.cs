
namespace MoonlightBay.Web.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 仅记录POST请求
        if (context.Request.Method == "POST")
        {
            // 读取请求体
            var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
            // 重置请求体流的位置，以便后续中间件或控制器可以再次读取
            context.Request.Body.Position = 0;

            // 记录原始请求体
            Console.WriteLine("Request body: " + requestBody);
        }

        // 调用管道中的下一个中间件组件
        await _next(context);
    }
}