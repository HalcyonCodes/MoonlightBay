using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MoonlightBay.Data.Interfaces;
using MoonlightBay.Model;
using System.Security.Claims;


namespace MoonlightBay.Data.Repositories;



public class OrderRepository(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor,
    IAccountRepository accountRepository
) : IOrderRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IAccountRepository _accountRepository = accountRepository;

    
    public async Task<Guid?> AddOrderAsync(Order order)
    {
        if (order.SourceTerminal == null) return null;
        if (order.TargetTerminal == null) return null;
        if (order.Status == null) return null;
        if (order.OrderService == null) return null;
        //if (order.OrderService.WorkScript == null) return null;
        order.OrderResources ??= [];

        Order newOrder = new()
        {
            OrderID = Guid.NewGuid(),
            CreatedTime = DateTime.Now,
            Status = Order.OrderStatus.waiting
        };
        newOrder.OrderResources ??= [];

        //public OrderService? OrderService{get; set;} //订单内容
        //public List<OrderServiceResourceClass>? OrderResources{get; set;} //订单资源和参数

        if (order.SourceTerminal == null) return null;
        Terminal? sourceTerminal = await _dbContext.Terminals.FirstOrDefaultAsync(t => t.TerminalID == order.SourceTerminal.TerminalID);
        if (sourceTerminal == null) return null;
        Terminal? targetTerminal = await _dbContext.Terminals.FirstOrDefaultAsync(t => t.TerminalID == order.SourceTerminal.TerminalID);
        if (targetTerminal == null) return null;
        OrderService? orderService = await _dbContext.OrderServices.FirstOrDefaultAsync(t => t.OrderServiceID == order.OrderService.OrderServiceID);
        if (orderService == null) return null;
        newOrder.SourceTerminal = sourceTerminal;
        newOrder.TargetTerminal = targetTerminal;
        newOrder.OrderService = orderService;

        //处理订单参数
        foreach(var q in order.OrderResources){
            if(q.OrderServiceResource == null) break;
            if(q.OrderServiceResource.OrderServiceResourceID == null) break;
            OrderServiceResource? resource = await _dbContext.OrderServiceResources
            .FirstOrDefaultAsync(q => q.OrderServiceResourceID == q.OrderServiceResourceID);

            OrderServiceResourceClass newOrderResource = new(){
                OrderServiceResoourceClassID = Guid.NewGuid(),
                OrderServiceResource = resource,
                ResourceIntValue = q.ResourceIntValue,
                ResourceStringValue = q.ResourceStringValue,
                ResourceDoubleValue = q.ResourceDoubleValue,
                CreatedTime = DateTime.Now,
            };
            _dbContext.OrderServiceResourceClasses.Add(newOrderResource);
            newOrder.OrderResources.Add(newOrderResource);
        }

        

        _dbContext.Orders.Add(newOrder);
        await _dbContext.SaveChangesAsync();
        return newOrder.OrderID;
    }

    public async Task<int> DeleteOrderAsync(Order order)
    {
        if(order.OrderID == null) return -1;
        Order? dbOrder = await _dbContext.Orders
        .Include(t => t.OrderResources)
        .FirstOrDefaultAsync(t => t.OrderID == order.OrderID);
        if(dbOrder == null) return -1;

        //从Channel里解除所有包含的order。
        List<OrderChannel> orderChannels = await _dbContext.OrderChannels
        .Where(t => t.Orders!.Any(q => q.OrderID == order.OrderID))
        .ToListAsync();
        
        orderChannels.ForEach(q =>{
            q.Orders ??= [];
            q.Orders.Remove(dbOrder);
        });

        //移除所有orderResourceClass资源
        dbOrder.OrderResources ??= [];
        List<OrderServiceResourceClass> resourceClasses = dbOrder.OrderResources;
        _dbContext.OrderServiceResourceClasses.RemoveRange(resourceClasses);
        dbOrder.OrderResources.Clear();

        //移除order
        _dbContext.Orders.Remove(order);
        await _dbContext.SaveChangesAsync();
        return 0;
    }

    public async Task<List<OrderChannel>?> GetOrderChannelByTerminalIDAsync(Guid? terminalID)
    {
        Terminal? dbTerminal = await _dbContext.Terminals
        .Include(t => t.OrderChannels!)
        .ThenInclude(t => t.Orders!)
        .ThenInclude(t => t.OrderResources)
        .Include(t => t.OrderChannels!)
        .ThenInclude(t => t.Orders!)
        .ThenInclude(q => q.OrderService)
        .ThenInclude(q => q.WorkScript)
        .FirstOrDefaultAsync( t => t.TerminalID == terminalID);
        if(dbTerminal == null) return null;
        dbTerminal.OrderChannels ??= [];
        
        List<OrderChannel> orderChannels = [.. dbTerminal.OrderChannels.OrderBy(t => t.OrderChannelLevel)];
        return orderChannels;
    }

    public async Task<int> UpdateOrderAsync(Order order)
    {
        Order? dbOrder = await _dbContext.Orders
        .Include(t => t.OrderResources)
        .Include(q => q.OrderService)
       
        .FirstOrDefaultAsync(t => t.OrderID == order.OrderID);

        if(dbOrder == null) return -1;
        if(order.Status == null) return -1;
        dbOrder.Status = order.Status;
        
        if(order.SourceTerminal == null || order.SourceTerminal.TerminalID == null) return -1;
        if(order.TargetTerminal == null || order.TargetTerminal.TerminalID == null) return -1;

        Terminal? orderSourceTerminal = await _dbContext.Terminals
        .FirstOrDefaultAsync(t => t.TerminalID == order.SourceTerminal.TerminalID);
        if(orderSourceTerminal == null) return -1;

        Terminal? orderTargetTerminal = await _dbContext.Terminals
        .FirstOrDefaultAsync(t => t.TerminalID == order.TargetTerminal.TerminalID);
        if(orderTargetTerminal == null) return -1;

        if(order.OrderService == null) return -1;
        if(order.OrderService.OrderServiceID == null) return -1;
        OrderService? orderService = await _dbContext.OrderServices
        .FirstOrDefaultAsync(t => t.OrderServiceID == order.OrderService.OrderServiceID);
        if(orderService == null) return -1;

        order.OrderResources ??= [];

        //移除OrderResourceClass，创建新的orderResourceClass
        dbOrder.OrderResources ??= [];
        List<OrderServiceResourceClass> dbResourceClasses = dbOrder.OrderResources;
        dbOrder.OrderResources.Clear();
        _dbContext.OrderServiceResourceClasses.RemoveRange(dbResourceClasses);
        
        bool flag = false;
        foreach(var t in order.OrderResources){
            OrderServiceResourceClass newResourceClass = new(){
                OrderServiceResoourceClassID = Guid.NewGuid(),
                CreatedTime = DateTime.Now,
                ResourceIntValue = t.ResourceIntValue,
                ResourceStringValue = t.ResourceStringValue,
                ResourceDoubleValue = t.ResourceDoubleValue
            };
            if(t.OrderServiceResource == null) {
                flag = true;
                break;
            }
            if(t.OrderServiceResource.OrderServiceResourceID == null) {
                flag = true;
                break;
            }

            OrderServiceResource? dbResource = await _dbContext.OrderServiceResources
            .FirstOrDefaultAsync(q => q.OrderServiceResourceID == t.OrderServiceResource.OrderServiceResourceID);
            
            if(dbResource == null){
                flag = true;
                break;
            }
            newResourceClass.OrderServiceResource = dbResource;
            dbOrder.OrderResources.Add(newResourceClass);
        }
        
        if(flag == true) return -1;
        _dbContext.Orders.Update(dbOrder);
        await _dbContext.SaveChangesAsync();
        return 0;
    }

    public async Task<int> UpdateOrderStatusAsync(Order order)
    {
        Order? dbOrder = await _dbContext.Orders
        .FirstOrDefaultAsync(t => t.OrderID == order.OrderID);
        if(dbOrder == null) return -1;
        if(order.Status == null) return -1;
        dbOrder.Status = order.Status;
        _dbContext.Orders.Update(dbOrder);
        await _dbContext.SaveChangesAsync(); 
        return 0;
     }

    public async Task<int> AddOrderToOrderChannelAsync(Guid orderID, int channelLevel)
    {
        
        var userName = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
     
        ApplicationUser? user = await _accountRepository.GetUserByUserNameAsync(userName);
        if (user == null)
        {
            userName = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            user = await _accountRepository.GetUserByUserNameAsync(userName!);
        }


        if (user == null) return -1;
        Terminal? terminal = await _dbContext.Terminals
        .Include(t => t.OrderChannels)!
        .ThenInclude(t => t.Orders)
        .FirstOrDefaultAsync(t => t.User == user);
        if(terminal == null) return -1;
        terminal.OrderChannels ??= [];
        OrderChannel? orderChannel = terminal.OrderChannels
        .FirstOrDefault(t => t.OrderChannelLevel == channelLevel);
        if(orderChannel == null) return -1;
        Order? order = await _dbContext.Orders.FirstOrDefaultAsync(t => t.OrderID == orderID);
        if(order == null) return -1;
        orderChannel.Orders ??= [];
        int index = orderChannel.Orders.Count;
        order.OrderIndex = index;
        _dbContext.Orders.Update(order);
        orderChannel.Orders.Add(order);
        await _dbContext.SaveChangesAsync();
        return 0;
    }

    //从所有终端订单栈里得到弹出优先度最高的订单
    public async Task<Order?> GetOrderAsync()
    {

        var userName = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        ApplicationUser? user = await _accountRepository.GetUserByUserNameAsync(userName);
        if (user == null)
        {
            userName = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            user = await _accountRepository.GetUserByUserNameAsync(userName!);
        }



        Terminal? terminal = await _dbContext.Terminals
        .Include(t => t.OrderChannels)!
        .ThenInclude(t => t.Orders)!
        .ThenInclude(t => t.OrderResources)
        .Include(t => t.OrderChannels)!
        .ThenInclude(t => t.Orders)!
        .ThenInclude(t => t.OrderService)
        .ThenInclude(t => t!.OrderServiceResources)!
        .ThenInclude(q => q.OrderServiceResource)
        .Include(t => t.OrderChannels)!
        .ThenInclude(t => t.Orders)!
        .ThenInclude(t => t.OrderService)
        .ThenInclude(q => q.WorkScript)
        .FirstOrDefaultAsync(t => t.User == user);
        if(terminal == null) return null;
        terminal.OrderChannels ??= [];
        List<OrderChannel>? orderChannel = [.. terminal.OrderChannels.OrderBy(t => t.OrderChannelLevel)];
        Order? targetOrder = null;
        bool flag = false;
        foreach(var q in orderChannel){
            q.Orders ??= [];
            if(q.Orders.Count == 0)
            {}else{
                targetOrder = q.Orders.OrderBy(t => t.OrderIndex).FirstOrDefault();
                if(targetOrder == null){
                    flag = true;
                    break;
                }
                break;
            }
        }
        if(flag){return null;}
        return targetOrder;
    }

    //获取所有订单通道中的订单
    public async Task<List<OrderChannel>?> GetOrderChannelsAsync(){


        var userName = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        ApplicationUser? user = await _accountRepository.GetUserByUserNameAsync(userName);
        if (user == null)
        {
            userName = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            user = await _accountRepository.GetUserByUserNameAsync(userName!);
        }

        Terminal? terminal = await _dbContext.Terminals
        .Include(t => t.OrderChannels)!
        .ThenInclude(t => t.Orders)!
        .ThenInclude(t => t.OrderResources)!
        .Include(t => t.OrderChannels)!
        .ThenInclude(t => t.Orders)!
        .ThenInclude(t => t.OrderService)
        .ThenInclude(t => t!.OrderServiceResources)!
        .ThenInclude(q => q.OrderServiceResource)
         .Include(t => t.OrderChannels)!
        .ThenInclude(t => t.Orders)!
        .ThenInclude(t => t.OrderService)
        .ThenInclude(q => q.WorkScript)
        .FirstOrDefaultAsync(t => t.User == user);

        if(terminal == null) return[];

        List<OrderChannel>? orderChannels = terminal.OrderChannels;

        orderChannels ??= [];
        return orderChannels;
    }

    public async Task<Order?> GetOrderByIDAsync(Guid? orderID)
    {
        Order? dbOrder = await _dbContext.Orders
        .Include(t => t.OrderResources)
        .Include(t => t.OrderService)
        .ThenInclude(t => t!.OrderServiceResources)
        .Include(t => t.OrderService)
        .ThenInclude(q => q.WorkScript)
        .FirstOrDefaultAsync(t => t.OrderID == orderID);
        if(dbOrder == null) return null;
        return dbOrder;
    }
    
}