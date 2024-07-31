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
    public async Task<IActionResult> GetOrder(){

        Order? order = await _ordereRepository.GetOrderAsync();
        if(order == null) return BadRequest("get order failed.");
        OrderResultViewModel resultViewModel = new(){
            code = "200",
            message = "",
            order = null,
        };
        if(order.OrderService == null) return BadRequest("get order failed.");

        OrderServiceViewModel serviceViewModel = new(){
            orderServiceID = order.OrderService.OrderServiceID,
            orderServiceDesc = order.OrderService.OrderServiceDesc,
            orderServiceName = order.OrderService.OrderServiceName,
            orderServiceResources = []
        };

        order.OrderService.OrderServiceResources ??= [];

        foreach(var q in order.OrderService.OrderServiceResources){
            OrderServiceResourceViewModel newResourceView = new(){
                orderServiceResourceID = q.OrderServiceResourceID,
                orderServiceResourceName = q.OrderServiceResourceName,
                orderServiceResourceDesc = q.OrderServiceResourceDesc,
            };
            serviceViewModel.orderServiceResources.Add(newResourceView);
        };

        order.OrderResources ??= [];
        if(order.SourceTerminal == null) return BadRequest("get order failed.");
        if(order.TargetTerminal == null) return BadRequest("get order failed.");

        OrderViewModel orderView = new(){
            orderID = order.OrderID,
            orderService = serviceViewModel,
            createdTime = DateTime.Now,
            sourceTerminalID = order.SourceTerminal.TerminalID,
            targetTerminalID = order.TargetTerminal.TerminalID,
            status = (OrderViewModel.OrderStatus?)order.Status,
            orderServiceResources = [],
        };

        foreach(var q in order.OrderResources){
            OrderServiceResourceClassesViewModel classViewModel = new(){
                orderServiceResourceClasssID = q.OrderServiceResoourceClassID,
                orderServiceResource = serviceViewModel.orderServiceResources
                .FirstOrDefault(t => t.orderServiceResourceID == q.OrderServiceResource!.OrderServiceResourceID),
                createdTime = q.CreatedTime,
                resourceIntValue = q.ResourceIntValue,
                resourceStringValue = q.ResourceStringValue,
                resourceDoubleValue = q.ResourceDoubleValue,
            };
            orderView.orderServiceResources.Add(classViewModel);
        }
        resultViewModel.order = orderView;

        return Ok(resultViewModel);
        
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> GetOrderChannels(){
        List<OrderChannel>? orderChannels = await _ordereRepository.GetOrderChannelsAsync();
        if(orderChannels == null) return BadRequest("Get order channels failed.");
        orderChannels ??= [];

        bool flag = false;
        foreach(var q in orderChannels){
            OrderChannelViewModel orderChannel = new(){
                orderChannelID = q.OrderChannelID,
                orderChannelLevel = q.OrderChannelLevel,
                orders = [],
            };

            q.Orders ??= [];


            foreach (var s in q.Orders!)
            {
                if(s.TargetTerminal == null || s.TargetTerminal.TerminalID == null){
                    flag = true;
                    break;
                }
                if(s.SourceTerminal == null || s.SourceTerminal.TerminalID == null){
                    flag = true;
                    break;
                }
                OrderServiceViewModel orderServiceViewModel = new(){
                    orderServiceID = s.OrderService!.OrderServiceID,
                    orderServiceName = s.OrderService!.OrderServiceName,
                    orderServiceDesc = s.OrderService!.OrderServiceDesc,
                    orderServiceResources = [],
                };
                s.OrderService.OrderServiceResources ??= [];
                foreach(var c in s.OrderService.OrderServiceResources){
                    OrderServiceResourceViewModel resourceView = new(){
                        orderServiceResourceID = c.OrderServiceResourceID,
                        orderServiceResourceName = c.OrderServiceResourceName,
                        orderServiceResourceDesc = c.OrderServiceResourceDesc,
                    };
                    orderServiceViewModel.orderServiceResources.Add(resourceView);
                }
                OrderViewModel orderView = new()
                {
                    orderID = s.OrderID,
                    orderService = orderServiceViewModel,
                    createdTime = DateTime.Now,
                    sourceTerminalID = s.SourceTerminal.TerminalID,
                    targetTerminalID = s.TargetTerminal.TerminalID,
                    status = (OrderViewModel.OrderStatus?)s.Status,
                    orderServiceResources = [],
                };
                s.OrderResources ??= [];
                foreach(var v in s.OrderResources){
                    OrderServiceResourceClassesViewModel viewModel = new(){
                        orderServiceResourceClasssID = v.OrderServiceResoourceClassID,
                        createdTime = v.CreatedTime,
                        resourceIntValue = v.ResourceIntValue,
                        resourceDoubleValue = v.ResourceDoubleValue,
                        resourceStringValue = v.ResourceStringValue,
                    };
                    if(v.OrderServiceResource == null) {
                        flag = true;
                        break;
                    };
                    OrderServiceResourceViewModel serviceViewModel = new(){
                         orderServiceResourceID = v.OrderServiceResource.OrderServiceResourceID,
                         orderServiceResourceName = v.OrderServiceResource.OrderServiceResourceName,
                         orderServiceResourceDesc = v.OrderServiceResource.OrderServiceResourceDesc,
                    };    
                }
                

                orderView.orderServiceResources.AddRange( );
                
                
            }
            if(flag == true){break;}
        }

        if(flag == true) return BadRequest("Get order channels failed.");
        

        OrderChannelsResultViewModel resoultViewModel = new(){
            code = "200",
            message = "",
            orderChannels = [],
        };



    }


    

}




