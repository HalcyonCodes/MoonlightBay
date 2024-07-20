


namespace MoonlightBay.Web.Models;


public class OrderServiceResourceViewModel{
    public int? orderServiceResourceID;
    public string? orderServiceResourceName;
    public string? orderServiceResourceDesc; //资源说明

}

public  class OrderServiceResourcesResultViewModel{
    public string? code;
    public string? message;
    public List<OrderServiceResourceViewModel>? orderServiceResources;

 
}

public class OrderServiceViewModel{
    public int? orderServiceID;
    public string? orderServiceName;
    public List<OrderServiceResourceViewModel>? orderServiceResources;
    public DateTime? createdTime;
    public string? orderServiceDesc;
}

public class OrderServicesResultViewModel{
    public string? code;
    public string? message;
    public List<OrderServiceViewModel>? orderServices;
}