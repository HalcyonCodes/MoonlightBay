using MoonlightBay.Data.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MoonlightBay.Model;

using MoonlightBay.Web.Models;
using Microsoft.AspNetCore.Authorization;


namespace MoonlightBay.Web.Controllers;

/// <summary>
/// 订单服务控制器
/// </summary>

[Route("api/v1/[controller]/[action]")]
public class OrderServiceController(
    ILoggerFactory loggerFactory,
    UserManager<ApplicationUser> userManager,
    IOrderServiceRepository orderServiceRepository
    ) : Controller
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<AccountController>();
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IOrderServiceRepository _orderServiceRepository = orderServiceRepository;
    

    //SER001-0: AddOrderServiceResource
    //Desc: 添加服务资源
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> AddOrderServiceResource([FromBody] OrderServiceResourceViewModel viewModel )
    {
        OrderServiceResource newOrderServiceResource = new()
        {
            CreatedTime = DateTime.Now,
            OrderServiceResourceName = viewModel.orderServiceResourceName,
            OrderServiceResourceDesc = viewModel.orderServiceResourceDesc
        };

        int? status = await _orderServiceRepository.AddOrderServiceResourceAsync(newOrderServiceResource);
        if(status == null) return BadRequest("add serviceResource faild.");
        return Ok();
    }

    //SER002-0: DeleteOrderServiceResource
    //Desc: 删除服务资源
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteOrderServiceResource([FromBody] OrderServiceResourceViewModel viewModel )
    {
        int status = await _orderServiceRepository.DeleteOrderServiceResourceByIDAsync(viewModel.orderServiceResourceID);
        if(status != 0) return BadRequest("delete order service resource failed.");
        return Ok();
    }

    //SER003-0: GetOrderServiceResources
    //Desc: 获取所有订单服务资源列表
    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetOrderServiceResources()
    {
        List<OrderServiceResource>? orderServiceResources= await _orderServiceRepository.GetOrderServiceResourcesAsync();
        orderServiceResources ??= [];

        OrderServiceResourcesResultViewModel resultViewModel= new(){
            code = "200",
            message = "",
            orderServiceResources = []
        };

        orderServiceResources.ForEach(t => {
            OrderServiceResourceViewModel resourceView = new(){
                orderServiceResourceID = t.OrderServiceResourceID,
                orderServiceResourceName = t.OrderServiceResourceName,
                orderServiceResourceDesc = t.OrderServiceResourceDesc
            };
            resultViewModel.orderServiceResources.Add(resourceView);
        });
        return Ok(resultViewModel);
    }

    //SER004-0: UpdateOrderServiceResource
    //Desc: 更新一个订单服务资源
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateOrderServiceResource(OrderServiceResourceViewModel viewModel){
        OrderServiceResource orderServiceResource = new(){
            OrderServiceResourceID = viewModel.orderServiceResourceID,
            OrderServiceResourceName = viewModel.orderServiceResourceName,
            OrderServiceResourceDesc = viewModel.orderServiceResourceDesc
        };
        int status = await _orderServiceRepository.UpdateOrderServiceResourceAsync(orderServiceResource); 
        if(status != 0) return BadRequest("update order service resource failed.");
        return Ok();
    }

    //SER005-0: AddOrderService
    //Desc: 添加一个订单服务,并初始化
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> AddOrderSercie(OrderServiceViewModel viewModel){
        OrderService newOrderService = new() {
            OrderServiceID = null,
            OrderServiceName = viewModel.orderServiceName,
            OrderServiceDesc = viewModel.orderServiceDesc,
            OrderServiceResources = []
        };
        int? status = await _orderServiceRepository.AddOrderServiceAsync(newOrderService);
        if(status != 0) return BadRequest("add order service failed.");
        return Ok();
    }

    //SER006-0: DeleteOrderService
    //Desc: 删除一个订单服务
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteOrderService(int? orderServiceID){
        int? status = await _orderServiceRepository.DeleteOrderServiceByIDAsync(orderServiceID);
        if(status != 0) return BadRequest("delete order service failed.");
        return Ok();
    }

    //SER007-0: UpdateOrderService
    //Desc: 更新一个订单服务
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateOrderService(OrderServiceViewModel viewModel){
        OrderService? dbOrderService = await _orderServiceRepository.GetOrderServiceByIDWithResourcesAsync(viewModel.orderServiceID);
        if(dbOrderService == null) return BadRequest("Update Order Service failed.");
        int? status = await _orderServiceRepository.UpdateOrderServiceAsync(dbOrderService);
        if(status != 0) return BadRequest("Update Order Service failed.");
        return Ok();
    }

    //SER008-0: GetOrderService
    //Desc: 获得一个订单服务
    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetOrderService(int? orderServiceID){
        if(orderServiceID == null) return BadRequest("get order service failed.");
        OrderService? dbOrderService = await _orderServiceRepository
        .GetOrderServiceByIDWithResourcesAsync(orderServiceID);
        if(dbOrderService == null) return BadRequest("get order service failed.");
        OrderServiceViewModel resultViewModel = new(){
            orderServiceID = dbOrderService.OrderServiceID,
            orderServiceName = dbOrderService.OrderServiceName,
            orderServiceDesc = dbOrderService.OrderServiceDesc,
            createdTime = dbOrderService.CreatedTime,
        };
        resultViewModel.orderServiceResources ??= [];
        dbOrderService.OrderServiceResources ??= [];
        dbOrderService.OrderServiceResources.ForEach(t => {
            OrderServiceResourceViewModel resourceView = new(){
                orderServiceResourceID = t.OrderServiceResourceID,
                orderServiceResourceName = t.OrderServiceResourceName,
                orderServiceResourceDesc = t.OrderServiceResourceDesc
            };
            resultViewModel.orderServiceResources.Add(resourceView);
        });
        return Ok(resultViewModel);
    }

    //SER008-1: GetOrderServices
    //Desc: 获得所有订单服务
    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetOrderServices(){
        List<OrderService>? orderServices = await _orderServiceRepository.GetOrderServicesAsync();
        orderServices ??= [];
        OrderServicesResultViewModel resultViewModel = new(){
            code = "200",
            message = "",
            orderServices = []
        };
        orderServices.ForEach(t => {
            OrderServiceViewModel serviceViewModel = new(){
                orderServiceID = t.OrderServiceID,
                orderServiceName = t.OrderServiceName,
                createdTime = t.CreatedTime,
                orderServiceDesc = t.OrderServiceDesc,
                orderServiceResources = [],
            };
            t.OrderServiceResources ??= [];
            t.OrderServiceResources.ForEach(q => {
                OrderServiceResourceViewModel resourceViewModel = new() {
                    orderServiceResourceID = q.OrderServiceResourceID,
                    orderServiceResourceName = q.OrderServiceResourceName,
                    orderServiceResourceDesc = q.OrderServiceResourceDesc,
                };
                serviceViewModel.orderServiceResources.Add(resourceViewModel);
            });
            resultViewModel.orderServices.Add(serviceViewModel);
        });
        return Ok(resultViewModel);
        
    }

    //SER009-0: AddServiceResourceToOrderService
    //Desc: 为订单服务添加服务资源
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> AddServiceResourceToOrderService(OrderServiceViewModel viewModel){
        OrderService? orderService = await _orderServiceRepository.GetOrderServiceByIDWithResourcesAsync(viewModel.orderServiceID);
        if(orderService == null) return BadRequest("add order service resource to order service failed.");
        viewModel.orderServiceResources ??= [];
        orderService.OrderServiceResources ??= [];
        viewModel.orderServiceResources.ForEach(t => {
            OrderServiceResource orderServiceResource = new() {
                OrderServiceResourceID = t.orderServiceResourceID,
            };
            orderService.OrderServiceResources.Add(orderServiceResource);
        });


        int status = await _orderServiceRepository.AddOrderServiceResourcesToOrderServiceAsync(orderService);
        if(status != 0) return BadRequest("add service resource to service failed.");
        return Ok();
    }

    //SER010-0: AddOrderServiceWorkScript
    //Desc: 添加工作脚本
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> AddOrderServiceWorkScript(OrderServiceViewModel viewModel){
        OrderServiceScript newScript = new(){
            OrderServiceScriptID = null,
            OrderServiceScriptName = viewModel.orderServiceName,
            OrderServiceDesc = viewModel.orderServiceDesc
        };

        int? status = await _orderServiceRepository.AddOrderServiceScriptAsync(newScript);
        if(status != 0) return BadRequest("add order service work script failed.");
        return Ok();
    }

    //SER011-0 DeleteOrderServiceWorkScript
    //Desc: 删除工作脚本
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DelteOrderServiceWorkScript(OrderServiceScriptViewModel viewModel){
        
        OrderServiceScript? script = new(){
            OrderServiceScriptID = viewModel.orderServiceScriptID,
            OrderServiceScriptName = viewModel.orderServiceScriptName,
            OrderServiceDesc = viewModel.orderServiceDesc
        };


        int status = await _orderServiceRepository.DeleteOrderServiceScriptAsync(script);
        if(status != 0) return BadRequest("delete order service work script failed.");
        return Ok();
    }

    //SER012-0 UpdateOrderServiceWorkScript
    //Desc: 更新工作脚本
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateOrderServiceWorkScript(OrderServiceViewModel viewModel){
        OrderServiceScript script = new(){
            OrderServiceScriptID = viewModel.orderServiceID,
            OrderServiceScriptName = viewModel.orderServiceName,
            OrderServiceDesc = viewModel.orderServiceDesc
        };
        int? status = await _orderServiceRepository.UpdateOrderServiceSeriptAsync(script);
        if(status != 0) return BadRequest("update order service work script failed.");
        return Ok();
    }


    //SER013-0 GetOrderServiceWorkScripts
    //Desc: 获取所有工作脚本
    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetOrderServiceWorkScripts(){
        List<OrderServiceScript>? scripts = await _orderServiceRepository.GetOrderServiceScriptsAsync();
        scripts ??= [];

        
        OrderServiceScriptsResultViewModel resultViewModel = new(){
            code = "200",
            message = "",
            orderServiceScripts = [],
        };
        
        scripts.ForEach(t => {
            OrderServiceScriptViewModel scriptViewModel = new(){
                orderServiceScriptID = t.OrderServiceScriptID,
                orderServiceDesc = t.OrderServiceDesc,
                orderServiceScriptName = t.OrderServiceScriptName
            };
            resultViewModel.orderServiceScripts.Add(scriptViewModel);
        });

        return Ok(resultViewModel);
    }






}