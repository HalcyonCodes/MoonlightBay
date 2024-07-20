namespace MoonlightBay.Model;

//订单的服务内容
public class OrderService{
    public int? OrderServiceID{get;set;}
    public string? OrderServiceName{get;set;}
    public List<OrderServiceResource>? OrderServiceResources { get; set; } //所有和服务绑定的资源
    public DateTime? CreatedTime{get;set;}
    public string? OrderServiceDesc{get;set;} //服务说明
}


