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


namespace MoonlightBay.Web.Controllers;

/// <summary>
/// 订单控制器
/// </summary>

[Route("api/v1/[controller]/[action]")]
public class OrderController(
    ILoggerFactory loggerFactory
    ) : Controller
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<AccountController>();
    

}