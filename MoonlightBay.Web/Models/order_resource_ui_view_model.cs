

namespace MoonlightBay.Web.Models;

public class OrderResourceUIViewModel{
    public string? id {get; set;}
    public string? bindingCount {get; set;}
    public string? desc{get; set;}
    public string? name {get; set;}
}

public class OrderResourceUiDataViewModel{
    public List<OrderResourceUIViewModel>? orderResources {get; set;}
}

public class UpdateOrderResourceUIDViewModel{
    public List<OrderResourceUIViewModel>? orderResources {get; set;}
    public string? orderServiceId{get; set;}
}

public class OrderResourceUIResoultViewModel{
    public string? code{get; set;}
    public string? message{get; set;}
    public OrderResourceUiDataViewModel? data{get; set;}

}







