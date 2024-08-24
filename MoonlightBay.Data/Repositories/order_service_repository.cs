using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MoonlightBay.Data.Interfaces;
using MoonlightBay.Model;


namespace MoonlightBay.Data.Repositories;



public class OrderServiceRepository(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor,
    IAccountRepository accountRepository
) : IOrderServiceRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IAccountRepository _accountRepository = accountRepository;


    public async Task<int?> AddOrderServiceResourceAsync(OrderServiceResource orderServiceResource){
        OrderServiceResource newOrderServiceResource = orderServiceResource;
        _dbContext.OrderServiceResources.Add(newOrderServiceResource);
        await _dbContext.SaveChangesAsync();
        return newOrderServiceResource.OrderServiceResourceID;
    }
    public async Task<int> DeleteOrderServiceResourceByIDAsync(int? orderServiceResourceID){
        if(orderServiceResourceID == null) return -1;
        OrderServiceResource? orderServiceResource = await _dbContext.OrderServiceResources
        .FirstOrDefaultAsync(t => t.OrderServiceResourceID == orderServiceResourceID);
        if (orderServiceResource == null) return -1;
        //解除service里的resource绑定
        List<OrderService> orderServices = await _dbContext.OrderServices
        .Include(t => t.OrderServiceResources)

        .Where(t => t.OrderServiceResources!.Any(r => r.OrderServiceResource!.OrderServiceResourceID == orderServiceResourceID)).ToListAsync();

        orderServices.ForEach(async q => {
            q.OrderServiceResources ??= [];
            List<OrderServiceResourceClass>? resourceClass = await _dbContext.OrderServiceResourceClasses
            .Where(q => q.OrderServiceResource!.OrderServiceResourceID == q.OrderServiceResource.OrderServiceResourceID).ToListAsync();
            foreach(var k in resourceClass){
                q.OrderServiceResources.Remove(k);
            }
        });

        _dbContext.OrderServiceResources.Remove(orderServiceResource);
        await _dbContext.SaveChangesAsync();
        return 0;

    }

    public async Task<int> UpdateOrderServiceResourceAsync(OrderServiceResource targetOrderServiceResource){
        OrderServiceResource? orderServiceResource = await _dbContext.OrderServiceResources
        .FirstOrDefaultAsync(t => t.OrderServiceResourceID == targetOrderServiceResource.OrderServiceResourceID);
        if(orderServiceResource == null) return -1;
        orderServiceResource.OrderServiceResourceName = targetOrderServiceResource.OrderServiceResourceName;
        orderServiceResource.OrderServiceResourceDesc = targetOrderServiceResource.OrderServiceResourceName;
        _dbContext.OrderServiceResources.Update(orderServiceResource);
        await _dbContext.SaveChangesAsync();
        return 0;
    }

    public async Task<OrderServiceResource?> GetOrderServiceResourceByNameAsync(string orderServiceResourceName){
        OrderServiceResource? orderServiceResource = await _dbContext.OrderServiceResources
        .FirstOrDefaultAsync(t => t.OrderServiceResourceName == orderServiceResourceName);
        if(orderServiceResource == null) return null;
        return orderServiceResource;
    }
    public async Task<OrderServiceResource?> GetOrderServiceResourceByIDAsync(int? orderServiceResourceID){
        if(orderServiceResourceID == null) return null;
        OrderServiceResource? orderServiceResource = await _dbContext.OrderServiceResources
        .FirstOrDefaultAsync(t => t.OrderServiceResourceID == orderServiceResourceID);
        if(orderServiceResource == null) return null;
        return orderServiceResource;
    }
    public async Task<List<OrderServiceResource>?> GetOrderServiceResourcesAsync(){
        
        List<OrderServiceResource>? orderServiceResources = await _dbContext.OrderServiceResources
        .ToListAsync();
        orderServiceResources ??= [];
        return orderServiceResources;
    }

    public async Task<int?> AddOrderServiceAsync(OrderService orderService){
        OrderService newOrderService = new(){
            OrderServiceID = null,
            OrderServiceName = orderService.OrderServiceName,
            OrderServiceDesc = orderService.OrderServiceDesc,
            CreatedTime = DateTime.Now,
            OrderServiceResources = []
        };
        //查看服务名称是否重复
        OrderService? dbOrderService = await _dbContext.OrderServices
        .FirstOrDefaultAsync(t => t.OrderServiceName == newOrderService.OrderServiceName);
        if(dbOrderService != null) return -1;

        _dbContext.OrderServices.Add(newOrderService);
        await _dbContext.SaveChangesAsync();
        return newOrderService.OrderServiceID;
    }

    public async Task<int> DeleteOrderServiceByIDAsync(int? orderServiceID){
        if(orderServiceID == null) return -1;

        OrderService? orderService = await _dbContext.OrderServices
        .Include(q => q.OrderServiceResources)
        .FirstOrDefaultAsync(q => q.OrderServiceID == orderServiceID);

        if(orderService == null) return -1;
        orderService.OrderServiceResources ??= [];
        orderService.OrderServiceResources.Clear();

        //清理附带service的所有order
        List<Order> orders = await _dbContext.Orders
        .Include(t => t.OrderResources)
        .Where(t => t.OrderService == orderService)
        .ToListAsync();

        orders.ForEach(q => {
            //清理订单中的资源
            q.OrderResources ??= [];
            _dbContext.OrderServiceResourceClasses.RemoveRange(q.OrderResources);
            _dbContext.OrderServices.Remove(q.OrderService!);
            _dbContext.Orders.Remove(q);
        });
        
        _dbContext.OrderServices.Remove(orderService);
        await _dbContext.SaveChangesAsync();
        return 0;
    }

    public async Task<int> UpdateOrderServiceAsync(OrderService orderService){
        OrderService? dbOrderService = await _dbContext.OrderServices
        .Include(t => t.OrderServiceResources)
        .FirstOrDefaultAsync(t => t.OrderServiceID == orderService.OrderServiceID);
        if(dbOrderService == null) return -1;
        dbOrderService.OrderServiceName = orderService.OrderServiceName;
        dbOrderService.OrderServiceDesc = orderService.OrderServiceDesc;
        //重新绑定资源
        dbOrderService.OrderServiceResources ??= [];
        dbOrderService.OrderServiceResources.Clear();
        orderService.OrderServiceResources ??= [];
        /*List<OrderServiceResource>? newOrderResources = await _dbContext.OrderServiceResources
        .Where(t => orderService.OrderServiceResources.Any(q => q.OrderServiceResource!.OrderServiceResourceName == t.OrderServiceResourceName))
        .ToListAsync();
        */
        //找出多余的并且删除
        List<OrderServiceResourceClass> elseClass = await _dbContext.OrderServiceResourceClasses
        .Where(q => !orderService.OrderServiceResources.Select(g => g.OrderServiceResoourceClassID).Contains(q.OrderServiceResoourceClassID))
        .ToListAsync();
        
        _dbContext.OrderServiceResourceClasses.RemoveRange(elseClass);

        List<OrderServiceResourceClass> classes = await _dbContext.OrderServiceResourceClasses
        .Include(q => q.OrderServiceResource)
        .Where(q => orderService.OrderServiceResources.Select(g => g.OrderServiceResoourceClassID).Contains(q.OrderServiceResoourceClassID))
        .ToListAsync();

        if(classes.Count != orderService.OrderServiceResources.Count) return -1;
        dbOrderService.OrderServiceResources.Clear();
        foreach(var q in classes){
            dbOrderService.OrderServiceResources.Add(q);
        }
        //dbOrderService.OrderServiceResources.AddRange(newOrderResources);
        await _dbContext.SaveChangesAsync();
        return 0;
    }
    

    public async Task<OrderService?> GetOrderServiceByNameWithResourcesAsync(string orderServiceResourceName){
        OrderService? orderService = await _dbContext.OrderServices
        .Include(t => t.OrderServiceResources)
        .FirstOrDefaultAsync(t => t.OrderServiceName == orderServiceResourceName);
        if(orderService == null) return null;
        return orderService;
    }
    public async  Task<OrderService?> GetOrderServiceByIDWithResourcesAsync(int? orderServiceID){
        if(orderServiceID == null) return null;
        OrderService? orderService = await _dbContext.OrderServices
        .Include(t => t.OrderServiceResources)
        .FirstOrDefaultAsync(t => t.OrderServiceID == orderServiceID);
        if(orderService == null) return null;
        return orderService;

    }
    public async Task<List<OrderService>> GetOrderServicesAsync(){
        //if(_httpContextAccessor.HttpContext == null) return null;
        //ApplicationUser? user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
        //if(user == null) return null;
        List<OrderService>? orderServices = await _dbContext.OrderServices
        .Include(t => t.OrderServiceResources)
        .ToListAsync();
        orderServices ??= [];
        return orderServices;

    }

    public async Task<int> AddOrderServiceResourcesToOrderServiceAsync(OrderService orderService)
    {
        if(orderService.OrderServiceID == null) return -1;
        OrderService? dbOrderService = await _dbContext.OrderServices
        .Include(t => t.OrderServiceResources)
        .FirstOrDefaultAsync(t => t.OrderServiceID == orderService.OrderServiceID);
        if(dbOrderService == null) return -1;
        dbOrderService.OrderServiceResources ??= [];
        orderService.OrderServiceResources ??= [];
        List<OrderServiceResourceClass> resourceClasses = orderService.OrderServiceResources;
        if(dbOrderService.OrderServiceResources.Count - resourceClasses.Count != 0) return -1;
        orderService.OrderServiceResources ??= [];
        List<OrderServiceResourceClass>? oldClass = orderService.OrderServiceResources;
        _dbContext.OrderServiceResourceClasses.AddRange(oldClass);
        _dbContext.OrderServices.Update(dbOrderService);
        await _dbContext.SaveChangesAsync();
        return 0;
        /*
        oldClass ??=[];
        _dbContext.OrderServiceResourceClasses.RemoveRange(oldClass);
        orderService.OrderServiceResources.Clear();
        bool flag = false;
        foreach(var q in orderService.OrderServiceResources){
            OrderServiceResource? newResouce = await _dbContext.OrderServiceResources
            .FirstOrDefaultAsync(w => w.OrderServiceResourceID == q.OrderServiceResource!.OrderServiceResourceID);
            if(newResouce == null){
                flag = true;
                break;
            }
            OrderServiceResourceClass newClass = new(){
                OrderServiceResoourceClassID = Guid.NewGuid(),
                CreatedTime = DateTime.Now,
                ResourceIntValue = q.ResourceIntValue,
                ResourceDoubleValue = q.ResourceDoubleValue,
                ResourceStringValue = q.ResourceStringValue,
                OrderServiceResource = newResouce,
            };
            dbOrderService.OrderServiceResources.Add(newClass);
        }
        if(flag == true){
            return -1;
        }
        await _dbContext.SaveChangesAsync();
        return 0;*/
    }

    public async Task<int> AddOrderServiceScriptToOrderServiceAsync(OrderService orderService){
        if(orderService.OrderServiceID == null) return -1;
        if(orderService.WorkScript == null) return -1;
        if(orderService.WorkScript.OrderServiceScriptID == null) return -1;
        OrderService? dbOrderService = await _dbContext.OrderServices
        .Include(t => t.OrderServiceResources)
        .FirstOrDefaultAsync(t => t.OrderServiceID == orderService.OrderServiceID);
        if(dbOrderService == null) return -1;

        OrderServiceScript? dbOrderServiceScript = await _dbContext.OrderServiceScripts
        .FirstOrDefaultAsync(t => t.OrderServiceScriptID == orderService.WorkScript.OrderServiceScriptID);
        if(dbOrderServiceScript == null) return -1;
        dbOrderService.WorkScript = dbOrderServiceScript;
        _dbContext.OrderServices.Update(dbOrderService);
        await _dbContext.SaveChangesAsync();
        return 0;
    }

    public async Task<int?> AddOrderServiceScriptAsync(OrderServiceScript script){
        OrderServiceScript newOrderServiceScript = new(){
            OrderServiceScriptID = null,
            OrderServiceDesc = script.OrderServiceDesc,
            OrderServiceScriptName = script.OrderServiceScriptName
        };
        _dbContext.OrderServiceScripts.Add(newOrderServiceScript);
        await _dbContext.SaveChangesAsync();
        return newOrderServiceScript.OrderServiceScriptID;

    }
    public async Task<int> DeleteOrderServiceScriptAsync(OrderServiceScript script){
        if(script.OrderServiceScriptID == null) return -1;
        OrderServiceScript? dbScript = await _dbContext.OrderServiceScripts
        .FirstOrDefaultAsync(t => t.OrderServiceScriptID == script.OrderServiceScriptID);
        if(dbScript == null) return -1;
        List<OrderService>? dbOrderServices = await _dbContext.OrderServices
        .Where(t => (t.WorkScript != null) && (t.WorkScript.OrderServiceScriptID == script.OrderServiceScriptID))
        .ToListAsync();
        dbOrderServices ??= [];
        dbOrderServices.ForEach(q =>{
            q.WorkScript = null;
            _dbContext.OrderServices.Update(q);
        });
        _dbContext.OrderServiceScripts.Remove(dbScript);
        await _dbContext.SaveChangesAsync();
        return 0;
    }

    public async Task<int> UpdateOrderServiceSeriptAsync(OrderServiceScript script){
        if(script.OrderServiceScriptID == null) return -1;
        OrderServiceScript? dbOrderScript = await _dbContext.OrderServiceScripts
        .FirstOrDefaultAsync(t => t.OrderServiceScriptID == script.OrderServiceScriptID);
        if(dbOrderScript == null) return -1;
        dbOrderScript.OrderServiceScriptName = script.OrderServiceScriptName;
        dbOrderScript.OrderServiceDesc = script.OrderServiceDesc;
        _dbContext.OrderServiceScripts.Update(dbOrderScript);
        await _dbContext.SaveChangesAsync();
        return 0;
    }

    public async Task<OrderServiceScript?> GetOrderServiceScriptAsync(int? scriptID){
        if(scriptID == null) return null;
        OrderServiceScript? dbOrderServiceScript = await _dbContext.OrderServiceScripts
        .FirstOrDefaultAsync(t => t.OrderServiceScriptID == scriptID);
        if(dbOrderServiceScript == null) return null;
        return dbOrderServiceScript;
    }

    public async Task<List<OrderServiceScript>?> GetOrderServiceScriptsAsync(){
        List<OrderServiceScript>? scripts = await _dbContext.OrderServiceScripts
        .ToListAsync();
        scripts ??= [];
        return scripts;
    }

    public async Task<int> DeleteOrderServiceClassAsync(Guid? orderServiceClassID)
    {
        OrderServiceResourceClass? dbResourceClass = await _dbContext.OrderServiceResourceClasses
        .FirstOrDefaultAsync(q => q.OrderServiceResoourceClassID == orderServiceClassID);
        if(dbResourceClass == null) return -1;
        _dbContext.OrderServiceResourceClasses.Remove(dbResourceClass);
        await _dbContext.SaveChangesAsync();
        return 0;
    }

   
}