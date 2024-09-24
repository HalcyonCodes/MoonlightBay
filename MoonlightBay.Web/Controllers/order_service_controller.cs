using MoonlightBay.Data.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MoonlightBay.Model;

using MoonlightBay.Web.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace MoonlightBay.Web.Controllers;

/// <summary>
/// 订单服务控制器
/// </summary>

[Route("api/v1/[controller]/[action]")]
public class OrderServiceController(
    ILoggerFactory loggerFactory,
    UserManager<ApplicationUser> userManager,
    IOrderServiceRepository orderServiceRepository,
    IAccountRepository accountRepository
    ) : Controller
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<AccountController>();
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IOrderServiceRepository _orderServiceRepository = orderServiceRepository;
    private readonly IAccountRepository _accountRepository = accountRepository;
    
    //UI
    //SER001-0: AddOrderServiceResource
    //Desc: 添加服务资源
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddOrderServiceResource([FromBody] OrderServiceResourceViewModel viewModel )
    {
       var userName = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if(userName == null) return BadRequest("faild.");
        ApplicationUser? user = await _accountRepository.GetUserByUserNameAsync(userName);
        if(user == null){
            userName = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            if(userName == null) return BadRequest("faild.");
            user = await _accountRepository.GetUserByUserNameAsync(userName!);
        }


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


    //UI
    //SER002-0: DeleteOrderServiceResource
    //Desc: 删除服务资源
    [HttpPost]
    [Authorize] 
    public async Task<IActionResult> DeleteOrderServiceResource([FromBody] OrderServiceResourceViewModel viewModel )
    {
         var userName = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if(userName == null) return BadRequest("faild.");
        ApplicationUser? user = await _accountRepository.GetUserByUserNameAsync(userName);
        if(user == null){
            userName = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            if(userName == null) return BadRequest("faild.");
            user = await _accountRepository.GetUserByUserNameAsync(userName!);
        }
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
         var userName = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if(userName == null) return BadRequest("faild.");
        ApplicationUser? user = await _accountRepository.GetUserByUserNameAsync(userName);
        if(user == null){
            userName = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            if(userName == null) return BadRequest("faild.");
            user = await _accountRepository.GetUserByUserNameAsync(userName!);
        }
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

    
    
    //UI
    //SER003-1: GetOrderServiceResourcesPage
    //Desc: 通过页面获取订单服务资源列表
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetOrderServiceResourcesPage( [FromQuery] int pageIndex){
        var userName = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if(userName == null) return BadRequest("faild.");
        ApplicationUser? user = await _accountRepository.GetUserByUserNameAsync(userName);
        if(user == null){
            userName = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            if(userName == null) return BadRequest("faild.");
            user = await _accountRepository.GetUserByUserNameAsync(userName!);
        }
  
        if(user == null) return BadRequest("faild.");
        if(user.Role != "Admin") return BadRequest("faild.");
        List<OrderServiceResource>? orderServiceResources= await _orderServiceRepository.GetOrderServiceResourcesPageAsync(pageIndex);
        orderServiceResources ??= [];
        OrderResourceUIResoultViewModel resultViewModel = new(){
            code = "200",
            message = "",
            data = new OrderResourceUiDataViewModel()
        };
        resultViewModel.data.orderResources = [];
        OrderResourceUIViewModel temp;

        foreach(var item in orderServiceResources){
            int count = await _orderServiceRepository.GetOrderServiceResourcesBindingCountAsync(item.OrderServiceResourceID);
            temp = new() {
                id = item.OrderServiceResourceID.ToString(),
                name = item.OrderServiceResourceName,
                desc = item.OrderServiceResourceDesc,
                bindingCount = count.ToString()
            };
           resultViewModel.data.orderResources.Add(temp); 
        }
        return Ok(resultViewModel);

    }

    //UI
    //SER003-2: GetOrderServiceResourcesByServiceID
    //Desc: 通过serviceID获取订单服务资源列表
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetOrderServiceResourcesByServiceID([FromQuery] int serviceID){
        var userName = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if(userName == null) return BadRequest("faild.");
        ApplicationUser? user = await _accountRepository.GetUserByUserNameAsync(userName);
        if(user == null){
            userName = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            if(userName == null) return BadRequest("faild.");
            user = await _accountRepository.GetUserByUserNameAsync(userName!);
        }
  
        if(user == null) return BadRequest("faild.");
        if(user.Role != "Admin") return BadRequest("faild.");
        OrderService? orderService = await _orderServiceRepository.GetOrderServiceByIDWithResourcesAsync(serviceID);
        if(orderService == null) return BadRequest("faild.");
        List<OrderServiceResourceClass>? resources = orderService.OrderServiceResources;
        resources ??= [];
        //
        OrderResourceUIResoultViewModel viewModel = new(){
            code = "200",
            message = "",
            data = new OrderResourceUiDataViewModel()
        };
        viewModel.data.orderResources = [];

        OrderResourceUIViewModel temp;
        foreach(var q in resources){
            int bindingCount = await _orderServiceRepository.GetOrderServiceResourcesBindingCountAsync(q.OrderServiceResource!.OrderServiceResourceID);
            temp = new(){
                id = q.OrderServiceResource.OrderServiceResourceID.ToString(),
                name = q.OrderServiceResource.OrderServiceResourceName,
                desc = q.OrderServiceResource.OrderServiceResourceDesc,
                bindingCount = bindingCount.ToString(),
            };
            viewModel.data.orderResources.Add(temp);
        }
        return Ok(viewModel);
    }

    //UI
    //SER003-2: GetOrderServiceResourcesUI
    //Desc: 获取所有订单服务资源列表
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetOrderServiceResourcesUI()
    {
         var userName = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if(userName == null) return BadRequest("faild.");
        ApplicationUser? user = await _accountRepository.GetUserByUserNameAsync(userName);
        if(user == null){
            userName = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            if(userName == null) return BadRequest("faild.");
            user = await _accountRepository.GetUserByUserNameAsync(userName!);
        }
        if(user == null) return BadRequest("faild.");
        if(user.Role != "Admin") return BadRequest("faild.");
        List<OrderServiceResource>? orderServiceResources= await _orderServiceRepository.GetOrderServiceResourcesAsync();
        orderServiceResources ??= [];

        OrderResourceUIResoultViewModel resultViewModel= new(){
            code = "200", 
            message = "",
            data = new OrderResourceUiDataViewModel()
        };
        resultViewModel.data.orderResources = [];

        foreach (var q in orderServiceResources)
        {
            int bindingCount = await _orderServiceRepository.GetOrderServiceResourcesBindingCountAsync(q.OrderServiceResourceID);
            OrderResourceUIViewModel resourceView = new()
            {
                id = q.OrderServiceResourceID.ToString(),
                name = q.OrderServiceResourceName,
                desc = q.OrderServiceResourceDesc,
                bindingCount = bindingCount.ToString(),
            };
            resultViewModel.data.orderResources.Add(resourceView);
        }
        return Ok(resultViewModel);
    }




    //SER004-0: UpdateOrderServiceResource
    //Desc: 更新一个订单服务资源
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> UpdateOrderServiceResource([FromBody] OrderServiceResourceViewModel viewModel){
         var userName = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if(userName == null) return BadRequest("faild.");
        ApplicationUser? user = await _accountRepository.GetUserByUserNameAsync(userName);
        if(user == null){
            userName = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            if(userName == null) return BadRequest("faild.");
            user = await _accountRepository.GetUserByUserNameAsync(userName!);
        }
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

    //UI
    //SER004-1: UpdateOrderServiceResources
    //Desc: 更新一个订单服务所有资源
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> UpdateOrderServiceResources ( [FromBody] UpdateOrderResourceUIDViewModel viewModel){
        var userName = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if(userName == null) return BadRequest("faild.");
        ApplicationUser? user = await _accountRepository.GetUserByUserNameAsync(userName);
        if(user == null){
            userName = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            if(userName == null) return BadRequest("faild.");
            user = await _accountRepository.GetUserByUserNameAsync(userName!);
        }
        if(user == null) return BadRequest("faild.");
        if(user.Role != "Admin") return BadRequest("faild.");
        OrderService? orderService = new()
        {
            OrderServiceID = int.Parse(viewModel.orderServiceId!)
        };
        viewModel.orderResources ??= [];
        List<OrderServiceResource> resources = [];
        foreach(var q in viewModel.orderResources!){
            OrderServiceResource newResource = new(){
                OrderServiceResourceID = int.Parse(q.id!)
            };
            resources.Add(newResource);
        }
        int status = await _orderServiceRepository.UpdateOrderServiceResourcesAsync(int.Parse(viewModel.orderServiceId!), resources);
        if(status != 0) return BadRequest("faild.");
        return Ok();

    }
    
    
    //SER005-0: AddOrderService
    //Desc: 添加一个订单服务,并初始化
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddOrderService([FromBody] OrderServiceViewModel viewModel){
        var userName = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if(userName == null) return BadRequest("faild.");
        ApplicationUser? user = await _accountRepository.GetUserByUserNameAsync(userName);
        if(user == null){
            userName = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            if(userName == null) return BadRequest("faild.");
            user = await _accountRepository.GetUserByUserNameAsync(userName!);
        }
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
    //UI
    //SER005-1: AddOrderServiceUI
    //Desc: 添加一个订单服务,并初始化
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddOrderServiceUI([FromBody] OrderServiceAddUIViewModel viewModel){
         var userName = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if(userName == null) return BadRequest("faild.");
        ApplicationUser? user = await _accountRepository.GetUserByUserNameAsync(userName);
        if(user == null){
            userName = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            if(userName == null) return BadRequest("faild.");
            user = await _accountRepository.GetUserByUserNameAsync(userName!);
        }
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
    //UI
    //SER006-0: DeleteOrderService
    //Desc: 删除一个订单服务
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> DeleteOrderService([FromBody] OrderServiceDeleteUIViewModel viewModel){
        
        int? status = await _orderServiceRepository.DeleteOrderServiceByIDAsync(viewModel.orderServiceID);
        if(status != 0) return BadRequest("delete order service failed.");
        return Ok();
    }
    
    //dddd
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
            OrderServiceResource resource;
            if (v.orderServiceResource != null)
            {
                resource = new()
                {
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

    //UI
    //SER008-2: GetOrderServicesByResource
    //Desc: 获得orderResource所绑定的所有服务
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetOrderServicesByResource([FromQuery] int orderResourceID){
        List<OrderService>? orderServices = await _orderServiceRepository.GetOrderServicesByResourcesAsync(orderResourceID);
        orderServices ??= [];
        OrderServiceUIResoultViewModel resultViewModel = new(){
            code = "200",
            message = "",
            data = new OrderServiceUIDataViewModel(),
        };
        OrderServiceUIViewModel serviceViewModel;
        resultViewModel.data.orderServices = [];
        orderServices.ForEach(q => {
            serviceViewModel = new(){
                id = q.OrderServiceID.ToString(),
                name = q.OrderServiceName,
                desc = q.OrderServiceDesc,
            };
           resultViewModel.data.orderServices.Add(serviceViewModel); 
        });
        return Ok(resultViewModel);
    }

    //UI
    //SER008-3: GetOrderServicesByPageIndex
    //Desc: 获取订单页面
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetOrderServicesByPageIndex([FromQuery] int pageIndex){
        List<OrderService>? orderServices = await _orderServiceRepository.GetOrderServicesByPageIndexAsync(pageIndex);
        orderServices ??= [];
        OrderServiceUIPageResultViewModel resultViewModel = new(){
            code = "200",
            message = "",
            data = new OrderServiceUIPageDataViewModel(),
        };
        resultViewModel.data.orderServices = [];
        OrderServiceUIPageViewModel temp;
       
            foreach(var q in orderServices){
            List<string> resources = [];
            q.OrderServiceResources?.ForEach(w => {
                resources.Add(w.OrderServiceResource!.OrderServiceResourceName!);
            });
            int count = await _orderServiceRepository.GetOrderServiceBindingCountAsync(q.OrderServiceID);
            string countStr = count.ToString();

            temp = new(){
                id = q.OrderServiceID.ToString(),
                name = q.OrderServiceName,
                desc = q.OrderServiceDesc,
                resources = resources,
                workScript = q.WorkScript == null? "null" : q.WorkScript.OrderServiceScriptName,
                bindingCount = countStr,
            };
            resultViewModel.data.orderServices.Add(temp);
        };
        return Ok(resultViewModel);
    }

    //UI
    //SER009-0: AddServiceResourceToOrderService
    //Desc: 为订单服务添加服务资源
    
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddServiceResourceToOrderService([FromBody] OrderServiceViewModel viewModel){
        OrderService? orderService = await _orderServiceRepository.GetOrderServiceByIDWithResourcesAsync(viewModel.orderServiceID);
        if(orderService == null) return BadRequest("add order service resource to order service failed.");
        viewModel.orderServiceResources ??= [];
        orderService.OrderServiceResources ??= [];

        bool flag = false; 
        List<OrderServiceResourceClass> oldClass = new([.. orderService.OrderServiceResources]);

        int status = 0;
        foreach(var w in oldClass){
            status = await _orderServiceRepository.DeleteOrderServiceClassAsync(w.OrderServiceResoourceClassID);
            if(status != 0) break;
        }
        if(status != 0){
            return BadRequest("");
        }
        

        foreach( var w in viewModel.orderServiceResources){
            OrderServiceResource? resource = await _orderServiceRepository.GetOrderServiceResourceByIDAsync(w.orderServiceResource!.orderServiceResourceID);
            if(resource == null){
                flag = true;
                break;
            }
            OrderServiceResourceClass newClass = new(){
                OrderServiceResoourceClassID = Guid.NewGuid(),
                CreatedTime = DateTime.Now,
                ResourceIntValue = w.resourceIntValue,
                ResourceDoubleValue = w.resourceDoubleValue,
                ResourceStringValue = w.resourceStringValue,
                OrderServiceResource = resource,
            };
            orderService.OrderServiceResources.Add(newClass);
        }
        if(flag == true) return BadRequest("add service resource to service failed.");
        status = await _orderServiceRepository.AddOrderServiceResourcesToOrderServiceAsync(orderService);
        if(status != 0) return BadRequest("add service resource to service failed.");
        return Ok();
    }

 
    //UI
    //SER010-0: AddOrderServiceWorkScript
    //Desc: 添加工作脚本
    
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddOrderServiceWorkScript([FromBody] OrderServiceScriptViewModel viewModel){
        var userName = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if(userName == null) return BadRequest("faild.");
        ApplicationUser? user = await _accountRepository.GetUserByUserNameAsync(userName);
        if(user == null){
            userName = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            if(userName == null) return BadRequest("faild.");
            user = await _accountRepository.GetUserByUserNameAsync(userName!);
        }
        if(user == null) return BadRequest("faild.");
        if(user.Role != "Admin") return BadRequest("faild.");
        OrderServiceScript newScript = new(){
            //OrderServiceScriptID = null,
            OrderServiceScriptName = viewModel.orderServiceScriptName,
            OrderServiceDesc = viewModel.orderServiceDesc,
        };

        int? status = await _orderServiceRepository.AddOrderServiceScriptAsync(newScript);
        if(status < 0) return BadRequest("add order service work script failed.");
        return Ok();
    }

    //SER011-0 DeleteOrderServiceWorkScript
    //Desc: 删除工作脚本
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> DeleteOrderServiceWorkScript([FromBody] OrderServiceScriptViewModel viewModel){
         var userName = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if(userName == null) return BadRequest("faild.");
        ApplicationUser? user = await _accountRepository.GetUserByUserNameAsync(userName);
        if(user == null){
            userName = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            if(userName == null) return BadRequest("faild.");
            user = await _accountRepository.GetUserByUserNameAsync(userName!);
        }
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
    public async Task<IActionResult> UpdateOrderServiceWorkScript([FromBody] OrderServiceScriptViewModel viewModel){
         var userName = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if(userName == null) return BadRequest("faild.");
        ApplicationUser? user = await _accountRepository.GetUserByUserNameAsync(userName);
        if(user == null){
            userName = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            if(userName == null) return BadRequest("faild.");
            user = await _accountRepository.GetUserByUserNameAsync(userName!);
        }
        if(user == null) return BadRequest("faild.");
        if(user.Role != "Admin") return BadRequest("faild.");
        OrderServiceScript script = new(){
            OrderServiceScriptID = viewModel.orderServiceScriptID,
            OrderServiceScriptName = viewModel.orderServiceScriptName,
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

    //UI
    //SER013-1 GetOrderServiceWorkScriptsUI
    //Desc: 获取所有工作脚本
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetOrderServiceWorkScriptsUI(){
        List<OrderServiceScript>? scripts = await _orderServiceRepository.GetOrderServiceScriptsAsync();
        scripts ??= [];
        OrderServiceUIWorkScriptResultViewModel resultViewModel = new(){
            code = "200",
            message = "",
            data = new OrderServiceUIWorkScriptDataViewModel(),
        };
        resultViewModel.data.orderWorkScripts ??= [];
        OrderServiceUIWorkScriptViewModel temp;
        
            foreach(var q in scripts){
            int bindingCount = await _orderServiceRepository.GetOrderServiceScriptBindingCountAsync(q.OrderServiceScriptID);
            temp = new(){
                id = q.OrderServiceScriptID.ToString(),
                name = q.OrderServiceScriptName,
                desc = q.OrderServiceDesc,
                bindingCount = bindingCount.ToString()
                
            };
            resultViewModel.data.orderWorkScripts.Add(temp);
        }

        return Ok(resultViewModel);
    }
     //UI
    //SER013-2 GetOrderServiceWorkScriptsUI
    //Desc: 获取指定service的脚本
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetOrderServiceWorkScriptUI([FromQuery] int serviceID)
    {
        OrderService? orderService = await _orderServiceRepository.GetOrderServiceByIDWithResourcesAsync(serviceID);
        if (orderService == null) return BadRequest("get order service failed.");
        OrderServiceScript? script = orderService.WorkScript;

        OrderServiceUIWorkScriptResultViewModel resultViewModel = new()
        {
            code = "200",
            message = "",
            data = new OrderServiceUIWorkScriptDataViewModel(),
        };
        resultViewModel.data.orderWorkScripts ??= [];
        OrderServiceUIWorkScriptViewModel temp;
        if (script != null)
        {
            int bindingCount = await _orderServiceRepository.GetOrderServiceScriptBindingCountAsync(script.OrderServiceScriptID);
            temp = new()
            {
                id = script.OrderServiceScriptID.ToString(),
                name = script.OrderServiceScriptName,
                desc = script.OrderServiceDesc,
                bindingCount = bindingCount.ToString()

            };
            resultViewModel.data.orderWorkScripts.Add(temp);
        }
        return Ok(resultViewModel);


    }


    //UI
    //SER014-0 AddOrderServiceWorkScriptsToOrderService
    //Desc: 将工作脚本添加到OrderService里
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddOrderServiceWorkScriptToOrderSerivce([FromBody] OrderServiceViewModel viewModel){
         var userName = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if(userName == null) return BadRequest("faild.");
        ApplicationUser? user = await _accountRepository.GetUserByUserNameAsync(userName);
        if(user == null){
            userName = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            if(userName == null) return BadRequest("faild.");
            user = await _accountRepository.GetUserByUserNameAsync(userName!);
        }
        if(user == null) return BadRequest("faild.");
        if(user.Role != "Admin") return BadRequest("faild.");

        OrderServiceScript script = new(){
            OrderServiceScriptID = viewModel.orderServiceID
        };
        OrderService orderService = new(){
            OrderServiceID = viewModel.orderServiceID,
            OrderServiceName = viewModel.orderServiceName,
            OrderServiceDesc = viewModel.orderServiceDesc,
            WorkScript = script,
        };

        int status = await _orderServiceRepository.AddOrderServiceScriptToOrderServiceAsync(orderService);
        if(status != 0) return BadRequest("add script to order service faild.");
        return Ok();

    }

    //SER014-1 AddOrderServiceWorkScriptsToOrderServiceUI
    //Desc: 将工作脚本添加到OrderService里
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddOrderServiceWorkScriptToOrderSerivceUI([FromBody] OrderServiceScriptSerivceViewModel viewModel){
         var userName = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if(userName == null) return BadRequest("faild.");
        ApplicationUser? user = await _accountRepository.GetUserByUserNameAsync(userName);
        if(user == null){
            userName = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            if(userName == null) return BadRequest("faild.");
            user = await _accountRepository.GetUserByUserNameAsync(userName!);
        }
        if(user == null) return BadRequest("faild.");
        if(user.Role != "Admin") return BadRequest("faild.");
         OrderServiceScript script = new(){
            OrderServiceScriptID = int.Parse(viewModel.scriptID!),
        };
        OrderService orderService = new(){
            OrderServiceID = int.Parse(viewModel.orderServiceID!),
            WorkScript = script,
        };

        int status = await _orderServiceRepository.AddOrderServiceScriptToOrderServiceAsync(orderService);
        if(status != 0) return BadRequest("add script to order service faild.");
        return Ok();

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