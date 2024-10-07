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


    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateOrderLite([FromBody] OrderRequestLiteViewModel viewModel)
    {
        List<Terminal>? terminal = await _terminalRepository.GetUserTerminalsAsync();

        if (terminal == null) return BadRequest("create order failed.");

        OrderChannel? terminalChannel = terminal.SelectMany(q => q.OrderChannels!)
        .FirstOrDefault(q => q.OrderChannelLevel == viewModel.orderChannelLevel);

        Order newOrder = new()
        {
            OrderID = Guid.NewGuid(),
            CreatedTime = DateTime.Now,
            Status = Order.OrderStatus.waiting,
            OrderIndex = null
        };
        Terminal? sourceTerminal = terminal.FirstOrDefault();
        Terminal? targetTerminal = new()
        {
            TerminalID = Guid.Parse(viewModel.order!.sourceTerminalID!)
        };

    
        OrderService? orderService = new()
        {
            OrderServiceID = viewModel.order.orderService!.orderServiceID
        };

        viewModel.order.orderServiceResources ??= [];
        newOrder.OrderResources ??= [];

        int index = 0;
       
        foreach (var q in viewModel.order.orderServiceResources)
        {
            
            
            OrderServiceResourceClass newResourceClass = new()
            {
                OrderServiceResoourceClassID = Guid.NewGuid(),
                CreatedTime = DateTime.Now,
                ResourceIntValue = q.resourceIntValue,
                ResourceDoubleValue = q.resourceDoubleValue,
                ResourceStringValue = q.resourceStringValue,
            };
            newOrder.OrderResources.Add(newResourceClass);
            index++;
        }

        newOrder.OrderService = orderService;
        newOrder.SourceTerminal = sourceTerminal;
        newOrder.TargetTerminal = targetTerminal;
        Guid? status = await _ordereRepository.AddOrderAsync(newOrder);

        int status2 = await _ordereRepository.AddOrderToOrderChannelAsync((Guid)status, (int)viewModel.orderChannelLevel!);
    
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

        OrderServiceScriptViewModel script = new()
        {
            orderServiceScriptID = order.OrderService.WorkScript.OrderServiceScriptID,
            orderServiceScriptName = order.OrderService.WorkScript.OrderServiceScriptName,
            orderServiceDesc = order.OrderService.WorkScript.OrderServiceDesc
        };
        OrderServiceViewModel serviceViewModel = new()
        {
            orderServiceID = order.OrderService.OrderServiceID,
            orderServiceDesc = order.OrderService.OrderServiceDesc,
            orderServiceName = order.OrderService.OrderServiceName,
            orderServiceResources = [],
            orderServiceWorkScript = script
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
                OrderServiceScriptViewModel script = new()
                {
                    orderServiceScriptID = s.OrderService.WorkScript.OrderServiceScriptID,
                    orderServiceScriptName = s.OrderService.WorkScript.OrderServiceScriptName,
                    orderServiceDesc = s.OrderService.WorkScript.OrderServiceDesc
                };
                OrderServiceViewModel orderServiceViewModel = new(){
                    orderServiceID = s.OrderService!.OrderServiceID,
                    orderServiceName = s.OrderService!.OrderServiceName,
                    orderServiceDesc = s.OrderService!.OrderServiceDesc,
                    orderServiceResources = [],
                    orderServiceWorkScript = script,
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
                orderChannel.orders.Add(orderView);

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
    public async Task<IActionResult> UpdateOrderStatus([FromBody] OrderStatusViewModel viewModel){
        
        Order? order = await _ordereRepository.GetOrderByIDAsync(viewModel.orderID);
        if(order == null) return BadRequest("get order failed.");
        order.Status = (Order.OrderStatus?)viewModel.status;
        int status = await _ordereRepository.UpdateOrderStatusAsync(order);
        if(status != 0) return BadRequest("update order status failed.");
        return Ok();
    }

    //UI 
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetOrderChannel([FromQuery] int channelLevels, [FromQuery] Guid terminalID){
        Terminal? terminal = await _terminalRepository.GetTerminalByIDAsync(terminalID);
        if(terminal == null) return BadRequest("get terminal failed.");
        
        List<OrderChannel>? channels = terminal.OrderChannels;
        if(channels == null) return BadRequest("get order channel failed.");
        OrderChannel? channel = channels.FirstOrDefault(q => q.OrderChannelLevel == channelLevels);
        if(channel == null) return BadRequest("get order channel failed.");
        ChannelOrderResultViewModel resultViewModel = new(){
            code = "200",
            message = "",
            data = new (){channel = []},
        };
        channel.Orders ??= [];
        ChannelOrderViewModel temp;

        foreach(var v in channel.Orders){

            temp = new(){
                id = v.OrderID.ToString(),
                date = v.CreatedTime!.Value.ToString("yyyy/M/d"),
                time = v.CreatedTime.Value.ToString("HH:mm"),
                name = v.OrderService!.OrderServiceName,
            };
            resultViewModel.data.channel.Add(temp);
        }

        return Ok(resultViewModel);

    }
    

     


}




