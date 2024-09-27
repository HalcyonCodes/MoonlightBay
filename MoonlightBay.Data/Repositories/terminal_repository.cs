
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MoonlightBay.Data.Interfaces;
using MoonlightBay.Model;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace MoonlightBay.Data.Repositories;


public class TerminalRepository(ApplicationDbContext dbContext
, UserManager<ApplicationUser> userManager
, IHttpContextAccessor httpContextAccessor
    ,IAccountRepository accountRepository
) : ITerminalRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IHttpContextAccessor _contextAccessor = httpContextAccessor;
    private readonly IAccountRepository _accountRepository = accountRepository;

    public async Task<Guid?> AddAysnc(Terminal terminal)
    {
        _dbContext.Terminals.Add(terminal);
        await _dbContext.SaveChangesAsync();
        return terminal.TerminalID;
    }

    public async Task<int> DeleteAsync(Guid terminalID)
    {
        Terminal? terminal = await _dbContext.Terminals
        .Include(t => t.User)
        .Include(t => t.OrderChannels!)
        .ThenInclude(t => t.Orders!)
        .ThenInclude(t => t.OrderResources)
        .FirstOrDefaultAsync(t => t.TerminalID  == terminalID);
        if(terminal == null) return -1;

        
        List<Order> channelOrders = terminal.OrderChannels!.SelectMany(t => t.Orders!).ToList();
        //清理terminal中的订单资源
        channelOrders.ForEach(q => {
            q.OrderResources!.Clear();
        });
        _dbContext.OrderServiceResourceClasses.RemoveRange(terminal.OrderChannels!.SelectMany(t => t.Orders!).SelectMany(q => q.OrderResources!).ToList());
        //清理order
        List<OrderChannel>? orderChannels = terminal.OrderChannels;
        orderChannels ??= [];
        orderChannels.ForEach(q => {
            q.Orders!.Clear();
        });
        _dbContext.Orders.RemoveRange(channelOrders);
        //清理orderChannel
        terminal.OrderChannels ??= [];
        terminal.OrderChannels.Clear();
        _dbContext.OrderChannels.RemoveRange(orderChannels);
        //清理terminal
        _dbContext.Terminals.Remove(terminal);
        await _dbContext.SaveChangesAsync();
        return 0;
    }

    public async Task<List<Terminal>?> GetUserTerminalsAsync()
    {
        //if(_contextAccessor.HttpContext == null) return null;
        //ApplicationUser? user = await _userManager.GetUserAsync(_contextAccessor.HttpContext.User);
        var userName = _contextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        ApplicationUser? user = await _accountRepository.GetUserByUserNameAsync(userName);
        if (user == null)
        {
            userName = _contextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            user = await _accountRepository.GetUserByUserNameAsync(userName!);
        }

        if (user == null) return null;
        List<Terminal> terminals = await _dbContext.Terminals
        .Include(q => q.OrderChannels)!
        .ThenInclude(q => q.Orders)!
        .ThenInclude(q => q.OrderResources)
        .Include(q => q.OrderChannels)!
        .ThenInclude(q => q.Orders)!
        .ThenInclude(q => q.OrderService)
        .ThenInclude(q => q!.OrderServiceResources)
        .Where(t => t.User!.Id == user.Id)
        .ToListAsync();
        terminals ??= [];
        return terminals;
    }

    public async Task<int> TerminalInitAsync(Guid? terminalID)
    {
        if(terminalID == null) return -1;
        Terminal? terminal = await _dbContext.Terminals
        .Include(t => t.OrderChannels)
        .SingleOrDefaultAsync(t => t.TerminalID == terminalID);
        if(terminal == null) return -1;
        if(terminal.User != null) return -1;
        terminal!.OrderChannels ??= [];

        //添加订单频道
        if(terminal.OrderChannels.Count != 0) return -1;
        for(int i = 0; i<=4 ; i++){
            OrderChannel orderChannel = new(){
                OrderChannelID = Guid.NewGuid(),
                OrderChannelLevel = i,
                Orders = []
            };
            _dbContext.OrderChannels.Add(orderChannel);
            terminal.OrderChannels.Add(orderChannel);
        }

        //添加用户
        if(_contextAccessor.HttpContext == null) return -1;
        ApplicationUser? user = await _userManager.GetUserAsync(_contextAccessor.HttpContext.User);
        terminal.User = user;

        _dbContext.Terminals.Update(terminal);
        await _dbContext.SaveChangesAsync();
        return 0;
    }

    public async Task<int> UpdateAsync(Terminal updateTerminal)
    {
        Terminal? terminal = await _dbContext.Terminals
        .SingleOrDefaultAsync(t => t.TerminalID == updateTerminal.TerminalID);
        if (terminal == null) return -1;
        terminal.TerminalName = updateTerminal.TerminalName;
        terminal.MechineID = updateTerminal.MechineID;
        terminal.TerminalIP = updateTerminal.TerminalIP;
        terminal.TerminalStatus = updateTerminal.TerminalStatus;
        _dbContext.Terminals.Update(terminal);
        await _dbContext.SaveChangesAsync();
        return 0;
    }

    public async Task<List<Terminal>?> GetTerminalsAsync(int pageIndex){
        List<Terminal>? terminals = await _dbContext.Terminals
        .Include(q => q.OrderChannels)!
        .ThenInclude(q => q.Orders)!
        .ThenInclude(q => q.OrderResources)
        .Include(q => q.OrderChannels)!
        .ThenInclude(q => q.Orders)!
        .ThenInclude(q => q.OrderService)
        .ThenInclude(q => q!.OrderServiceResources)
        .Skip(pageIndex  * 12)
        .Take(12)
        .ToListAsync();
        return terminals;
    }

    public async Task<Terminal?> GetTerminalByIDAsync(Guid terminalID){
        Terminal? terminal = await _dbContext.Terminals
        .Include(q => q.OrderChannels)!
        .ThenInclude(q => q.Orders)!
        .ThenInclude(q => q.OrderResources)
        .Include(q => q.OrderChannels)!
        .ThenInclude(q => q.Orders)!
        .ThenInclude(q => q.OrderService)
        .ThenInclude(q => q!.OrderServiceResources)
        .SingleOrDefaultAsync(q => q.TerminalID == terminalID);
        return terminal;
    }

    
}