namespace MoonlightBay.Model;


public class Terminal{
    public Guid? TerminalID{get;set;}

    public string? TerminalName{get;set;}

    public ApplicationUser? User{get;set;}  //terminal的所有者

    public string? TerminalIP{get;set;}  //terminal所在ip

    public string? MechineID{get;set;}  //机器ID

    public List<OrderChannel>? OrderChannels{get;set;}
}