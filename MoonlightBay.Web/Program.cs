using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.AspNetCore.Authentication.Cookies;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
//
using MoonlightBay.Data;
//using MoonlightBay.Data.Interface;
//using MoonlightBay.Data.Repositories;
//
using MoonlightBay.Model;
//
using MoonlightBay.Web;
using MoonlightBay.Data.Interfaces;
using MoonlightBay.Data.Repositories;
//
using MoonlightBay.Web.Middleware;


var builder = WebApplication.CreateBuilder(args);



// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderServiceRepository, OrderServiceRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITerminalRepository, TerminalRepository>();




//配置数据库
builder.Services.AddDbContextPool<ApplicationDbContext>(options =>

    options.UseMySql("Server=localhost;Port=3306;Database=MoonlightBay;User=root;Password=z12312;",
        ServerVersion.Create(new Version(8, 1, 0), ServerType.MySql),
        b =>
        {
            b.MigrationsAssembly("MoonlightBay.Web");
        }
));



builder.Services.AddIdentity<ApplicationUser, IdentityRole>(
    options => {
        options.Password.RequireDigit = false; // 要求至少一个数字
    options.Password.RequiredLength = 6;  // 要求密码最少长度为 8
    options.Password.RequireLowercase = false; // 要求至少一个小写字母
    options.Password.RequireUppercase = false; // 要求至少一个大写字母
    options.Password.RequireNonAlphanumeric = false; // 要求至少一个特殊字
    }
)
//.AddRoles<ApplicationUser>()
.AddEntityFrameworkStores<ApplicationDbContext>();
//.AddDefaultTokenProviders();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => options.IdleTimeout = TimeSpan.FromMinutes(20));
builder.Services.Configure<WebApiSettings>(builder.Configuration.GetSection("WebApiSettings"));

// 从配置中获取密钥
var secretKey = builder.Configuration.GetSection("WebApiSettings")["SecretKey"];
var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey!));

// 配置 TokenValidationParameters
var tokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = signingKey,
    ValidateIssuer = true,
    ValidIssuer = "MoonlightBay",
    ValidateAudience = true,
    ValidAudience = "MoonlightBayAudience",
    ValidateLifetime = true,
    ClockSkew = TimeSpan.Zero
};

// 添加 JWT Bearer 身份验证服务
/*builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = tokenValidationParameters;
        
    });*/
builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(o => { o.TokenValidationParameters = tokenValidationParameters; });

// 添加授权服务
builder.Services.AddAuthorizationBuilder()
             // 添加授权服务
             .AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"))
             // 添加授权服务
             .AddPolicy("RequireTerminalRole", policy => policy.RequireRole("Terminal"));


//设置大小写一致
builder.Services.AddControllers().AddNewtonsoftJson(
    options => {options.SerializerSettings.ContractResolver = new DefaultContractResolver();}
);

//设置跨域策略
builder.Services.AddCors(options => {
  options.AddPolicy("corsPolicy", opt => opt.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddMvc();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

//使用静态文件
app.UseStaticFiles();

// Configure the HTTP request pipeline.
/*if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseRoleInitialization();
//app.UseMiddleware<RoleInitializationMiddleware>();
//app.UseMiddleware<RoleAuthorizeMiddleware>();

app.UseRouting();
app.UseSession();
// 添加 JWT 身份验证
app.UseAuthentication(); // 添加身份验证中间件
app.UseAuthorization(); // 添加授权中间件

app.UseEndpoints(endpoints =>
           {
               endpoints.MapControllers();
           });

app.UseHttpsRedirection();*/

//
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseRouting();
app.UseCors("corsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.UseHttpsRedirection();

app.Run();

