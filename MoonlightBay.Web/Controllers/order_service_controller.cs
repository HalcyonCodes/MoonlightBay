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
    [Authorize]
    public async Task<IActionResult> AddOrderServiceResource([FromBody] OrderServiceResourceViewModel viewModel )
    {
        ApplicationUser? user = await _userManager.GetUserAsync(HttpContext.User);
        if(user == null) return BadRequest("faild.");
        if(user.Role != "Admin") return BadRequest("faild.");
        OrderServiceResource newOrderServiceResource = new()
        {
            OrderServiceResourceID = null,
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
    [Authorize]
    public async Task<IActionResult> DeleteOrderServiceResource([FromBody] OrderServiceResourceViewModel viewModel )
    {
        ApplicationUser? user = await _userManager.GetUserAsync(HttpContext.User);
        if(user == null) return BadRequest("faild.");
        if(user.Role != "Admin") return BadRequest("faild.");
        int status = await _orderServiceRepository.DeleteOrderServiceResourceByIDAsync(viewModel.orderServiceResourceID);
        if(status != 0) return BadRequest("delete order service resource failed.");
        return Ok();
    }

    //SER003-0: GetOrderServiceResources
    //Desc: 获取所有订单服务资源列表
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetOrderServiceResources()
    {
        ApplicationUser? user = await _userManager.GetUserAsync(HttpContext.User);
        if(user == null) return BadRequest("faild.");
        if(user.Role != "Admin") return BadRequest("faild.");
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
    [Authorize]
    public async Task<IActionResult> UpdateOrderServiceResource([FromBody] OrderServiceResourceViewModel viewModel){
        ApplicationUser? user = await _userManager.GetUserAsync(HttpContext.User);
        if(user == null) return BadRequest("faild.");
        if(user.Role != "Admin") return BadRequest("faild.");
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
    [Authorize]
    public async Task<IActionResult> AddOrderService([FromBody] OrderServiceViewModel viewModel){
        ApplicationUser? user = await _userManager.GetUserAsync(HttpContext.User);
        if(user == null) return BadRequest("faild.");
        if(user.Role != "Admin") return BadRequest("faild.");
        OrderService newOrderService = new() {
            OrderServiceID = null,
            OrderServiceName = viewModel.orderServiceName,
            OrderServiceDesc = viewModel.orderServiceDesc,
            OrderServiceResources = []
        };
        int? status = await _orderServiceRepository.AddOrderServiceAsync(newOrderService);
        if(status == null) return BadRequest("add order service failed.");
        return Ok();
    }

    //SER006-0: DeleteOrderService
    //Desc: 删除一个订单服务
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> DeleteOrderService([FromBody] int? orderServiceID){
        
        int? status = await _orderServiceRepository.DeleteOrderServiceByIDAsync(orderServiceID);
        if(status != 0) return BadRequest("delete order service failed.");
        return Ok();
    }

    //SER007-0: UpdateOrderService
    //Desc: 更新一个订单服务
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> UpdateOrderService([FromBody] OrderServiceViewModel viewModel){
        OrderService? dbOrderService = await _orderServiceRepository.GetOrderServiceByIDWithResourcesAsync(viewModel.orderServiceID);
        if(dbOrderService == null) return BadRequest("Update Order Service failed.");
        dbOrderService = new(){
            OrderServiceID = dbOrderService.OrderServiceID,
            OrderServiceDesc = dbOrderService.OrderServiceDesc,
            OrderServiceName = dbOrderService.OrderServiceName,
            OrderServiceResources = []
        };
        dbOrderService.OrderServiceResources ??= [];
        dbOrderService.OrderServiceResources.Clear();
        viewModel.orderServiceResources ??= [];
        foreach(var v in viewModel.orderServiceResources){
            OrderServiceResource resource = new(){
                OrderServiceResourceID = v.orderServiceResource!.orderServiceResourceID,
               
            };
            OrderServiceResourceClass resourceClass = new(){
                OrderServiceResoourceClassID = v.orderServiceResourceClasssID,
                CreatedTime = v.createdTime,
                ResourceIntValue = v.resourceIntValue,
                ResourceDoubleValue = v.resourceDoubleValue,
                ResourceStringValue = v.resourceStringValue,
                OrderServiceResource = resource,
            };
            dbOrderService.OrderServiceResources.Add(resourceClass);
        }

        int? status = await _orderServiceRepository.UpdateOrderServiceAsync(dbOrderService);
        if(status != 0) return BadRequest("Update Order Service failed.");
        return Ok();
    }


    //SER008-0: GetOrderService
    //Desc: 获得一个订单服务
    [HttpGet]
    [Authorize]
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
        foreach(var w in dbOrderService.OrderServiceResources){
            OrderServiceResourceViewModel newResourceView = new(){
                orderServiceResourceID = w.OrderServiceResource!.OrderServiceResourceID,
                orderServiceResourceDesc = w.OrderServiceResource.OrderServiceResourceDesc,
                orderServiceResourceName = w.OrderServiceResource.OrderServiceResourceName,
            };
            OrderServiceResourceClassesViewModel classesViewModel = new(){
                orderServiceResourceClasssID = w.OrderServiceResoourceClassID,
                orderServiceResource = newResourceView,
                createdTime = w.CreatedTime,
                resourceDoubleValue = w.ResourceDoubleValue,
                resourceStringValue = w.ResourceStringValue,
                resourceIntValue = w.ResourceIntValue
            };
            
        }

        return Ok(resultViewModel);
    }

    //SER008-1: GetOrderServices
    //Desc: 获得所有订单服务
    [HttpGet]
    [Authorize]
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
                    orderServiceResourceID = q.OrderServiceResource!.OrderServiceResourceID,
                    orderServiceResourceDesc = q.OrderServiceResource.OrderServiceResourceDesc,
                    orderServiceResourceName = q.OrderServiceResource.OrderServiceResourceName,
                };
                OrderServiceResourceClassesViewModel resouceClassViewModel = new(){
                    orderServiceResourceClasssID = q.OrderServiceResoourceClassID,
                    createdTime = q.CreatedTime,
                    resourceIntValue = q.ResourceIntValue,
                    resourceDoubleValue = q.ResourceDoubleValue,
                    resourceStringValue = q.ResourceStringValue,
                    orderServiceResource = resourceViewModel,  
                };
                serviceViewModel.orderServiceResources.Add(resouceClassViewModel);
            });

            resultViewModel.orderServices.Add(serviceViewModel);
        });
        return Ok(resultViewModel);
    }

    //SER009-0: AddServiceResourceToOrderService
    //Desc: 为订单服务添加服务资源
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddServiceResourceToOrderService([FromBody] OrderServiceViewModel viewModel){
        OrderService? orderService = await _orderServiceRepository.GetOrderServiceByIDWithResourcesAsync(viewModel.orderServiceID);
        if(orderService == null) return BadRequest("add order service resource to order service failed.");
        viewModel.orderServiceResources ??= [];
        orderService.OrderServiceResources ??= [];
        foreach( var w in viewModel.orderServiceResources){
            OrderServiceResource  resource = new(){
                OrderServiceResourceID = w.orderServiceResource!.orderServiceResourceID,
                OrderServiceResourceDesc = w.orderServiceResource!.orderServiceResourceDesc,
                OrderServiceResourceName = w.orderServiceResource!.orderServiceResourceName,
            };
            OrderServiceResourceClass newClass = new(){
                OrderServiceResoourceClassID = w.orderServiceResourceClasssID,
                CreatedTime = w.createdTime,
                ResourceIntValue = w.resourceIntValue,
                ResourceDoubleValue = w.resourceDoubleValue,
                ResourceStringValue = w.resourceStringValue,
                OrderServiceResource = resource,
            };
            orderService.OrderServiceResources.Add(newClass);
        }


        int status = await _orderServiceRepository.AddOrderServiceResourcesToOrderServiceAsync(orderService);
        if(status != 0) return BadRequest("add service resource to service failed.");
        return Ok();
    }

    //SER010-0: AddOrderServiceWorkScript
    //Desc: 添加工作脚本
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddOrderServiceWorkScript([FromBody] OrderServiceViewModel viewModel){
            ApplicationUser? user = await _userManager.GetUserAsync(HttpContext.User);
        if(user == null) return BadRequest("faild.");
        if(user.Role != "Admin") return BadRequest("faild.");
        OrderServiceScript newScript = new(){
            //OrderServiceScriptID = null,
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
    [Authorize]
    public async Task<IActionResult> DelteOrderServiceWorkScript([FromBody] OrderServiceScriptViewModel viewModel){
            ApplicationUser? user = await _userManager.GetUserAsync(HttpContext.User);
        if(user == null) return BadRequest("faild.");
        if(user.Role != "Admin") return BadRequest("faild.");
        
        OrderServiceScript? script = new(){
            OrderServiceScriptID = (int)viewModel.orderServiceScriptID,
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
    [Authorize]
    public async Task<IActionResult> UpdateOrderServiceWorkScript([FromBody] OrderServiceViewModel viewModel){
            ApplicationUser? user = await _userManager.GetUserAsync(HttpContext.User);
        if(user == null) return BadRequest("faild.");
        if(user.Role != "Admin") return BadRequest("faild.");
        OrderServiceScript script = new(){
            OrderServiceScriptID = (int)viewModel.orderServiceID,
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
    [Authorize]
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



    [HttpPost]
    [Authorize]
    public async Task<IActionResult> test([FromBody] ListTest test){
        /*OrderService? dbOrderService = await _orderServiceRepository.GetOrderServiceByIDWithResourcesAsync(viewModel.orderServiceID);
        if(dbOrderService == null) return BadRequest("Update Order Service failed.");
        int? status = await _orderServiceRepository.UpdateOrderServiceAsync(dbOrderService);
        if(status != 0) return BadRequest("Update Order Service failed.");*/
        int a;
        return Ok();
    }





}