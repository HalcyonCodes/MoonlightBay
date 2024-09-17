using MoonlightBay.Model;

namespace MoonlightBay.Web.Models;


public class ChannelOrderResultViewModel{
    public string? code;
    public string? message;
    public ChannelOrderDataViewModel? data;
}

public class ChannelOrderDataViewModel{
    public List<ChannelOrderViewModel>? channel{get; set;}
}

public class ChannelOrderViewModel{
     public string? id { get; set; }
    public string? date{get; set;}
    public string? time{get; set;}
    public string? name{get; set;}
  
}



