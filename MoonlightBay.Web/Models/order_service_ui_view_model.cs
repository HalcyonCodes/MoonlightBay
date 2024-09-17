namespace MoonlightBay.Web.Models;


public class OrderServiceUIResoultViewModel{
    public string? code{get; set;}
    public string? message{get; set;}
    public OrderServiceUIDataViewModel? data{get; set;}
}



public class OrderServiceUIDataViewModel{
    public List<OrderServiceUIViewModel>? orderServices {get; set;}

}



public class OrderServiceUIViewModel{
    public string? id {get; set;}
    public string? desc{get; set;}
    public string? name{get; set;}
}


