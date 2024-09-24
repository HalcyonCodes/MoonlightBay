using MoonlightBay.Data.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MoonlightBay.Model;

using MoonlightBay.Web.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Text;

namespace MoonlightBay.Web.Controllers;

/// <summary>
/// 终端控制器
/// </summary>

[Route("api/v1/[controller]/[action]")]
public class TermianlController(
    ILoggerFactory loggerFactory,
    UserManager<ApplicationUser> userManager,
    ITerminalRepository terminalRepository
    
    ) : Controller
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<AccountController>();
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ITerminalRepository _terminalRepository = terminalRepository;
    

    //TER001: TerminalInit
    //Desc: 初始化终端
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> TerminalInit([FromBody] TerminalCreateViewModel viewModel)
    {
       
        List<Terminal>? terminals = await _terminalRepository.GetUserTerminalsAsync();
        if(terminals == null) return BadRequest("not login.");
        terminals ??= [];
        if(terminals.Count != 0) return Ok();
        Terminal newTerminal = new(){
            TerminalID = Guid.NewGuid(),
            MechineID = viewModel.MechineID,
            TerminalName = viewModel.TerminalName,
            TerminalIP = viewModel.TerminalIP,
            TerminalDesc = viewModel.TerminalDesc,
            TerminalStatus = -1,
        };
        Guid? newTerminalID = await _terminalRepository.AddAysnc(newTerminal);
        if(newTerminalID == null) return BadRequest("add new terminal failed.");
        int result = await _terminalRepository.TerminalInitAsync(newTerminalID);
        if(result == -1) return BadRequest("new terminal init failed");
        return Ok();
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetTerminals([FromQuery] int pageIndex){
        List<Terminal>? terminals = await _terminalRepository.GetTerminalsAsync(pageIndex);
        TerminalResultViewModel resultViewModel = new(){
            code = "200",
            message = "",
            data = new TerminalsViewModel()
        };
        resultViewModel.data.terminals = [];
        TerminalViewModel temp;
        foreach (Terminal terminal in terminals){
            temp = new(){
                id = terminal.TerminalID.ToString(),
                name = terminal.TerminalName,
                ip = terminal.TerminalIP,
                desc = terminal.TerminalDesc,
                status = terminal.TerminalStatus,
            };
            resultViewModel.data.terminals.Add(temp);
        }
        return Ok(resultViewModel);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SetTerminalStatus([FromBody] TerminalStatusViewModel viewModel){

        Terminal? terminals = await _terminalRepository.GetTerminalByIDAsync(Guid.Parse(viewModel.terminalID));
        if(terminals == null) return BadRequest("terminal not found.");
        terminals.TerminalStatus = viewModel.status;
        await _terminalRepository.UpdateAsync(terminals);
        return Ok();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> UpdateTerminal([FromBody] UpdateTerminalUIViewModel viewModel){
        Terminal? terminal = await _terminalRepository.GetTerminalByIDAsync(Guid.Parse(viewModel.terminalID!));
        if(terminal == null) return BadRequest("terminal not found.");
        terminal.TerminalName = viewModel.terminalName;
        terminal.TerminalIP = viewModel.terminalIP;
        terminal.TerminalDesc = viewModel.desc;
        int status = await _terminalRepository.UpdateAsync(terminal);
        if(status == -1) return BadRequest("update failed.");
        return Ok();
    }

    




}