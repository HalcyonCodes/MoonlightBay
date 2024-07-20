namespace MoonlightBay.Model;

//订单里的服务内容的主要参数
public class OrderServiceResourceClass{
    public Guid? OrderServiceResoourceClassID{get;set;}
    public OrderServiceResource? OrderServiceResource{get;set;}
    public int? ResourceIntValue{get;set;}
    public string? ResourceStringValue{get;set;}
    public double? ResourceDoubleValue{get;set;}

}