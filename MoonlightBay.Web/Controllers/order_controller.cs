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
using System.Threading.Channels;


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
    public async Task<IActionResult> CreateOrder([FromBody]OrderChannelViewModel viewModel){
        
        List<Terminal>? terminal = await _terminalRepository.GetUserTerminalsAsync();

        if ( terminal == null ) return BadRequest("create order failed.");
  
        OrderChannel? terminalChannel = terminal.SelectMany(q => q.OrderChannels!)
        .FirstOrDefault(q => q.OrderChannelLevel == viewModel.orderChannelLevel);

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

            if (s.orderService == null) return BadRequest("create order failed.");
            OrderService? orderService = new()
            {
                OrderServiceID = s.orderService!.orderServiceID,
            };

            s.orderServiceResources ??= [];
            newOrder.OrderResources ??= [];

            foreach (var q in s.orderServiceResources)
            {
                if (q.orderServiceResource == null)
                {
                    flag = true;
                    break;
                }
                OrderServiceResource classResource = new()
                {
                    //OrderServiceResourceID = (int)q.orderServiceResource.orderServiceResourceID,
                    OrderServiceResourceID = q.orderServiceResource.orderServiceResourceID,
                };
                OrderServiceResourceClass newResourceClass = new()
                {
                    OrderServiceResoourceClassID = Guid.NewGuid(),
                    CreatedTime = DateTime.Now,
                    OrderServiceResource = classResource,
                    ResourceIntValue = q.resourceIntValue,
                    ResourceDoubleValue = q.resourceDoubleValue,
                    ResourceStringValue = q.resourceStringValue,
                };
                newOrder.OrderResources.Add(newResourceClass);
            }
            if (flag == true) break;
            newOrder.OrderService = orderService;
            newOrder.SourceTerminal = sourceTerminal;
            newOrder.TargetTerminal = targetTerminal;
            Guid? status = await _ordereRepository.AddOrderAsync(newOrder);
            if (status == null)
            {
                flag = true;
                break;
            }
            int status2 = await _ordereRepository.AddOrderToOrderChannelAsync((Guid)status, (int)viewModel.orderChannelLevel!);
            int a;
        }

        if (flag == true) return BadRequest("created order failed.");


        return Ok();    
    }

    [HttpGet]
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
                orderServiceResourceID = q.OrderServiceResource!.OrderServiceResourceID,
                orderServiceResourceName = q.OrderServiceResource!.OrderServiceResourceName,
                orderServiceResourceDesc = q.OrderServiceResource!.OrderServiceResourceDesc,
            };
            OrderServiceResourceClassesViewModel newclass = new(){
                orderServiceResourceClasssID = q.OrderServiceResoourceClassID,
                orderServiceResource = newResourceView,
                resourceIntValue = q.ResourceIntValue,
                resourceDoubleValue = q.ResourceDoubleValue,
                resourceStringValue = q.ResourceStringValue
            };
            serviceViewModel.orderServiceResources.Add(newclass);

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
            OrderServiceResourceViewModel viewModel = new(){
                orderServiceResourceID = q.OrderServiceResource!.OrderServiceResourceID,
                orderServiceResourceDesc = q.OrderServiceResource.OrderServiceResourceDesc,
                orderServiceResourceName = q.OrderServiceResource.OrderServiceResourceName,
            };

            OrderServiceResourceClassesViewModel classViewModel = new(){
                orderServiceResourceClasssID = q.OrderServiceResoourceClassID,

                orderServiceResource = viewModel,
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

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetOrderChannels(){
        List<OrderChannel>? orderChannels = await _ordereRepository.GetOrderChannelsAsync();

        if(orderChannels == null) return BadRequest("Get order channels failed.");
        orderChannels ??= [];
        orderChannels = [.. orderChannels.OrderBy(t => t.OrderChannelLevel)];
        List<OrderChannelViewModel> orderChannelViews = [];

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
                        orderServiceResourceID = c.OrderServiceResource!.OrderServiceResourceID,
                        orderServiceResourceName = c.OrderServiceResource!.OrderServiceResourceName,
                        orderServiceResourceDesc = c.OrderServiceResource!.OrderServiceResourceDesc,
                    };
                    OrderServiceResourceClassesViewModel classView = new(){
                        orderServiceResourceClasssID = c.OrderServiceResoourceClassID,
                        orderServiceResource = resourceView,
                    };
                    orderServiceViewModel.orderServiceResources.Add(classView);
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
                orderView.orderService = orderServiceViewModel;

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
                    
                    orderView.orderServiceResources.Add(viewModel);
                }

            }
            orderChannelViews.Add(orderChannel);

            if(flag == true){break;}
        }
        
        if(flag == true) return BadRequest("Get order channels failed.");
        
        OrderChannelsResultViewModel resultViewModel = new(){
            code = "200",
            message = "",
            orderChannels = orderChannelViews,
        };

        return Ok(resultViewModel);
    }


    [HttpPost]
    [Authorize]
    public async Task<IActionResult> UpdateOrderStatus([FromBody] Guid? orderID, int? orderStatus){
        if(orderStatus == null) return BadRequest("update order status failed.");
        Order? order = await _ordereRepository.GetOrderByIDAsync(orderID);
        if(order == null) return BadRequest("get order failed.");
        order.Status = (Order.OrderStatus?)orderStatus;
        int status = await _ordereRepository.UpdateOrderStatusAsync(order);
        if(status != 0) return BadRequest("update order status failed.");
        return Ok();
    }


}




