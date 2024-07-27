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
using Microsoft.AspNetCore.Authorization;
using System.Linq;


namespace MoonlightBay.Web.Controllers;

/// <summary>
/// 订单控制器
/// </summary>

[Route("api/v1/[controller]/[action]")]
public class OrderController(
    ILoggerFactory loggerFactory,
    UserManager<ApplicationUser> userManager,
    IOrderRepository orderRepository,
    ITerminalRepository terminalRepository
    ) : Controller
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<AccountController>();
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IOrderRepository _ordereRepository = orderRepository;
    private readonly ITerminalRepository _terminalRepository = terminalRepository;

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateOrder(OrderChannelViewModel viewModel){
        
        List<Terminal>? terminal = await _terminalRepository.GetUserTerminalsAsync();
        if ( terminal == null ) return BadRequest("create order failed.");
  
        if( terminal.SelectMany(t => t.OrderChannels ??= []).Select(r => r.OrderChannelID).Contains(viewModel.orderChannelID) == false){
            return BadRequest("create order failed.");
        }
        viewModel.orders ??= [];
        bool flag = false;
        foreach(var s in viewModel.orders){

        Order newOrder = new(){
            OrderID = Guid.NewGuid(),
            CreatedTime = DateTime.Now,
            Status = Order.OrderStatus.waiting,
            OrderIndex = null
        };
        Terminal? sourceTerminal = new(){
            TerminalID = s.sourceTerminalID,
        };
        Terminal? targetTerminal = terminal.FirstOrDefault();

        if(s.orderService == null) return BadRequest("create order failed.");
        OrderService? orderService = new(){
            OrderServiceID = s.orderService!.orderServiceID,
        };

        s.orderServiceResources ??= [];
        newOrder.OrderResources ??= [];

        foreach(var q in s.orderServiceResources){
            if(q.orderServiceResource == null){
                flag = true;
                break;
            }
            OrderServiceResource classResource = new(){
                OrderServiceResourceID = q.orderServiceResource.orderServiceResourceID,
            };
            OrderServiceResourceClass newResourceClass = new(){
                OrderServiceResoourceClassID = Guid.NewGuid(),
                CreatedTime = DateTime.Now,
                OrderServiceResource = classResource,
                ResourceIntValue = q.resourceIntValue,
                ResourceDoubleValue = q.resourceDoubleValue,
                ResourceStringValue = q.resourceStringValue,
            };
            newOrder.OrderResources.Add(newResourceClass);
        }
        if(flag == true) break;
        newOrder.OrderService = orderService;
        newOrder.SourceTerminal = sourceTerminal;
        newOrder.TargetTerminal = targetTerminal;
        Guid? status = await _ordereRepository.AddOrderAsync(newOrder);

        if(status == null){
            flag = true;
            break;
        }
        }
        if(flag == true) return BadRequest("created order failed.");

        return Ok();    
    }


    [HttpPost]
    [Authorize]
    public async Task<IActionResult> PopOrder(){
        Order? order = await _ordereRepository.PopOrderAsync();
        OrderViewModel 
    }


    

}




