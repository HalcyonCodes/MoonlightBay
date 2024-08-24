//using System;
using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Components;
using MoonlightBay.Data.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MoonlightBay.Model;
//using MoonlightBay.Web.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MoonlightBay.Web.Jwt;
using System.Text;
//using Microsoft.AspNetCore.Http.HttpResults;
using MoonlightBay.Web.Utils;
using MoonlightBay.Web.Models;
using MoonlightBay.Data.Repositories;
using Microsoft.VisualBasic;
using System.Security.Claims;


namespace MoonlightBay.Web.Controllers;

/// <summary>
/// 账户控制器
/// </summary>

[Route("api/v1/[controller]/[action]")]
public class AccountController(
    ILoggerFactory loggerFactory,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IOptions<WebApiSettings> settings,
    IHttpContextAccessor httpContextAccessor,
    IAccountRepository accountRepository

    ) : Controller
{

    private readonly ILogger _logger = loggerFactory.CreateLogger<AccountController>();
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    readonly IOptions<WebApiSettings> _settings = settings;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IAccountRepository _accountRepository = accountRepository;

    // POST: /Account/Register
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterApiModel registerModel)
    {
        //在identity里创建账户
        //IdentityRole? userAdmin = await _roleManager.FindByNameAsync("Admin");
        //if(userAdmin != null && registerModel.Role == "Admin"){
        //return BadRequest("regist admin failed");
        //}

        if(registerModel.Role != "Admin" && registerModel.Role != "Terminal"){
            return BadRequest ("regist account failed.");
        };

        var user = new ApplicationUser { UserName = registerModel.UserName, Role = registerModel.Role};
        var result = await _userManager.CreateAsync(user, registerModel.Password);
 
        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: true);
            _logger.LogInformation(3, "User created a new account with password.");
            return Ok();
        }
        // If we got this far, something failed.
        return ResponseHelper.Unauthorized();
    }

    // POST: /Account/Login
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginAPIModel loginModel)
    {
        //先在identity里验证账户
        Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(
                loginModel.UserName, loginModel.Password, true, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            _logger.LogInformation(1, "User logged in.");

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Value.SecretKey));
            var options = new TokenProviderOptions
            {
                Audience = "MoonlightBayAudience",
                Issuer = "MoonlightBay",
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
            };

            //用账户的账户名和密码来创建jwt
            TokenProvider tpm = new(options);
            TokenEntity? token = await tpm.GenerateToken(HttpContext, loginModel.UserName, loginModel.Password);

            //送出jwt，放到app让其他app的请求头带有jwt
            if (null != token)
                return new JsonResult(token);
            else
                return NotFound();
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning(2, "User account locked out.");
            return Ok("Lockout");
        }
        else
        {
            _logger.LogWarning(2, "Invalid login attempt.");
            return Ok("Invalid login attempt.");
        }
    }

    // POST: /Account/LogOff
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> LogOff()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation(4, "User logged out.");
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> Test()
    {
        var requestHeaders = _httpContextAccessor.HttpContext.Request.Headers;
        var requestBody = _httpContextAccessor.HttpContext.Request.Body;
        ApplicationUser? user = await _userManager.GetUserAsync(HttpContext.User);

        return Ok("asdasd");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> DDTest()
    {
        var requestHeaders = _httpContextAccessor.HttpContext.Request.Headers;
        var requestBody = _httpContextAccessor.HttpContext.Request.Body;

        //===
        var currentUser = _httpContextAccessor.HttpContext.User;

        // 从 Claims 中获取用户 ID
        string? userId = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);
        //ApplicationUser? user = await _userManager.FindByIdAsync(userId!);

        
        return Ok("asdasd");
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> PostTest(){
        return Ok();
    }

}

