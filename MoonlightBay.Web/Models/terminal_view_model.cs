

namespace MoonlightBay.Web.Models;


public class TerminalCreateViewModel{
    public string? TerminalName{get;set;}
    public string? TerminalIP{get;set;}  //terminal所在ip
    public string? MechineID{get;set;}  //机器ID
    public string? TerminalDesc{get;set;}  //机器描述
}



public class TerminalResultViewModel{
    public string? code{get; set;}
    public string? message{get; set;}
    public TerminalsViewModel? data{get; set;}

}

public class TerminalsViewModel{
    public List<TerminalViewModel>? terminals{get; set;}
}

public class TerminalViewModel{
    public string? id {get; set;}
    public string? ip {get; set;}
    public string? name{get; set;}
    public string? desc{get; set;}
    public int? status{get; set;}

}



